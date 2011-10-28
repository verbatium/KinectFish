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
        const double timerValue = 2500;
        System.Timers.Timer vortexGeneratorTimer = new System.Timers.Timer(timerValue);
        Dispatcher dispatcher;
        double vortexSpeed = 0.3;
        double screenWidth = 100, screenHeight = 100;

        public Vortices(Dispatcher MainWindowDispatcher)
        {
            vortexGeneratorTimer.Elapsed += new ElapsedEventHandler(GenerateVortex);
            
            dispatcher = MainWindowDispatcher;
        }

        public void StartFlow()
        {
            vortexGeneratorTimer.Enabled = true;
        }
        public void StopFlow()
        {
            vortexGeneratorTimer.Enabled = false;
        }

        private void GenerateVortex(object sender, ElapsedEventArgs e)
        {
            dispatcher.Invoke(DispatcherPriority.Normal, new Action(CreateVortex));
        }

        bool nextVortexIsBlue = false;
        private void CreateVortex()
        {
            // Create a new red vortex object
            SingleVortex RV1 = new SingleVortex();
            Canvas.SetTop(RV1, -500);
            RV1.Randomize(); // make it look different
            double blueleft = screenWidth / 2 - 100;
            double redleft = screenWidth / 2 - 300 + 100;
            if (nextVortexIsBlue)
            {
                RV1.paintBlue();
                Canvas.SetLeft(RV1, blueleft);
                nextVortexIsBlue = false;
                blues.Add(RV1);
            }
            else
            {
                Canvas.SetLeft(RV1, redleft);
                nextVortexIsBlue = true;
                reds.Add(RV1);
            }

            Storyboard sb1 = RV1.FindResource("Flow") as Storyboard;
            
            sb1.Begin(); // make it move
            sb1.SetSpeedRatio(vortexSpeed);

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
                    sv.speed = value;
                foreach (SingleVortex sv in blues)
                    sv.speed = value;
                vortexGeneratorTimer.Interval = timerValue / value;
            }
        }
        public void screenResized(double newWidth, double newHeight)
        {
            screenWidth = newWidth;
            screenHeight = newHeight;
            double blueleft = newWidth / 2;
            double redleft = newWidth / 2 - 300;

            foreach (SingleVortex sv in reds)
                Canvas.SetLeft(sv, redleft);
            foreach (SingleVortex sv in blues)
                Canvas.SetLeft(sv, blueleft);
        }
    }
}
