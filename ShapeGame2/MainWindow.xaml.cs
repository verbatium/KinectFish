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

        FourLineFish fourLineFish;
        double fishOffset = 0; // how far from the center to the right
        double maxFishOffset = 150;

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
            fourLineFish = this.FindName("UCFish") as FourLineFish;
            
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

        //bool nextVortexIsBlue = false;
        //public void CreateVortex()
        //{ 
        //    // Create a new red vortex object
        //    SingleVortex RV1 = new SingleVortex();
        //    Canvas.SetTop(RV1, -500);
        //    RV1.Randomize(); // make it look different
        //    if (nextVortexIsBlue)
        //    {
        //        RV1.paintBlue();
        //        Canvas.SetLeft(RV1, 250);
        //        nextVortexIsBlue = false;
        //    }
        //    else
        //    {
        //        Canvas.SetLeft(RV1, 0);
        //        nextVortexIsBlue = true;
        //    }

            
        //    redVortices.Add(RV1);
        //    Storyboard sb1 = RV1.FindResource("Flow") as Storyboard;
        //    sb1.Begin(); // make it move

        //    // Delete one old vortex from the list
        //    if (redVortices.Count > 0)
        //        if (redVortices[0].Finished)
        //            redVortices.RemoveAt(0);
        //}

        //public void NewRedVortex(object sender, ElapsedEventArgs e)
        ////public void NewRedVortex()
        //{
        //    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(CreateVortex));
        //}
        public class Player
        {
            public bool isAlive;
            public DateTime lastUpdated;
            private Brush brJoints;
            private Brush brBones;
            private Rect playerBounds;
            private Point playerCenter;
            private double playerScale;
            private int id;
            private static int colorId = 0;

            private const double BONE_SIZE = 0.01;
            private const double HEAD_SIZE = 0.075;
            private const double HAND_SIZE = 0.03;

            // Keeping track of all bone segments of interest as well as head, hands and feet
            public Dictionary<Bone, BoneData> segments = new Dictionary<Bone, BoneData>();

            //public Player(int SkeletonSlot)
            //{
            //    id = SkeletonSlot;

            //    // Generate one of 7 colors for player
            //    int[] iMixr = { 1, 1, 1, 0, 1, 0, 0 };
            //    int[] iMixg = { 1, 1, 0, 1, 0, 1, 0 };
            //    int[] iMixb = { 1, 0, 1, 1, 0, 0, 1 };
            //    byte[] iJointCols = { 245, 200 };
            //    byte[] iBoneCols = { 235, 160 };

            //    int i = colorId;
            //    colorId = (colorId + 1) % iMixr.Count();

            //    brJoints = new SolidColorBrush(Color.FromRgb(iJointCols[iMixr[i]], iJointCols[iMixg[i]], iJointCols[iMixb[i]]));
            //    brBones = new SolidColorBrush(Color.FromRgb(iBoneCols[iMixr[i]], iBoneCols[iMixg[i]], iBoneCols[iMixb[i]]));
            //    lastUpdated = DateTime.Now;
            //}

            public int getId()
            {
                return id;
            }

            public void setBounds(Rect r)
            {
                playerBounds = r;
                playerCenter.X = (playerBounds.Left + playerBounds.Right) / 2;
                playerCenter.Y = (playerBounds.Top + playerBounds.Bottom) / 2;
                playerScale = Math.Min(playerBounds.Width, playerBounds.Height / 2);
            }

            void UpdateSegmentPosition(JointID j1, JointID j2, Segment seg)
            {
                var bone = new Bone(j1, j2);
                if (segments.ContainsKey(bone))
                {
                    BoneData data = segments[bone];
                    data.UpdateSegment(seg);
                    segments[bone] = data;
                }
                else
                    segments.Add(bone, new BoneData(seg));
            }

            public void UpdateBonePosition(Microsoft.Research.Kinect.Nui.JointsCollection joints, JointID j1, JointID j2)
            {
                var seg = new Segment(joints[j1].Position.X * playerScale + playerCenter.X,
                                      playerCenter.Y - joints[j1].Position.Y * playerScale,
                                      joints[j2].Position.X * playerScale + playerCenter.X,
                                      playerCenter.Y - joints[j2].Position.Y * playerScale);
                seg.radius = Math.Max(3.0, playerBounds.Height * BONE_SIZE) / 2;
                UpdateSegmentPosition(j1, j2, seg);
            }

            public void UpdateJointPosition(Microsoft.Research.Kinect.Nui.JointsCollection joints, JointID j)
            {
                var seg = new Segment(joints[j].Position.X * playerScale + playerCenter.X,
                                      playerCenter.Y - joints[j].Position.Y * playerScale);
                seg.radius = playerBounds.Height * ((j == JointID.Head) ? HEAD_SIZE : HAND_SIZE) / 2;
                UpdateSegmentPosition(j, j, seg);
            }

            public void Draw(UIElementCollection children)
            {
                if (!isAlive)
                    return;

                // Draw all bones first, then circles (head and hands).
                DateTime cur = DateTime.Now;
                foreach (var segment in segments)
                {
                    Segment seg = segment.Value.GetEstimatedSegment(cur);
                    if (!seg.IsCircle())
                    {
                        var line = new Line();
                        line.StrokeThickness = seg.radius * 2;
                        line.X1 = seg.x1;
                        line.Y1 = seg.y1;
                        line.X2 = seg.x2;
                        line.Y2 = seg.y2;
                        line.Stroke = brBones;
                        line.StrokeEndLineCap = PenLineCap.Round;
                        line.StrokeStartLineCap = PenLineCap.Round;
                        children.Add(line);
                    }
                }
                foreach (var segment in segments)
                {
                    Segment seg = segment.Value.GetEstimatedSegment(cur);
                    if (seg.IsCircle())
                    {
                        var circle = new Ellipse();
                        circle.Width = seg.radius * 2;
                        circle.Height = seg.radius * 2;
                        circle.SetValue(Canvas.LeftProperty, seg.x1 - seg.radius);
                        circle.SetValue(Canvas.TopProperty, seg.y1 - seg.radius);
                        circle.Stroke = brJoints;
                        circle.StrokeThickness = 1;
                        circle.Fill = brBones;
                        children.Add(circle);
                    }
                }

                // Remove unused players after 1/2 second.
                if (DateTime.Now.Subtract(lastUpdated).TotalMilliseconds > 500)
                    isAlive = false;
            }
        }

        public Dictionary<int, Player> players = new Dictionary<int, Player>();

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

        int playersAlive = 0;
        SoundPlayer popSound = new SoundPlayer();
        SoundPlayer hitSound = new SoundPlayer();
        SoundPlayer squeezeSound = new SoundPlayer();

        Runtime nui = new Runtime();


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
                    fourLineFish.TurnFish(angle);
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
        
        //void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        //{
        //    SkeletonFrame skeletonFrame = e.SkeletonFrame;

        //    int iSkeletonSlot = 0;

        //    foreach (SkeletonData data in skeletonFrame.Skeletons)
        //    {
        //        if (SkeletonTrackingState.Tracked == data.TrackingState)
        //        {
        //            Player player;
        //            if (players.ContainsKey(iSkeletonSlot))
        //            {
        //                player = players[iSkeletonSlot];
        //            }
        //            else
        //            {
        //                player = new Player(iSkeletonSlot);
        //                player.setBounds(playerBounds);
        //                players.Add(iSkeletonSlot, player);
        //            }

        //            player.lastUpdated = DateTime.Now;

        //            // Update player's bone and joint positions
        //            if (data.Joints.Count > 0)
        //            {
        //                player.isAlive = true;

        //                // Head, hands, feet (hit testing happens in order here)
        //                player.UpdateJointPosition(data.Joints, JointID.Head);
        //                player.UpdateJointPosition(data.Joints, JointID.HandLeft);
        //                player.UpdateJointPosition(data.Joints, JointID.HandRight);
        //                player.UpdateJointPosition(data.Joints, JointID.FootLeft);
        //                player.UpdateJointPosition(data.Joints, JointID.FootRight);

        //                // Hands and arms
        //                player.UpdateBonePosition(data.Joints, JointID.HandRight, JointID.WristRight);
        //                player.UpdateBonePosition(data.Joints, JointID.WristRight, JointID.ElbowRight);
        //                player.UpdateBonePosition(data.Joints, JointID.ElbowRight, JointID.ShoulderRight);

        //                player.UpdateBonePosition(data.Joints, JointID.HandLeft, JointID.WristLeft);
        //                player.UpdateBonePosition(data.Joints, JointID.WristLeft, JointID.ElbowLeft);
        //                player.UpdateBonePosition(data.Joints, JointID.ElbowLeft, JointID.ShoulderLeft);

        //                // Head and Shoulders
        //                player.UpdateBonePosition(data.Joints, JointID.ShoulderCenter, JointID.Head);
        //                player.UpdateBonePosition(data.Joints, JointID.ShoulderLeft, JointID.ShoulderCenter);
        //                player.UpdateBonePosition(data.Joints, JointID.ShoulderCenter, JointID.ShoulderRight);

        //                // Legs
        //                player.UpdateBonePosition(data.Joints, JointID.HipLeft, JointID.KneeLeft);
        //                player.UpdateBonePosition(data.Joints, JointID.KneeLeft, JointID.AnkleLeft);
        //                player.UpdateBonePosition(data.Joints, JointID.AnkleLeft, JointID.FootLeft);

        //                player.UpdateBonePosition(data.Joints, JointID.HipRight, JointID.KneeRight);
        //                player.UpdateBonePosition(data.Joints, JointID.KneeRight, JointID.AnkleRight);
        //                player.UpdateBonePosition(data.Joints, JointID.AnkleRight, JointID.FootRight);

        //                player.UpdateBonePosition(data.Joints, JointID.HipLeft, JointID.HipCenter);
        //                player.UpdateBonePosition(data.Joints, JointID.HipCenter, JointID.HipRight);

        //                // Spine
        //                player.UpdateBonePosition(data.Joints, JointID.HipCenter, JointID.ShoulderCenter);
        //            }
        //        }
        //        iSkeletonSlot++;
        //    }
        //}

        //void CheckPlayers()
        //{
        //    foreach (var player in players)
        //    {
        //        if (!player.Value.isAlive)
        //        {
        //            // Player left scene since we aren't tracking it anymore, so remove from dictionary
        //            players.Remove(player.Value.getId());
        //            break;
        //        }
        //    }

        //    // Count alive players
        //    int alive = 0;
        //    foreach (var player in players)
        //    {
        //        if (player.Value.isAlive)
        //            alive++;
        //    }
        //    if (alive != playersAlive)
        //    {
        //        if (alive == 2)
        //            fallingThings.SetGameMode(FallingThings.GameMode.TwoPlayer);
        //        else if (alive == 1)
        //            fallingThings.SetGameMode(FallingThings.GameMode.Solo);
        //        else if (alive == 0)
        //            fallingThings.SetGameMode(FallingThings.GameMode.Off);

        //        if (playersAlive == 0)
        //            BannerText.NewBanner(Properties.Resources.Vocabulary, screenRect, true, Color.FromArgb(200, 255, 255, 255));

        //        playersAlive = alive;
        //    }
        //}

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
            Canvas.SetLeft(fourLineFish, screenRect.Width / 2 - 150);
            vortices.screenResized(playfield.ActualWidth, playfield.ActualHeight);

            BannerText.UpdateBounds(screenRect);

            playerBounds.X = 0;
            playerBounds.Width = playfield.ActualWidth;
            playerBounds.Y = playfield.ActualHeight * 0.2;
            playerBounds.Height = playfield.ActualHeight * 0.75;

            foreach (var player in players)
                player.Value.setBounds(playerBounds);

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
                double angle = joystick.State.X * 40 / 100;
                debugLabelCenter.Content = 1000/actualFrameTime;
                fourLineFish.TurnFish(angle);
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
            //fallingThings.DrawFrame(playfield.Children);
            //foreach (var player in players)
            //    player.Value.Draw(playfield.Children);
            //BannerText.Draw(playfield.Children);
            //FlyingText.Draw(playfield.Children);

            fishOffset += vortices.speed * actualFrameTime * fourLineFish.HeadAngle/300.0;
            fishOffset = Math.Max(fishOffset, -maxFishOffset);
            fishOffset = Math.Min(fishOffset, maxFishOffset);
            Canvas.SetLeft(fourLineFish, screenRect.Width / 2 - 150 + (int)fishOffset);

            fourLineFish.UpdateTail(actualFrameTime / 1000.0);
            vortices.Draw(playfield.Children);
            //foreach (SingleVortex rv in redVortices)
            //    playfield.Children.Add(rv);
            playfield.Children.Add(fourLineFish);
            //RedVortex redv = new RedVortex();
            //playfield.Children.Add(redv); // 240...290, 290
            //Canvas.SetLeft(redv, 140);
            //Canvas.SetTop(redv, 290);
            //playfield.Children.Add(RV1);
            //Canvas.SetLeft(RV1, 20);
            //Canvas.SetTop(RV1, 20);

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
            Point nose = fourLineFish.NosePosition; //new Point(290+fourLineFish.HeadAngle*3, 260);//new Point(fourLineFish.Margin.Left + fourLineFish.ActualWidth / 2 + fourLineFish.HeadAngle, fourLineFish.Margin.Left);
            const double maxDistance = 200;
            double redDistance = maxRed, blueDistance = maxBlue;
            byte leftMotor = 100, rightMotor = 100; //actual motor commands

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
            const double crashRadius = 50;
            if (redDistance < crashRadius || blueDistance < crashRadius)
            {
                vortices.speed = 0.3;
            }
            
            // calculate fan speed commands (0...255)
            if (redDistance < 150) //150*150)
                leftMotor = 255;// (byte)(Math.Sqrt(redDistance / maxDistance) * 255);
            if (blueDistance < 120) //*120)
                rightMotor = 255;// (byte)(Math.Sqrt(blueDistance / maxDistance) * 255);
                    //(byte)(127+Math.Sqrt(blueDistance / maxDistance) * 127);
            SerialConnector.SetFanSpeeds(leftMotor, rightMotor);
            debugLabelLeft.Content = leftMotor;
            debugLabelRight.Content = rightMotor;

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
            
            fourLineFish.TurnFish(e.NewValue);
            debugLabelTopCenter.Content = "Nose: " + fourLineFish.NosePosition.ToString();
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