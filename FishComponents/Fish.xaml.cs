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
    /// Interaction logic for Test.xaml
    /// </summary>
    public partial class Fish : UserControl
    {

        Point CenterJoint = new Point(50, 50);
        Point TailJoint = new Point(50, 75);
        Point ColarJoint = new Point(50, 25);
        Point Nose = new Point(50, 0);
        Point TailEnd = new Point(50, 100);
       // Point[] p = new Point[24]();
        Line l = new Line();

        public Fish()
        {
            InitializeComponent();
            createBody();
        }


        public void createBody()
        {

            l.X1 = 0;
            l.Y1 = 0;
  
            l.Stroke = Brushes.Black;
            p.Children.Clear();
            p.Children.Add(l);
        }

        public void ReSizeBody()
        {
           l.X2 = Width;
            l.Y2 = Height;
        }

        public static readonly DependencyProperty CenterProperty
            = DependencyProperty.Register("Center",
            typeof(double),
            typeof(Fish),
            new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender));
        /// <summary>
        /// Center Of Body  0 ... 1
        /// </summary>
        public double Center
        {
            get { return (double)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value);         }
        }

        public static readonly DependencyProperty ColarProperty = DependencyProperty.Register("Colar", typeof(double),typeof(Fish),new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender));
        /// <summary>
        /// Center Of Colar 0 ... 1 relative to Center
        /// </summary>
        public double Colar
        {
            get { return (double)GetValue(ColarProperty); }
            set { SetValue(ColarProperty, value);         }
        }

        public static readonly DependencyProperty TailProperty = DependencyProperty.Register("Tail",typeof(double), typeof(Fish),  new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender));
        /// <summary>
        /// Center Of Colar 0 ... 1 relative to Center
        /// </summary>
        public double Tail
        {
            get { return (double)GetValue(TailProperty); }
            set
            {

                SetValue(TailProperty, value);
            }
        }


        public static readonly DependencyProperty HeadAngleProperty = DependencyProperty.Register("HeadAngle", typeof(double), typeof(Fish), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender));
        public double HeadAngle
        {
            get { return (double)GetValue(HeadAngleProperty); }
            set { SetValue(HeadAngleProperty, value); }
        }

        public static readonly DependencyProperty BodyAngleProperty = DependencyProperty.Register("BodyAngle", typeof(double), typeof(Fish), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender));
        public double BodyAngle
        {
            get { return (double)GetValue(BodyAngleProperty); }
            set { SetValue(BodyAngleProperty, value); }

        }

        public static readonly DependencyProperty TailAngleProperty = DependencyProperty.Register("TailAngle", typeof(double), typeof(Fish), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender));
        public double TailAngle
        {
            get { return (double)GetValue(TailAngleProperty); }
            set { SetValue(TailAngleProperty, value); }
        }

        public static readonly DependencyProperty NosePositionProperty = DependencyProperty.Register("NosePosition", typeof(Point), typeof(Fish), new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsRender));
        public Point NosePosition
        {
            get
            {

                double x = (double)this.GetValue(Canvas.LeftProperty);
                double kx = this.ActualWidth / 300;
                double y = (double)this.GetValue(Canvas.TopProperty);
                double ky = this.ActualHeight / 300;

                Point p = new Point(Width/2,0);
                //p = Head.RenderTransform.Transform(p);

                x += p.X * kx;
                y += p.Y * ky;

                return new Point(x, y);
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ReSizeBody();
        }
    }
}
