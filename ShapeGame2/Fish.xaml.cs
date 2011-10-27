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

namespace ShapeGame2
{
    /// <summary>
    /// Interaction logic for Fish.xaml
    /// </summary>
    public partial class Fish : UserControl
    {



        Point CenterJoint = new Point(50, 50);
        Point TailJoint = new Point(50, 75);
        Point ColarJoint = new Point(50, 25);
        Point Nose = new Point(50, 0);
        Point TailEnd = new Point(50, 100);

        public Fish()
        {
            InitializeComponent();
            //Center = 50;
            //SetJointPoint(CenterP, CenterJoint, Brushes.Red);
            //SetJointPoint(TailP, TailJoint, Brushes.Red);
            //SetJointPoint(ColarP, ColarJoint, Brushes.Red);
            //SetJointPoint(NoseP, Nose, Brushes.Green);
            //SetJointPoint(TailEndP, TailEnd, Brushes.Green);

        }
        void SetJointPoint(Ellipse retval, Point Position, Brush Stroke)
        {

            retval.Width = 5;
            retval.Height = 5;
            retval.Stroke = Stroke;
            Canvas.SetLeft(retval, Position.X);
            Canvas.SetTop(retval, Position.Y);

        }

        public static readonly DependencyProperty CenterProperty
            = DependencyProperty.Register("Center",
            typeof(double),
            typeof(Fish),
            new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender));
        /// <summary>
        /// Center Of Body %
        /// </summary>
        public double Center
        {
            get { return (double)GetValue(CenterProperty); }
            set
            {
                SetValue(CenterProperty, value);
            }
        }

        public static readonly DependencyProperty ColarProperty
           = DependencyProperty.Register("Colar",
           typeof(double),
           typeof(Fish),
           new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender));
        /// <summary>
        /// Center Of Colar % relative to Center
        /// </summary>
        public double Colar
        {
            get { return (double)GetValue(ColarProperty); }
            set
            {
                SetValue(ColarProperty, value);
            }
        }
        public static readonly DependencyProperty TailProperty
   = DependencyProperty.Register("Tail",
   typeof(double),
   typeof(Fish),
   new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender));
        /// <summary>
        /// Center Of Colar % relative to Center
        /// </summary>
        public double Tail
        {
            get { return (double)GetValue(TailProperty); }
            set
            {
                SetValue(TailProperty, value);
            }
        }

    }
}
