using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FishComponents
{
    /// <summary>
    /// Interaction logic for Fish.xaml
    /// </summary>
    public partial class FishTest : UserControl
    {



        Point CenterJoint = new Point(50, 50);
        Point TailJoint = new Point(50, 75);
        Point ColarJoint = new Point(50, 25);
        Point Nose = new Point(50, 0);
        Point TailEnd = new Point(50, 100);
        Point[] p = new Point[24]();
        
        public FishTest()
        {
            InitializeComponent();




        }

        void SetJointPoint(Ellipse retval, Point Position, Brush Stroke)
        {

            retval.Width = 5;
            retval.Height = 5;
            retval.Stroke = Stroke;
            Canvas.SetLeft(retval, Position.X);
            Canvas.SetTop(retval, Position.Y);

        }




    }
}
