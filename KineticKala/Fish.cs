using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows;

namespace SkeletalViewer
{
    class Fish
    {
        public Point Position { get; set; }
        public double Angle { get; set; }
        Body body;
        


        public Path Path
        {
            get
            {
                return new Path();
            }
        }


    }
}
