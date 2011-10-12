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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class FourLineFish : UserControl
    {

        public static readonly DependencyProperty HeadAngleProperty
    = DependencyProperty.Register("HeadAngle", typeof(double), typeof(FourLineFish), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender));
        
        // Needed for fish head contourtransforms, because we cannot calculate in XAML.
        public static readonly DependencyProperty NegHeadAngleProperty
            = DependencyProperty.Register("NegHeadAngle", typeof(double), typeof(FourLineFish), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender));

        //public static readonly DependencyProperty BodyAngleProperty
        //    = DependencyProperty.Register("BodyAngle", typeof(double), typeof(UserControl1), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BodyAngle1Property
            = DependencyProperty.Register("BodyAngle1", typeof(double), typeof(FourLineFish), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty BodyAngle2Property
            = DependencyProperty.Register("BodyAngle2", typeof(double), typeof(FourLineFish), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty TailAngleProperty
    = DependencyProperty.Register("TailAngle", typeof(double), typeof(FourLineFish), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender));


        //public double BodyAngle
        //{
        //    get { return (double)GetValue(BodyAngleProperty); }
        //    set
        //    {
        //        SetValue(BodyAngleProperty, value);
        //    }

        //}
        
        public double NegHeadAngle
        {
            get { return (double)GetValue(NegHeadAngleProperty); }
            set { SetValue(NegHeadAngleProperty, value); }
        }

        public double HeadAngle
        {
            get { return (double)GetValue(HeadAngleProperty); }
            set { SetValue(HeadAngleProperty, value); }
        }


        public double BodyAngle1
        {
            get { return (double)GetValue(BodyAngle1Property); }
            set { SetValue(BodyAngle1Property, value); }
        }


        public double BodyAngle2
        {
            get { return (double)GetValue(BodyAngle2Property); }
            set { SetValue(BodyAngle2Property, value); }
        }

        public double TailAngle
        {
            get { return (double)GetValue(TailAngleProperty); }
            set { SetValue(TailAngleProperty, value); }
        }

        public FourLineFish()
        {
            InitializeComponent();
        }

        static FourLineFish()
        {

        }

        double turningAngle = 0.0;
        const double waterConstant = 2.0;

        public void TurnFish(double angle)
        {
            HeadAngle = angle / 2;
            NegHeadAngle = -HeadAngle;
            BodyAngle1 = angle / 2;
            BodyAngle2 = -angle / 2;
            TailAngle += waterConstant * (angle-turningAngle);
            turningAngle = angle;
            TurnCollar(HeadAngle);
            TurnHeadContours(HeadAngle);
        }

        void TurnCollar(double headAngle)
        {
            TransformGroup tg = Collar.RenderTransform as TransformGroup;
            ((RotateTransform)(tg.Children[0])).Angle = HeadAngle/2;
        }
        void TurnHeadContours(double headAngle)
        {
            TransformGroup tg = RightHead.RenderTransform as TransformGroup;
            ((SkewTransform)(tg.Children[0])).AngleY = -HeadAngle/2;
            tg = LeftHead.RenderTransform as TransformGroup;
            ((SkewTransform)(tg.Children[0])).AngleY = -HeadAngle / 2;
        }

        const double straighteningSpeed = 0.7;
        public void UpdateTail(double secondsPassed)
        {
            TailAngle *= (1-straighteningSpeed*secondsPassed);
        }
    }
}
