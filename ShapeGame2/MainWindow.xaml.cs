/////////////////////////////////////////////////////////////////////////
//
// This module contains code to do Kinect NUI initialization,
// processing, displaying players on screen, and sending updated player
// positions to the game portion for hit testing.
//
// Copyright © Microsoft Corporation.  All rights reserved.  
// This code is licensed under the terms of the 
// Microsoft Kinect for Windows SDK (Beta) from Microsoft Research 
// License Agreement: http://research.microsoft.com/KinectSDK-ToU
//
/////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Research.Kinect.Nui;
using ShapeGame_Utils;
using System.Timers;

// Since the timer resolution defaults to about 10ms precisely, we need to
// increase the resolution to get framerates above between 50fps with any
// consistency.
using System.Runtime.InteropServices;



namespace ShapeGame2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        const int TimerResolution = 2;  // ms
        const int NumIntraFrames = 3;
        const int MaxShapes = 80;
        const double MaxFramerate = 50;
        const double MinFramerate = 15;
        const double MinShapeSize = 12;
        const double MaxShapeSize = 90;
        const double DefaultDropRate = 2.5;
        const double DefaultDropSize = 32.0;
        const double DefaultDropGravity = 1.0;

        //FourLineFish fourLineFish;
        

        bool GameStarted = false;

        double swimDistance = 0; // total distance the fish has already moved
        public SerialConnector serialWindow;
        //List<SingleVortex> redVortices = new List<SingleVortex>();
        //System.Timers.Timer redVortexTimer;
        SimpleJoystick joystick;
        Vortices vortices = new Vortices(Dispatcher.CurrentDispatcher);

        int countdownValue = 60;
        System.Timers.Timer countdownTimer = new System.Timers.Timer(1000);
        private void countdownTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(CountDown));
        }
        private void CountDown()
        {
            if (countdownValue > 0)
            {
                countdownValue--;
                countdownLabel.Content = countdownValue;
            }
            else
            {
                GameStarted = false;
                countdownTimer.Enabled = false;
                vortices.StopFlow();
                StartButton.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            if (Runtime.Kinects.Count > 0)
                nui = Runtime.Kinects[0]; // new style of opening Kinects, instead of "nui = new Runtime();"

            // Restore window state to that last used
            Rect bounds = Properties.Settings.Default.PrevWinPosition;
            if (bounds.Right != bounds.Left)
            {
                this.Top = bounds.Top;
                this.Left = bounds.Left;
                this.Height = bounds.Height;
                this.Width = bounds.Width;
            }
            this.WindowState = (WindowState)Properties.Settings.Default.WindowState;
            //fourLineFish = this.FindName("UCFish") as FourLineFish;
            
            // find a joystick
            try
            {
                joystick = new SimpleJoystick();
            }
            catch (Exception)
            {


            }

            countdownTimer.Elapsed += new ElapsedEventHandler(countdownTimerElapsed);
            

            serialWindow = (ShapeGame2.SerialConnector)App.Current.Windows[0];
        }

        double dropRate = DefaultDropRate;
        double dropSize = DefaultDropSize;
        double dropGravity = DefaultDropGravity;
        DateTime lastFrameDrawn = DateTime.MinValue;
        DateTime predNextFrame = DateTime.MinValue;
        double actualFrameTime = 0;

        // Player(s) placement in scene (z collapsed):
        Rect playerBounds;
        Rect screenRect;

        double targetFramerate = MaxFramerate;
        int frameCount = 0;
        bool runningGameThread = false;
        bool nuiInitialized = false;
        FallingThings fallingThings = null;

        SoundPlayer popSound = new SoundPlayer();
        SoundPlayer hitSound = new SoundPlayer();
        SoundPlayer squeezeSound = new SoundPlayer();

        Runtime nui;


        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.SkeletonFrame;
            int iSkeleton = 0;

            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                if (SkeletonTrackingState.Tracked == data.TrackingState)
                {

                    double angle = getFishAngle(data.Joints);
                    //debugLabelCenter.Content = angle;
                    //fourLineFish.TurnFish(angle);
                    fish1.TurnFish(angle);
                     //seleton.Children.Add(getFishBody(data.Joints, brush));
                    serialWindow.turnFish(angle);
                }
                iSkeleton++;
            } // for each skeleton
        }

        private double getFishAngle(Microsoft.Research.Kinect.Nui.JointsCollection joints)
        {

            Point a = getDisplayPosition(joints[JointID.Head]);
            Point b = getDisplayPosition(joints[JointID.Spine]);
            Point c = Average(
            getDisplayPosition(joints[JointID.AnkleLeft]),
            getDisplayPosition(joints[JointID.AnkleRight]));

            System.Windows.Vector v1 = new System.Windows.Vector(c.X-b.X,c.Y - b.Y);
            System.Windows.Vector v2 = new System.Windows.Vector(a.X - b.X, a.Y - b.Y);

            return AngleConstrain (- NormalizeAngle(180.0 - System.Windows.Vector.AngleBetween(v1, v2)), -30,30);
        }
       public static Double AngleConstrain(double angle, double Min, double Max)
        {
            return Math.Min(Math.Max(angle, Min), Max);
        }
       public static Double NormalizeAngle(double angle)
        {
            return (( (angle%360 + 180) % 360 ) -180);
        }
       public static Point Average(Point a, Point b)
        {

            int dx = (int)((a.X - b.X) / 2);
            int dy = (int)((a.Y - b.Y) / 2);


            return new Point(a.X - dx, a.Y - dy);
        }

       private Point getDisplayPosition(Joint joint)
        {
            float depthX, depthY;
            nui.SkeletonEngine.SkeletonToDepthImage(joint.Position, out depthX, out depthY);
            depthX = depthX * 320; //convert to 320, 240 space
            depthY = depthY * 240; //convert to 320, 240 space
            int colorX, colorY;
            ImageViewArea iv = new ImageViewArea();
            // only ImageResolution.Resolution640x480 is supported at this point
            nui.NuiCamera.GetColorPixelCoordinatesFromDepthPixel(ImageResolution.Resolution640x480, iv, (int)depthX, (int)depthY, (short)0, out colorX, out colorY);

            // map back to skeleton.Width & skeleton.Height
            return new Point((int)(playfield.ActualWidth * colorX / 640.0), (int)(playfield.ActualHeight * colorY / 480));
        }
        

        private bool InitializeNui()
        {
            UninitializeNui();
            if (nui == null)
                return false;
            try
            {
                nui.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseColor);
            }
            catch (Exception _Exception)
            {
                Console.WriteLine(_Exception.ToString());
                return false;
            }

            nui.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.DepthAndPlayerIndex);
            nui.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
            nui.SkeletonEngine.TransformSmooth = true;
            nuiInitialized = true;
            return true;
        }

        private void UninitializeNui()
        {
            if ((nui != null) && (nuiInitialized))
                nui.Uninitialize();
            nuiInitialized = false;
        }

        private void Playfield_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePlayfieldSize();
        }

        private void UpdatePlayfieldSize()
        {
            // Size of player wrt size of playfield, putting ourselves low on the screen.
            screenRect.X = 0;
            screenRect.Y = 0;
            screenRect.Width = playfield.ActualWidth;
            screenRect.Height = playfield.ActualHeight;
            //Canvas.SetLeft(fourLineFish, screenRect.Width / 2 - 150);
            vortices.screenResized(playfield.ActualWidth, playfield.ActualHeight);

            BannerText.UpdateBounds(screenRect);

            playerBounds.X = 0;
            playerBounds.Width = playfield.ActualWidth;
            playerBounds.Y = playfield.ActualHeight * 0.2;
            playerBounds.Height = playfield.ActualHeight * 0.75;

            Rect rFallingBounds = playerBounds;
            rFallingBounds.Y = 0;
            rFallingBounds.Height = playfield.ActualHeight;
            if (fallingThings != null)
            {
                fallingThings.SetBoundaries(rFallingBounds);
            }
        }

        private void Window_Loaded(object sender, EventArgs e)
        {
            playfield.ClipToBounds = true;

            fallingThings = new FallingThings(MaxShapes, targetFramerate, NumIntraFrames);

            UpdatePlayfieldSize();

            fallingThings.SetGravity(dropGravity);
            fallingThings.SetDropRate(dropRate);
            fallingThings.SetSize(dropSize);
            fallingThings.SetPolies(PolyType.All);
            fallingThings.SetGameMode(FallingThings.GameMode.Off);

            if ((nui != null) && InitializeNui())
            {
                //nui.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_ColorFrameReady);
                nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
            }
            else
            {
                BannerText.NewBanner(Properties.Resources.NoKinectError, screenRect, false, Color.FromArgb(90, 255, 255, 255));
            }

            popSound.Stream = Properties.Resources.Pop_5;
            hitSound.Stream = Properties.Resources.Hit_2;
            squeezeSound.Stream = Properties.Resources.Squeeze;

            popSound.Play();

            Win32.timeBeginPeriod(TimerResolution);
            var gameThread = new Thread(GameThread);
            gameThread.SetApartmentState(ApartmentState.STA);
            gameThread.Start();

            FlyingText.NewFlyingText(screenRect.Width / 30, new Point(screenRect.Width / 2, screenRect.Height / 2), "Shapes!");
        }

        private void GameThread()
        {
            runningGameThread = true;
            predNextFrame = DateTime.Now;
            actualFrameTime = 1000.0 / targetFramerate;

            //redVortexTimer = new System.Timers.Timer(4500);
            //redVortexTimer.Elapsed += new ElapsedEventHandler( NewRedVortex);
            //redVortexTimer.Enabled = true;

            // Try to dispatch at as constant of a framerate as possible by sleeping just enough since
            // the last time we dispatched.
            while (runningGameThread)
            {
                // Calculate average framerate.  
                DateTime now = DateTime.Now;
                if (lastFrameDrawn == DateTime.MinValue)
                    lastFrameDrawn = now;
                double ms = now.Subtract(lastFrameDrawn).TotalMilliseconds;
                actualFrameTime = actualFrameTime * 0.95 + 0.05 * ms;
                lastFrameDrawn = now;

                // Adjust target framerate down if we're not achieving that rate
                frameCount++;
                if (((frameCount % 100) == 0) && (1000.0 / actualFrameTime < targetFramerate * 0.92))
                    targetFramerate = Math.Max(MinFramerate, (targetFramerate + 1000.0 / actualFrameTime) / 2);

                if (now > predNextFrame)
                    predNextFrame = now;
                else
                {
                    double msSleep = predNextFrame.Subtract(now).TotalMilliseconds;
                    if (msSleep >= TimerResolution)
                        Thread.Sleep((int)(msSleep + 0.5));
                }
                predNextFrame += TimeSpan.FromMilliseconds(1000.0 / targetFramerate);

                Dispatcher.Invoke(DispatcherPriority.Send,
                    new Action<int>(HandleGameTimer), 0);
            }
        }

        private void HandleGameTimer(int param)
        {
            if (joystick != null)
            {
                double angle = joystick.State.X * 30 / 100;
                debugLabelCenter.Content = 1000/actualFrameTime;
                //fourLineFish.TurnFish(angle);
                fish1.TurnFish(angle);
                serialWindow.turnFish(angle);
            }
            //else
            //{
            //    debugLabelCenter.Content = angleSlider.Value;
            //    fourLineFish.TurnFish((double)angleSlider.Value);
            //}
            // Every so often, notify what our actual framerate is
            if ((frameCount % 100) == 0)
                fallingThings.SetFramerate(1000.0 / actualFrameTime);

            //// Advance animations, and do hit testing.
            //for (int i = 0; i < NumIntraFrames; ++i)
            //{
            //    foreach (var pair in players)
            //    {
            //        HitType hit = fallingThings.LookForHits(pair.Value.segments, pair.Value.getId());
            //        if ((hit & HitType.Squeezed) != 0)
            //            squeezeSound.Play();
            //        else if ((hit & HitType.Popped) != 0)
            //            popSound.Play();
            //        else if ((hit & HitType.Hand) != 0)
            //            hitSound.Play();
            //    }
            //    fallingThings.AdvanceFrame();
            //}

            updateDistance(); // in top right corner

            // Draw new Wpf scene by adding all objects to canvas
            // FourLineFish tmp = fourLineFish;
            playfield.Children.Clear();

            //double offsetChange = vortices.speed * actualFrameTime * fourLineFish.HeadAngle / 300.0;
            //fourLineFish.MoveHorizontally(offsetChange, screenRect.Width);

            //fourLineFish.UpdateTail(actualFrameTime / 1000.0);
            
            //playfield.Children.Add(fourLineFish);

            double offsetChange = vortices.speed * actualFrameTime * fish1.inputAngle / 600.0;
            if(!fish1.MoveHorizontally(offsetChange, screenRect.Width, actualFrameTime / 1000.0))
                fish1.UpdateTail(actualFrameTime / 1000.0);
            playfield.Children.Add(fish1);

            vortices.Draw(playfield.Children);

            // Calculate vortex strength and apply to feedback system
            if ((frameCount % 10) == 0)
                TactileFeedback();
            if ((frameCount % 100) == 0 && GameStarted)
                vortices.speed += 0.1;

            //CheckPlayers();
        }

        void updateDistance()
        {
            if (!GameStarted)
                return;

            swimDistance += actualFrameTime / 1000.0 * vortices.speed;
            distanceLabel.Content = Math.Round((decimal) swimDistance, 2);
        }
        //double minRed = double.MaxValue, minBlue=double.MaxValue;
        double maxRed = 0, maxBlue = 0;

        void TactileFeedback()
        {
            //approximate the fish nose position
            //subject to change, because it is positioned using margins
            Point nose = fish1.NosePosition; //new Point(290+fourLineFish.HeadAngle*3, 260);//new Point(fourLineFish.Margin.Left + fourLineFish.ActualWidth / 2 + fourLineFish.HeadAngle, fourLineFish.Margin.Left);
            //const double maxDistance = 200;
            double redDistance = maxRed, blueDistance = maxBlue;
            //byte leftMotor = 100, rightMotor = 100; //actual motor commands

            //// find closest red and blue vortices
            //foreach (SingleVortex vortex in redVortices)
            //{
            //    if (((TranslateTransform)(((TransformGroup)(vortex.Vortex.RenderTransform)).Children[3])).Y - 500 > nose.Y) continue;

            //    // vortex.ActualWidth = 300
            //    Point vortexCenter = vortex.GetCenter();
            //    //new Point(Canvas.GetLeft(vortex) + 300 / 2,
            //    // ((TranslateTransform)(((TransformGroup)(vortex.Vortex.RenderTransform)).Children[3])).Y + Canvas.GetTop(vortex) + vortex.ActualHeight / 2);

            //    double distanceSquared = Math.Pow((nose.X - vortexCenter.X), 2) + Math.Pow((nose.Y - vortexCenter.Y), 2);

            //    if (vortex.Blue)
            //    {
            //        if (distanceSquared < blueDistance)
            //        {
            //            blueDistance = Math.Sqrt(distanceSquared);
            //        }
            //    }
            //    else
            //        if (distanceSquared < redDistance)
            //        {
            //            redDistance = Math.Sqrt(distanceSquared);
            //        }
            //}

            redDistance = vortices.minRedDistance(nose);
            blueDistance = vortices.minBlueDistance(nose);

            //minRed = Math.Min(redDistance, minRed);
            //minBlue = Math.Min(blueDistance, minBlue);

            //maxBlue = Math.Max(blueDistance, minBlue);
            //maxRed = Math.Max(redDistance, minRed); // error: never gets bigger than 0

            // if crashes into a vortex, slow down
            const double crashRadius = 75;
            if (redDistance < crashRadius || blueDistance < crashRadius)
            {
                vortices.speed = 0.3;
                fish1.StartCrashAnimation();
            }
            
            // calculate fan speed commands (0...255)
            //if (redDistance < 150) //150*150)
            //    leftMotor = 255;// (byte)(Math.Sqrt(redDistance / maxDistance) * 255);
            //if (blueDistance < 120) //*120)
            //    rightMotor = 255;// (byte)(Math.Sqrt(blueDistance / maxDistance) * 255);
            //        //(byte)(127+Math.Sqrt(blueDistance / maxDistance) * 127);
            //SerialConnector.SetFanSpeeds(leftMotor, rightMotor);
            //debugLabelLeft.Content = leftMotor;
            //debugLabelRight.Content = rightMotor;

            double minvalue = 30;
            double[] leftMotors = { minvalue, minvalue, minvalue, minvalue, minvalue };
            double[] rightMotors = { minvalue, minvalue, minvalue, minvalue, minvalue };
            double[] leftDistances = { 400, 280, 200, 130, 80 };
            double[] rightDistances = { 80, 130, 200, 280, 400 };
            byte[] motors = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            for (int i = 0; i < leftMotors.Length; i++)
            {
                if (redDistance < leftDistances[i])
                    leftMotors[i] = (1 - redDistance / leftDistances[i]) * (255 - leftMotors[i]) + minvalue;
                if (blueDistance < rightDistances[i])
                    rightMotors[i] = (1 - blueDistance / rightDistances[i]) * (255 - rightMotors[i]) + minvalue;
                motors[i] = (byte)leftMotors[i];
                motors[i + leftMotors.Length] = (byte)rightMotors[i];
            }
            progressBar1.Value = motors[0];
            progressBar2.Value = motors[1];
            progressBar3.Value = motors[2];
            progressBar4.Value = motors[3];
            progressBar5.Value = motors[4];
            progressBar6.Value = motors[5];
            progressBar7.Value = motors[6];
            progressBar8.Value = motors[7];
            progressBar9.Value = motors[8];
            progressBar10.Value = motors[9];

            SerialConnector.SetFanSpeeds(motors);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            runningGameThread = false;
            Properties.Settings.Default.PrevWinPosition = this.RestoreBounds;
            Properties.Settings.Default.WindowState = (int)this.WindowState;
            Properties.Settings.Default.Save();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UninitializeNui();
            Environment.Exit(0);
        }

        private void angleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
            //fourLineFish.TurnFish(e.NewValue);
            fish1.TurnFish(e.NewValue);
            //debugLabelTopCenter.Content = "Nose: " + fourLineFish.NosePosition.ToString();
            serialWindow.turnFish(e.NewValue);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.Visibility = System.Windows.Visibility.Hidden;
            countdownTimer.Enabled = true;
            GameStarted = true;
            countdownValue = 60;
            swimDistance = 0;
            distanceLabel.Content = swimDistance;
            vortices.speed = 0.3;
            vortices.StartFlow();
        }
    }
}

// Since the timer resolution defaults to about 10ms precisely, we need to
// increase the resolution to get framerates above between 50fps with any
// consistency.
//using System.Runtime.InteropServices;
public class Win32
{
    [DllImport("Winmm.dll")]
    public static extern int timeBeginPeriod(UInt32 uPeriod);
}