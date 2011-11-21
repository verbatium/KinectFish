using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ShapeGame2
{
    class Vortices
    {
        List<SingleVortex> reds = new List<SingleVortex>();
        List<SingleVortex> blues = new List<SingleVortex>();
        const double timerValue = 4500;
        System.Timers.Timer vortexGeneratorTimer = new System.Timers.Timer(timerValue);
        Dispatcher dispatcher;
        double vortexSpeed = 0.3;
        double screenWidth = 100, screenHeight = 100, tunnelWidth;

        public Vortices(Dispatcher MainWindowDispatcher)
        {
            vortexGeneratorTimer.Elapsed += new ElapsedEventHandler(GenerateVortex);
            
            dispatcher = MainWindowDispatcher;
        }

        public void StartFlow()
        {
            reds.Clear();
            blues.Clear();
            vortexGeneratorTimer.Interval = 100;
            vortexGeneratorTimer.Enabled = true;
            //CreateVortex();
        }
        public void StopFlow()
        {
            vortexGeneratorTimer.Enabled = false;
        }

        private void GenerateVortex(object sender, ElapsedEventArgs e)
        {
            vortexGeneratorTimer.Interval = timerValue / scaledSpeed;
            dispatcher.Invoke(DispatcherPriority.Normal, new Action(CreateVortex));
        }

        bool nextVortexIsBlue = false;
        private void CreateVortex()
        {
            // Create a new red vortex object
            SingleVortex RV1 = new SingleVortex();
            Canvas.SetTop(RV1, -500);
            Canvas.SetLeft(RV1, (screenWidth - RV1.Vortex.Width) / 2);
            RV1.Randomize(); // make it look different

            if (nextVortexIsBlue)
            {
                RV1.paintBlue();
                RV1.vortexOffset = tunnelWidth/3;
                nextVortexIsBlue = false;
                blues.Add(RV1);
            }
            else
            {
                RV1.vortexOffset = -tunnelWidth/3;
                nextVortexIsBlue = true;
                reds.Add(RV1);
            }

            Storyboard sb1 = RV1.FindResource("Flow") as Storyboard;
            
            sb1.Begin(); // make it move
            sb1.SetSpeedRatio(scaledSpeed);

            // Delete one old vortex from the list
            if (reds.Count > 0)
                if (reds[0].Finished)
                    reds.RemoveAt(0);

            if (blues.Count > 0)
                if (blues[0].Finished)
                    blues.RemoveAt(0);
        }

        public void Draw(UIElementCollection children)
        {
            foreach (SingleVortex sv in reds)
                children.Add(sv);
            foreach (SingleVortex sv in blues)
                children.Add(sv);
        }

        public Point FindClosest(Point to, bool isBlue)
        {
            List<SingleVortex> vortex;
            if (isBlue)
                vortex = blues;
            else
                vortex = reds;

            double mindistance = double.MaxValue;
            Point closest = new Point(double.MaxValue, double.MaxValue);

            foreach (SingleVortex sv in vortex)
            {
                Point center = sv.GetCenter();

                // discard vortices that are past the nose already
                if (center.Y > to.Y)
                    continue;

                double distance = (center - to).Length;
                if (distance < mindistance)
                {
                    mindistance = distance;
                    closest = center;
                }
            }
            return closest;
        }

        public double minRedDistance(Point to)
        {
            Point closest = FindClosest(to, false);
            return (closest - to).Length;
        }

        public double minBlueDistance(Point to)
        {
            Point closest = FindClosest(to, true);
            return (closest - to).Length;
        }

        public double speed
        {
            get { return vortexSpeed; }
            set
            {
                vortexSpeed = value;
                foreach (SingleVortex sv in reds)
                    sv.speed = scaledSpeed;
                foreach (SingleVortex sv in blues)
                    sv.speed = scaledSpeed;
            }
        }
        double scaledSpeed
        {
            get { return vortexSpeed * screenHeight / 1200.0; }
            set { }
        }
        public void screenResized(double newWidth, double newHeight, double tunnelWidth)
        {
            screenWidth = newWidth;
            screenHeight = newHeight;
            this.tunnelWidth = tunnelWidth;
            foreach (SingleVortex sv in reds)
            {
                sv.PlayfieldResized(tunnelWidth);
                Canvas.SetLeft(sv, (screenWidth - sv.Vortex.Width) / 2);
            }
            foreach (SingleVortex sv in blues)
            {
                sv.PlayfieldResized(tunnelWidth);
                Canvas.SetLeft(sv, (screenWidth - sv.Vortex.Width) / 2);
            }
        }
    }
}
