﻿/////////////////////////////////////////////////////////////////////////
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
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Samples.Kinect.WpfViewers;
using System.Timers;

// Since the timer resolution defaults to about 10ms precisely, we need to
// increase the resolution to get framerates above between 50fps with any
// consistency.
using System.Runtime.InteropServices;
using FishComponents;


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
        static int GameTime = 60;
        double recommendedAngle = 0.0;

        //FourLineFish fourLineFish;

        enum GamePhases
        {
            Standby,
            InstructionsLeftPose,
            InstructionsRightPose,
            Countdown,
            Started,
            GameOver,
            Demo
        }

        GamePhases GamePhase = GamePhases.Standby;

        FishComponents.Fish shadowFish;

        double swimDistance = 0; // total distance the fish has already moved
        //public SerialConnector serialWindow;
        //TODO: ADD Comport configuration
       

        public FeedBack feedback = new FeedBack("COM6");
        public RoboticFish roboticfish = new RoboticFish("COM7");
        //List<SingleVortex> redVortices = new List<SingleVortex>();
        //System.Timers.Timer redVortexTimer;

        Vortices vortices = new Vortices(Dispatcher.CurrentDispatcher);

        int countdownValue = GameTime;
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
                if(GamePhase == GamePhases.Started || GamePhase == GamePhases.Demo)
                    countdownLabel.Content = countdownValue;
                else if (GamePhase == GamePhases.Countdown)
                    InstructionLabel.Content = "Avoid vortices in " + countdownValue.ToString() + "!";
            }
            else
            {
                countdownTimer.Enabled = false;

                if (GamePhase == GamePhases.Started || GamePhase == GamePhases.Demo)
                {
                    GamePhase = GamePhases.GameOver;
                    vortices.StopFlow();
                    feedback.StopFans();
                    StartButton.Visibility = System.Windows.Visibility.Visible;
                    GameOver_fadeinout(GameOverLabel);
                }
                else if (GamePhase == GamePhases.Countdown)
                    StartGame();
            }
        }
        //Particle p = new Particle();

        public static readonly DependencyProperty KinectSensorManagerProperty =
            DependencyProperty.Register(
                "KinectSensorManager",
                typeof(KinectSensorManager),
                typeof(MainWindow),
                new PropertyMetadata(null));

        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();

        public MainWindow()
        {
            this.KinectSensorManager = new KinectSensorManager();
            this.KinectSensorManager.KinectSensorChanged += this.KinectSensorChanged;
            this.DataContext = this.KinectSensorManager;

            InitializeComponent();

#if DEBUG
    angleSlider.Visibility = System.Windows.Visibility.Visible;
    angleLabel.Visibility = System.Windows.Visibility.Visible;
    spedLabel.Visibility = System.Windows.Visibility.Visible;
    stackPanel1.Visibility = System.Windows.Visibility.Visible;
    angleSlider.Focus();
    //GameTime = 10;
    //StartSpeed = 0.5;
#endif

            //p.Status = Status.Unknown;

            //p.Radius = 35;
            //p.Mass = p.Radius * .01;
            //p.Opacity = p.Radius * .02;
            //Canvas.SetTop(p, 100     );
            //Canvas.SetLeft(p, 100);

            this.SensorChooserUI.KinectSensorChooser = sensorChooser;
            sensorChooser.Start();

            // Bind the KinectSensor from the sensorChooser to the KinectSensor on the KinectSensorManager
            var kinectSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.KinectSensorManager, KinectSensorManager.KinectSensorProperty, kinectSensorBinding);

            //fourLineFish = this.FindName("UCFish") as FourLineFish;
            


            countdownTimer.Elapsed += new ElapsedEventHandler(countdownTimerElapsed);
            

            //serialWindow = (ShapeGame2.SerialConnector)App.Current.Windows[0];
            feedback.Changed += new EventHandler(PortChanged);

            shadowFish = new FishComponents.Fish();
            shadowFish.Visibility = System.Windows.Visibility.Hidden;
        }

        public KinectSensorManager KinectSensorManager
        {
            get { return (KinectSensorManager)GetValue(KinectSensorManagerProperty); }
            set { SetValue(KinectSensorManagerProperty, value); }
        }


        #region Kinect discovery + setup

        private void KinectSensorChanged(object sender, KinectSensorManagerEventArgs<KinectSensor> args)
        {
            if (null != args.OldValue)
            {
                this.UninitializeKinectServices(args.OldValue);
            }

            if (null != args.NewValue)
            {
                this.InitializeKinectServices(this.KinectSensorManager, args.NewValue);
            }
        }

        // Kinect enabled apps should customize which Kinect services it initializes here.
        private void InitializeKinectServices(KinectSensorManager kinectSensorManager, KinectSensor sensor)
        {
            // Application should enable all streams first.
            kinectSensorManager.ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;
            kinectSensorManager.ColorStreamEnabled = true;

            sensor.SkeletonFrameReady += this.nui_SkeletonFrameReady;
            kinectSensorManager.TransformSmoothParameters = new TransformSmoothParameters
            {
                Smoothing = 0.5f,
                Correction = 0.5f,
                Prediction = 0.5f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.04f
            };
            kinectSensorManager.SkeletonStreamEnabled = true;
            kinectSensorManager.KinectSensorEnabled = true;
        }

        // Kinect enabled apps should uninitialize all Kinect services that were initialized in InitializeKinectServices() here.
        private void UninitializeKinectServices(KinectSensor sensor)
        {
            sensor.SkeletonFrameReady -= this.nui_SkeletonFrameReady;
        }

        #endregion Kinect discovery + setup

        DateTime lastFrameDrawn = DateTime.MinValue;
        DateTime predNextFrame = DateTime.MinValue;
        double actualFrameTime = 0;

        // Player(s) placement in scene (z collapsed):
        Rect playerBounds;
        Rect screenRect;

        double targetFramerate = MaxFramerate;
        int frameCount = 0;
        bool runningGameThread = false;

        SoundPlayer popSound = new SoundPlayer();
        SoundPlayer hitSound = new SoundPlayer();
        SoundPlayer squeezeSound = new SoundPlayer();

        private Skeleton[] skeletonData;

        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            try
            {
                SkeletonFrame skeletonFrame = e.OpenSkeletonFrame();
                
                if (skeletonFrame != null)
                {
                    int iSkeleton = 0;
                    if ((this.skeletonData == null) || (this.skeletonData.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }
                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);

                    foreach (Skeleton data in this.skeletonData)
                    {
                        if (SkeletonTrackingState.Tracked == data.TrackingState)
                        {
                            double angle = getFishAngle(data.Joints);
                            fish1.TurnFish(angle);
                            if (GamePhase == GamePhases.Started)
                                roboticfish.RobotAngle = angle;
                        }
                        iSkeleton++;
                    } // for each skeleton
                }
            }
            catch (Exception)
            {
              //  throw;
            }

        }

        DispatcherTimer debounceTimer = new DispatcherTimer();
        void startDebounce()
        {
            if (debounceTimer.IsEnabled)
                return;
            debounceTimer.Interval = TimeSpan.FromMilliseconds(10);
            debounceTimer.Tick += new EventHandler(endDebounce);
            debounceTimer.Start();
        }

        private void endDebounce(Object sender, EventArgs args)
        {
            debounceTimer.Stop();
            if(feedback.isButtonDown())
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(ButtonPressed));
        }

        private void ButtonPressed()
        {
            switch (GamePhase)
            {
                case GamePhases.Standby:
                    {
                        GamePhase = GamePhases.InstructionsLeftPose;
                        vortices.speed = 5.0;

                        shadowFish.FishClone(fish1);
                        Canvas.SetTop(shadowFish, Canvas.GetTop(fish1));
                        shadowFish.Visibility = System.Windows.Visibility.Visible;
                        shadowFish.TurnFish(-30);
                        shadowFish.MoveHorizontally(-500, screenRect.Width, 1.0, vortices.scaledSpeed);
                        shadowFish.HeadAngle = 30 * 0.3;
                        shadowFish.BodyAngle = shadowFish.HeadAngle;
                        shadowFish.BodyAngle2 = shadowFish.BodyAngle;
                        shadowFish.TailAngle = shadowFish.BodyAngle2 * 1.2;
                        shadowFish.Angle = -30 * 1.4;
                        InstructionLabel.Content = "Try to match the shadow!";
                        InstructionLabel.Visibility = System.Windows.Visibility.Visible;
                        break;
                    }
                case GamePhases.InstructionsLeftPose:
                case GamePhases.InstructionsRightPose:
                    StartGame();
                    break;
                case GamePhases.Started:
                    ResetGame();
                    vortices.speed = 5.0;
                    break;
                case GamePhases.GameOver: // TODO display high scores or some other indication ("Very good!"/"Try harder!")
                    GamePhase = GamePhases.Standby;
                    vortices.speed = 5.0;
                    break;
            }
      
        }

        private void GameOver_fadeinout(FrameworkElement element)
        {

            Storyboard storyboard = new Storyboard();
            TimeSpan duration = TimeSpan.FromMilliseconds(1000); //

            DoubleAnimation fadeInAnimation = new DoubleAnimation() { From = 0.0, To = 1.0, Duration = new Duration(duration) };
            Storyboard.SetTargetName(fadeInAnimation, element.Name);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath("Opacity", 1));
            storyboard.Children.Add(fadeInAnimation);
            storyboard.Begin(element);

            DoubleAnimation fadeOutAnimation = new DoubleAnimation() { From = 1.0, To = 0.0, Duration = new Duration(duration) };
            fadeOutAnimation.BeginTime = TimeSpan.FromSeconds(5);

            Storyboard.SetTargetName(fadeOutAnimation, element.Name);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath("Opacity", 0));
            storyboard.Children.Add(fadeOutAnimation);
            //EventArgs e = new ControlAnimationEventArgs(element, "fadeout");
            storyboard.Completed += new EventHandler(GameOver_Completed);
            storyboard.Begin(element);
        }


        void GameOver_Completed(object sender, EventArgs e)
        {
            vortices.speed = 4.7;
            GamePhase = GamePhases.Standby;
        }

        private void PortChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(startDebounce));
        }

        private double getFishAngle(Microsoft.Kinect.JointCollection joints)
        {

            Point a = getDisplayPosition(joints[JointType.Head]);
            a = getDisplayPosition(joints[JointType.ShoulderCenter]);

            Point b = getDisplayPosition(joints[JointType.Spine]);

            //Point c = new Point(a.X, b.Y + (b.Y-a.Y));
            //Point c = Average(
            //getDisplayPosition(joints[Joint.AnkleLeft]),
            //getDisplayPosition(joints[Joint.AnkleRight]));

            //System.Windows.Vector v1 = new System.Windows.Vector(c.X-b.X,c.Y - b.Y);
            System.Windows.Vector v2 = new System.Windows.Vector(b.X - a.X, b.Y - a.Y);
            double ang = AngleConstrain(2*(v2.Angle() - 90.0), -30, 30);//AngleConstrain(-NormalizeAngle(180.0 - v2.Angle()), -30, 30);
            //Console.WriteLine(ang);
            return ang;
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
           int colorX, colorY;
           ColorImagePoint colorPoint = sensorChooser.Kinect.MapSkeletonPointToColor(joint.Position, ColorImageFormat.RgbResolution640x480Fps30);
           colorX = colorPoint.X;
           colorY = colorPoint.Y;

           return new Point((int)(playfield.ActualWidth * colorX / 640.0), (int)(playfield.ActualHeight * colorY / 480));
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
            

          

            playerBounds.X = 0;
            playerBounds.Width = playfield.ActualWidth;
            playerBounds.Y = playfield.ActualHeight * 0.2;
            playerBounds.Height = playfield.ActualHeight * 0.75;

            Rect rFallingBounds = playerBounds;
            rFallingBounds.Y = 0;
            rFallingBounds.Height = playfield.ActualHeight;
           
            if (fish1 !=null)
            {
                double ratio = fish1.Height; // to preserve fish proportions
                fish1.Height = playfield.ActualHeight * 0.5;
                ratio /= fish1.Height;
                fish1.Width /= ratio;
                Canvas.SetLeft(fish1, playfield.ActualWidth/2 - fish1.Width / 2);
                Canvas.SetTop(fish1, playfield.ActualHeight/3*2 - fish1.Height / 2);
                fish1.fishOffset /= ratio;

                Canvas.SetLeft(shadowFish, playfield.ActualWidth / 2 - fish1.Width / 2);
                Canvas.SetTop(shadowFish, playfield.ActualHeight / 3 * 2 - fish1.Height / 2);
                shadowFish.fishOffset /= ratio;
            }
            

            // resize the background gradient
            double tunnelWidth = playfield.ActualHeight / 3.0;
            double edgeWidth = tunnelWidth / 2.0;
            Point StartPoint = new Point(0, 0.5);
            Point EndPoint = new Point(1, 0.5);
            GradientStopCollection stops = new GradientStopCollection(6);
            stops.Add(new GradientStop(Color.FromArgb(0xff, 0x10, 0x16, 0xF5), 0));
            stops.Add(new GradientStop(Color.FromArgb(0xff, 0x10, 0x16, 0xF5), (playfield.ActualWidth-tunnelWidth-edgeWidth)/(2*playfield.ActualWidth)));
            stops.Add(new GradientStop(Color.FromArgb(0xff, 0x10, 0xC6, 0xF5), (playfield.ActualWidth - tunnelWidth) / (2 * playfield.ActualWidth)));
            stops.Add(new GradientStop(Color.FromArgb(0xff, 0x10, 0xC6, 0xF5), (playfield.ActualWidth + tunnelWidth) / (2 * playfield.ActualWidth)));
            stops.Add(new GradientStop(Color.FromArgb(0xff, 0x10, 0x16, 0xF5), (playfield.ActualWidth + tunnelWidth + edgeWidth) / (2 * playfield.ActualWidth)));
            stops.Add(new GradientStop(Color.FromArgb(0xff, 0x10, 0x16, 0xF5), 1));
            LinearGradientBrush tunnelBrush = new LinearGradientBrush(stops, StartPoint, EndPoint);
            playfield.Background = tunnelBrush;

            // limit fish movement
            fish1.maxFishOffset = tunnelWidth / 2;

            shadowFish.FishClone(fish1);

            vortices.screenResized(playfield.ActualWidth, playfield.ActualHeight, tunnelWidth);

        }

        private void Window_Loaded(object sender, EventArgs e)
        {
            playfield.ClipToBounds = true;

            UpdatePlayfieldSize();

            popSound.Stream = Properties.Resources.Pop_5;
            hitSound.Stream = Properties.Resources.Hit_2;
            squeezeSound.Stream = Properties.Resources.Squeeze;

            popSound.Play();

            Win32.timeBeginPeriod(TimerResolution);
            var gameThread = new Thread(GameThread);
            gameThread.SetApartmentState(ApartmentState.STA);
            gameThread.Start();

        }

        private void GameThread()
        {
            try
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
            catch (Exception)
            {

                //   throw;
            }
        }

        double oldangle = 0.0;
        private void HandleGameTimer(int param)
        {
            try
            {

                angleLabel.Content = Math.Round(fish1.inputAngle,2);
                spedLabel.Content = Math.Round(vortices.speed,2);


                // Every so often, notify what our actual framerate is

                updateDistance(); // in top right corner

                // if you change this, remember to change dataRate variable in turnFish()
                //if ((frameCount % 2) == 0) // if fps is 50, this means "at 25 Hz"
                if (GamePhase != GamePhases.Started)
                    roboticfish.RobotAngle = 0;
                roboticfish.turnFish(); // move the robot fish, if necessary

                // Draw new Wpf scene by adding all objects to canvas
                playfield.Children.Clear();

                if (GamePhase == GamePhases.Demo)
                    fish1.inputAngle = recommendedAngle;

                double offsetChange = fish1.maxFishOffset / 150.0 * vortices.speed * actualFrameTime * fish1.inputAngle / 600.0;

                if (!fish1.MoveHorizontally(offsetChange, screenRect.Width, actualFrameTime / 1000.0, vortices.scaledSpeed))
                    fish1.UpdateTail(actualFrameTime / 1000.0);
                playfield.Children.Add(fish1);


                // p.Status = (Status)_r.Next(1, 9);

                //Canvas.SetLeft(p, fish1.NosePosition.X - p.ActualWidth/2);
                //Canvas.SetTop(p, fish1.NosePosition.Y - 2*p.ActualHeight);
                //playfield.Children.Add(p);

                switch (GamePhase)
                {
                    case GamePhases.Demo:
                    case GamePhases.Started:
                        {
                            // Calculate vortex strength and apply to feedback system
                            if ((frameCount % 10) == 0)
                                TactileFeedback();
                            // make the flow faster
                            if ((frameCount % 25) == 0)
                            {
                                vortices.speed += 0.1;
                            }
                            oldangle = fish1.inputAngle;
                            vortices.Draw(playfield.Children);
                            break;
                        }
                    case GamePhases.InstructionsLeftPose:
                        {
                            if (Math.Abs(shadowFish.fishOffset - fish1.fishOffset) < 10)
                            {
                                GamePhase = GamePhases.InstructionsRightPose;

                                shadowFish.TurnFish(30);
                                shadowFish.MoveHorizontally(500, screenRect.Width, 1.0, vortices.scaledSpeed);
                                shadowFish.HeadAngle = -30 * 0.3;
                                shadowFish.BodyAngle = shadowFish.HeadAngle;
                                shadowFish.BodyAngle2 = shadowFish.BodyAngle;
                                shadowFish.TailAngle = shadowFish.BodyAngle2 * 1.2;
                                shadowFish.Angle = 30 * 1.4;
                                InstructionLabel.Content = "Good! Once more!";
                            }
                            playfield.Children.Add(shadowFish);
                            break;
                        }
                    case GamePhases.InstructionsRightPose:
                        {
                            if (Math.Abs(shadowFish.fishOffset - fish1.fishOffset) < 10)
                            {
                                GamePhase = GamePhases.Countdown;
                                countdownValue = 6;
                                shadowFish.Visibility = System.Windows.Visibility.Hidden;
                                countdownTimer.Enabled = true;
                            }
                            playfield.Children.Add(shadowFish);
                            break;
                        }
                    case GamePhases.GameOver:

                        vortices.Draw(playfield.Children);

                        break;
                }


            }
            catch (Exception)
            {

               // throw;
            }
        }

        void updateDistance()
        {
            if (GamePhase != GamePhases.Started)
                return;

            swimDistance += actualFrameTime / 1000.0 * vortices.speed;
            distanceLabel.Content = Math.Round((decimal) swimDistance, 2);
        }
        //double minRed = double.MaxValue, minBlue=double.MaxValue;
        double maxRed = 0, maxBlue = 0;
        private double StartSpeed=4.3;

        void TactileFeedback()
        {
            try
            {


                //approximate the fish nose position
                //subject to change, because it is positioned using margins
                Point nose = fish1.NosePosition; //new Point(290+fourLineFish.HeadAngle*3, 260);//new Point(fourLineFish.Margin.Left + fourLineFish.ActualWidth / 2 + fourLineFish.HeadAngle, fourLineFish.Margin.Left);
                //const double maxDistance = 200;
                double redDistance = maxRed, blueDistance = maxBlue;

                redDistance = vortices.minRedDistance(nose);
                blueDistance = vortices.minBlueDistance(nose);

                Point closestRed = vortices.FindClosest(nose, false);
                Point closestBlue = vortices.FindClosest(nose, true);

                // if crashes into a vortex, slow down
                //const double crashRadius = 120;
                if (redDistance < playfield.ActualHeight / 8 || blueDistance < playfield.ActualHeight / 8)
                {
                    vortices.speed *= 0.8;
                    if (vortices.speed < 0.3)
                        vortices.speed = 0.3;
                    fish1.StartCrashAnimation();
                }


                byte minvalue = 0;
                byte[] motors = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                double moveAway = playfield.ActualHeight / 7.5;
                closestBlue.X += moveAway;
                closestRed.X -= moveAway;
                double movementFreedom = playfield.ActualHeight / 3.0; ;
                int steps = motors.Length;
                double stepSize = movementFreedom / steps;
                double maxDistance = screenRect.Height / 2;
                double turbo = 2.6;
                double sensitivityX = 1.5;
                for (int i = 0; i < motors.Length; i++)
                {
                    double xDistance = (nose - closestBlue).X + (i - steps / 2) * stepSize;
                    double distance = Math.Sqrt(Math.Pow(sensitivityX * xDistance, 2.0) + Math.Pow((nose - closestBlue).Y, 2.0));
                    if (distance < maxDistance)
                        motors[i] += (byte)((1 - distance / maxDistance) * 255);

                    xDistance = (nose - closestRed).X + (i - steps / 2) * stepSize;
                    distance = Math.Sqrt(Math.Pow(sensitivityX * xDistance, 2.0) + Math.Pow((nose - closestRed).Y, 2.0));
                    if (distance < maxDistance)
                        motors[i] += (byte)((1 - distance / maxDistance) * 255);
                }
                for (int i = 0; i < motors.Length; i++)
                {
                    motors[i] = (byte)Math.Min(turbo * (double)motors[i], 255);
                    motors[i] = Math.Max(motors[i], minvalue);

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

                feedback.SetFanSpeeds(motors);

                int centerlevel = Math.Max(motors[3], motors[6]);
                if (centerlevel > 127)
                {
                    if (motors[4] > motors[5])
                        recommendedAngle = 30.0 * centerlevel / 255.0;
                    else if (motors[4] < motors[5])
                        recommendedAngle = -30.0 * centerlevel / 255.0;
                    else
                        recommendedAngle = 0.0;
                }
                else
                    recommendedAngle = 0.0;
            }
            catch (Exception)
            {

                //throw;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            feedback.StopFans();
            roboticfish.rapidCenter();
            runningGameThread = false;
            Properties.Settings.Default.PrevWinPosition = this.RestoreBounds;
            Properties.Settings.Default.WindowState = (int)this.WindowState;
            Properties.Settings.Default.Save();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void angleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
            //fourLineFish.TurnFish(e.NewValue);
            fish1.TurnFish(e.NewValue);
            //debugLabelTopCenter.Content = "Nose: " + fourLineFish.NosePosition.ToString();
            roboticfish.RobotAngle = e.NewValue;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            //StartGame();
            StartDemo();
        }
        public void StartGame()
        {
            InstructionLabel.Visibility = System.Windows.Visibility.Hidden;
            shadowFish.Visibility = System.Windows.Visibility.Hidden;
            StartButton.Visibility = System.Windows.Visibility.Hidden;
            countdownTimer.Enabled = true;
            GamePhase = GamePhases.Started;
            countdownValue = GameTime;
            swimDistance = 0;
            distanceLabel.Content = swimDistance;
            vortices.speed = StartSpeed;
            vortices.StartFlow();
        }
        public void StartDemo()
        {
            StartGame();
            GamePhase = GamePhases.Demo;
        }
        public void ResetGame()
        {
            StartButton.Visibility = System.Windows.Visibility.Visible;
            countdownTimer.Enabled = false;
            GamePhase = GamePhases.Standby;
            countdownValue = GameTime;
            swimDistance = 0;
            distanceLabel.Content = swimDistance;
            vortices.speed = 0.3;
            vortices.StopFlow();
            feedback.StopFans();
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