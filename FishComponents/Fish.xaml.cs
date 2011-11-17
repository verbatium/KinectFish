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
using System.ComponentModel;
using System.Globalization;
using System.Windows.Media.Animation;

namespace FishComponents
{
    /// <summary>
    /// Interaction logic for Test.xaml
    /// </summary>
    public partial class Fish : UserControl , INotifyPropertyChanged
    {
  
        public event PropertyChangedEventHandler PropertyChanged;

        Storyboard storyboard;

        public Fish()
        {
            InitializeComponent();
            storyboard = (Storyboard)this.FindResource("CrashSlowdown");
        }
        Point[] Tangent(Point p1, Point p2, Point p3, bool tangent = true, double k=0.3)
        {
            //TranslateTransform toVector = new TranslateTransform(-p2.X, -p2.Y);
            Vector v1 = new Vector(p1.X-p2.X,p1.Y-p2.Y );
            Vector v2 = new Vector(p3.X - p2.X, p3.Y - p2.Y);
            Vector v4 = v1*k;
            Vector v5 = v2*k;
            if (tangent)
            {
                double a = Vector.AngleBetween(v1, v2); 
                double b = Math.Sign(a) * (Math.Abs(a)/2 - 90);
                v4 = v4.Rotate(b);
                v5 = v5.Rotate(-b);
            }
            else
            {
                double b = Math.Abs(Vector.AngleBetween(v1, v2)) / 4;
                v4 = v4.Rotate(-b);
                v5 = v5.Rotate(b);
            }
           
            return new Point[] { new Point(p2.X + v4.X, p2.Y + v4.Y), new Point(p2.X+v5.X,p2.Y+ v5.Y) };
        }

        PID rotatePID = new PID(6.0, 0.0, 0.5);
        PID body1PID = new PID(0.6, 0.0, 4.5);
        PID body2PID = new PID(0.6, 0.0, 5.0);
        PID tailPID = new PID(0.6, 0.005, 4.5);
        double fishOffset = 0;
        double maxFishOffset = 100;
        public double inputAngle = 0;

        public void TurnFish(double angle)
        {
            //Angle = angle;
            //HeadAngle = angle / 2;
            //BodyAngle2 -= (angle - Angle)/2;
            //body2PID.resetIntegral();
            inputAngle = angle;
        }

        public void UpdateTail(double secondsPassed)
        {
            HeadAngle *= 0.995;
            Angle *= 0.99;
            BodyAngle -= body1PID.update((BodyAngle) * secondsPassed);
            BodyAngle2 -= body2PID.update((BodyAngle2) * secondsPassed);
            TailAngle -= tailPID.update(TailAngle * secondsPassed);
        }

        public bool MoveHorizontally(double toRight, double screenWidth, double secondsPassed)
        {
            fishOffset += toRight;
            fishOffset = Math.Max(fishOffset, -maxFishOffset);
            fishOffset = Math.Min(fishOffset, maxFishOffset);
            if (fishOffset != maxFishOffset && fishOffset != -maxFishOffset)
            {
                Canvas.SetLeft(this, screenWidth / 2 - this.ActualWidth / 2 + (int)fishOffset);
                HeadAngle = -inputAngle * 0.3;
                BodyAngle -= body1PID.update((BodyAngle - HeadAngle) * secondsPassed);
                BodyAngle2 -= body2PID.update((BodyAngle2 - BodyAngle) * secondsPassed);
                TailAngle -= tailPID.update((TailAngle - BodyAngle2*1.2) * secondsPassed);
                double oldAngle = Angle;
                Angle -= rotatePID.update((Angle - inputAngle * 1.4) * secondsPassed);
                BodyAngle2 += (Angle - oldAngle);
                //BodyAngle2 -= toRight * 2;
                //BodyAngle2 = Math.Max(BodyAngle2, -30);
                //BodyAngle2 = Math.Min(BodyAngle2, 30);
                //body2PID.resetIntegral();
                //TailAngle += toRight * 0.1;
                //tailPID.resetIntegral();
                return true;
            }
            else
                return false;
        }

        public void StartCrashAnimation()
        {
            storyboard.Begin();
            //Canvas.SetTop(p, 100);
        }

        public Transform TailLTransform
        {
            get
            {
                TransformGroup retval = new TransformGroup();
                retval.Children.Add(new RotateTransform(-TailAngle * 0.5, TailPoint.X, TailPoint.Y));
                retval.Children.Add(new RotateTransform(-BodyAngle2 , CenterPoint.X, CenterPoint.Y));
                return retval;
            }
        }
        public Transform TailLineTransform
        {
            get
            {
                TransformGroup retval = new TransformGroup();
                retval.Children.Add(new RotateTransform(-TailAngle, TailPoint.X, TailPoint.Y));
                retval.Children.Add(new RotateTransform(-BodyAngle2 , CenterPoint.X, CenterPoint.Y));
                return retval;
            }
        }
        public Transform ColarLineTransform
        {
            get
            {
                TransformGroup retval = new TransformGroup();
                retval.Children.Add(new RotateTransform(HeadAngle * 0.5, ColarPoint.X, ColarPoint.Y));
                retval.Children.Add(new RotateTransform(BodyAngle , CenterPoint.X, CenterPoint.Y));
                return retval;
            }
        }

        public Transform HeadLineTransform
        {
            get
            {
                TransformGroup retval = new TransformGroup();
                retval.Children.Add(new RotateTransform(HeadAngle, ColarPoint.X, ColarPoint.Y));
                retval.Children.Add(new RotateTransform(BodyAngle , CenterPoint.X, CenterPoint.Y));
                return retval;
            }
        }

        public Transform CenterLineTransform
        {
            get
            {
                return new RotateTransform(0.5 * (BodyAngle - BodyAngle2), CenterPoint.X, CenterPoint.Y);
            }
        } 
     


        PointCollection pOutlinePoints = new PointCollection(new Point[24]);
        struct tangentProperties
        {
            public bool IsTangent;
            public  double k;
            public tangentProperties(bool IsTangent=true, double k=0.3)
            {
                this.IsTangent = IsTangent;
                this.k = k;
            }
        }
        tangentProperties[] pars = new tangentProperties[] {
                new tangentProperties(true,0.3),
                new tangentProperties(false,0.3), //Tail
                new tangentProperties(true,0.3),
                new tangentProperties(true,0.5), //body left
                new tangentProperties(true,0.3),
                new tangentProperties(true,0.3),//nose
                new tangentProperties(true,0.3),
                new tangentProperties(true,0.5)};//body right

        public PointCollection createOutline()
        {
            pOutlinePoints[2] = (TailLTransform.Transform(TailPointR));//// (pOutlinePoints[2])
            pOutlinePoints[5] = TailLineTransform.Transform(EndPoint);// (pOutlinePoints[5])
            pOutlinePoints[8] = TailLTransform.Transform(TailPointL);// (pOutlinePoints[8]);
            pOutlinePoints[11] = CenterLineTransform.Transform(CenterPointL);// (pOutlinePoints[11]);
            pOutlinePoints[14] = ColarLineTransform.Transform(ColarPointL);//(pOutlinePoints[14]);
            pOutlinePoints[17] = HeadLineTransform.Transform(NosePoint);//(pOutlinePoints[17]);
            pOutlinePoints[20] = ColarLineTransform.Transform(ColarPointR);//(pOutlinePoints[20]); 
            pOutlinePoints[23] = CenterLineTransform.Transform(CenterPointR);//(pOutlinePoints[23]); 
            //TailPointR

            Point[] pp;

            for (int i = 0; i < 8; i++)
            {
                int j = ((i+7) * 3 + 2) % 24;
                int k = ((i + 8) * 3 + 2) % 24;
                int l = ((i + 9) * 3 + 2) % 24;
                pp = Tangent(pOutlinePoints[j], pOutlinePoints[k], pOutlinePoints[l], pars[i].IsTangent,pars[i].k);
                pOutlinePoints[k - 1] = (pp[0]); pOutlinePoints[l - 2] = (pp[1]);
            }

            //pp = Tangent(pOutlinePoints[23], pOutlinePoints[2], pOutlinePoints[5]);
            //pOutlinePoints[1] = (pp[0]); pOutlinePoints[3] = (pp[1]);
            ////EndPoint
            //pp = Tangent(pOutlinePoints[2], pOutlinePoints[5], pOutlinePoints[5], false);
            //pOutlinePoints[4] = (pp[0]); pOutlinePoints[6] = (pp[1]);
            ////TailPointL
            //pp = Tangent(pOutlinePoints[5], pOutlinePoints[8], pOutlinePoints[11]);
            //pOutlinePoints[7] = (pp[0]); pOutlinePoints[9] = (pp[1]);

            ////CenterPointL
            //pp = Tangent(pOutlinePoints[8], pOutlinePoints[11], pOutlinePoints[14], true, 0.5);
            //pOutlinePoints[10] = (pp[0]); pOutlinePoints[12] = (pp[1]);

            ////ColarPointL
            //pp = Tangent(pOutlinePoints[11], pOutlinePoints[14], pOutlinePoints[17]);
            //pOutlinePoints[13] = (pp[0]); pOutlinePoints[15] = (pp[1]);

            ////NosePoint
            //pp = Tangent(pOutlinePoints[14], pOutlinePoints[17], pOutlinePoints[20]);
            //pOutlinePoints[16] = (pp[0]); pOutlinePoints[18] = (pp[1]);

            ////ColarPointR
            //pp = Tangent(pOutlinePoints[17], pOutlinePoints[20], pOutlinePoints[23]);
            //pOutlinePoints[19] = (pp[0]); pOutlinePoints[21] = (pp[1]);

            ////CenterPointR
            //pp = Tangent(pOutlinePoints[20], pOutlinePoints[23], pOutlinePoints[2], true, 0.5);
            //pOutlinePoints[22] = (pp[0]); pOutlinePoints[0] = pp[1];

            return pOutlinePoints;
        }

        public void ReSizeBody()
        {
            if ((Visibility)FindResource("PointVisibility")== System.Windows.Visibility.Visible)
            {
                OnPropertyChanged("NosePoint");
                OnPropertyChanged("TailPoint");
                OnPropertyChanged("TailPointL");
                OnPropertyChanged("TailPointR");
                OnPropertyChanged("CenterPointL");
                OnPropertyChanged("ColarPoint");
                OnPropertyChanged("ColarPointL");
                OnPropertyChanged("ColarPointR");
                OnPropertyChanged("EndPoint");
            }
            OnPropertyChanged("CenterPoint");
            OnPropertyChanged("CenterPointR");

            OnPropertyChanged("OutlinePoints");

            OnPropertyChanged("HeadLineTransform");
            OnPropertyChanged("TailLTransform");
            OnPropertyChanged("TailLineTransform");
            OnPropertyChanged("ColarLineTransform");
            OnPropertyChanged("CenterLineTransform");
            createOutline();
        }

        public static readonly DependencyProperty CenterProperty
            = DependencyProperty.Register("Center",
            typeof(double),
            typeof(Fish),
            new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnProportionChanged), new CoerceValueCallback(CoerceProportion)));
        /// <summary>
        /// Center Of Body  0 ... 1
        /// </summary>
        public double Center
        {
            get { return (double)GetValue(CenterProperty); }
            set
            {
                SetValue(CenterProperty, value);
            }
        }

        public static readonly DependencyProperty ColarProperty = DependencyProperty.Register("Colar", typeof(double), typeof(Fish), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnProportionChanged), new CoerceValueCallback(CoerceProportion)));
        /// <summary>
        /// Center Of Colar 0 ... 1 relative to Center
        /// </summary>
        public double Colar
        {
            get { return (double)GetValue(ColarProperty); }
            set { SetValue(ColarProperty, value); OnPropertyChanged("Colar"); }
        }

        public static readonly DependencyProperty ColarWidthProperty = DependencyProperty.Register("ColarWidth", typeof(double), typeof(Fish), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnProportionChanged), new CoerceValueCallback(CoerceProportion)));
        /// <summary>
        /// Width Of Colar 0 ... 1 relative to Width
        /// </summary>
        public double ColarWidth
        {
            get { return (double)GetValue(ColarWidthProperty); }
            set { SetValue(ColarWidthProperty, value); }
        }
        public static readonly DependencyProperty TailWidthProperty = DependencyProperty.Register("TailWidth", typeof(double), typeof(Fish), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnProportionChanged), new CoerceValueCallback(CoerceProportion)));
        /// <summary>
        /// Center Of Tail 0 ... 1 relative to Width
        /// </summary>
        public double TailWidth
        {
            get { return (double)GetValue(TailWidthProperty); }
            set { SetValue(TailWidthProperty, value);}
        }



        public static readonly DependencyProperty TailProperty = DependencyProperty.Register("Tail", typeof(double), typeof(Fish), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnProportionChanged), new CoerceValueCallback(CoerceProportion)));
        /// <summary>
        /// Center Of Colar 0 ... 1 relative to Center
        /// </summary>
        public double Tail
        {
            get { return (double)GetValue(TailProperty); }
            set { SetValue(TailProperty, value);         }
        }

        private static object CoerceProportion(DependencyObject element, object proporion)
        {


           //CultureInfo provider = new CultureInfo("fr-FR");
           //provider.NumberFormat.NumberDecimalSeparator = ".";
           //decimal newValue = decimal.Parse((string)Tail.ToString(), CultureInfo.InvariantCulture);
           double newValue = (double)proporion;
            Fish control = (Fish)element;

            //newValue =  Math.Max(MinValue, Math.Min(MaxValue, newValue));

            return newValue;
        }

        private static void OnProportionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Fish control = (Fish)obj;

            RoutedPropertyChangedEventArgs<double> e = new RoutedPropertyChangedEventArgs<double>(
                (double)args.OldValue, (double)args.NewValue, ProportionChangedEvent);
            control.ReSizeBody();
            control.OnProportionChanged(e);

        }
        /// <summary>
        /// Identifies the TailChanged routed event.
        /// </summary>
        public static readonly RoutedEvent ProportionChangedEvent = EventManager.RegisterRoutedEvent("TailChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>), typeof(Fish));

        /// <summary>
        /// Occurs when the Tail property changes.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<double> ProportionChanged
        {
            add { AddHandler(ProportionChangedEvent, value); }
            remove { RemoveHandler(ProportionChangedEvent, value); }
        }

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnProportionChanged(RoutedPropertyChangedEventArgs<double> args)
        {
            RaiseEvent(args);
        }

        public static readonly DependencyProperty HeadAngleProperty = DependencyProperty.Register("HeadAngle", typeof(double), typeof(Fish), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnProportionChanged)));
        public double HeadAngle
        {
            get { return (double)GetValue(HeadAngleProperty); }
            set {         SetValue(HeadAngleProperty, value); }
        }

        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(Fish), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnProportionChanged)));
        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set {         SetValue(AngleProperty, value); }
        }

        public static readonly DependencyProperty BodyAngleProperty = DependencyProperty.Register("BodyAngle", typeof(double), typeof(Fish), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnProportionChanged)));
        public double BodyAngle
        {
            get { return (double)GetValue(BodyAngleProperty); }
            set
            {
                SetValue(BodyAngleProperty, value);
                //OnPropertyChanged("BodyAngle");
            }

        }

        public static readonly DependencyProperty BodyAngle2Property = DependencyProperty.Register("BodyAngle2", typeof(double), typeof(Fish), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnProportionChanged)));
        public double BodyAngle2
        {
            get { return (double)GetValue(BodyAngle2Property); }
            set {         SetValue(BodyAngle2Property, value); }

        }

        public static readonly DependencyProperty TailAngleProperty = DependencyProperty.Register("TailAngle", typeof(double), typeof(Fish), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnProportionChanged)));
        public double TailAngle
        {
            get { return (double)GetValue(TailAngleProperty); }
            set { SetValue(TailAngleProperty, value);         }
        }

        public static readonly DependencyProperty NosePositionProperty = DependencyProperty.Register("NosePosition", typeof(Point), typeof(Fish), new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsRender));
        public Point NosePosition
        {
            get
            {
                Point retval = new Point((double)this.GetValue(Canvas.LeftProperty), (double)this.GetValue(Canvas.TopProperty));
                Point p = new ScaleTransform(this.ActualWidth / this.Width,this.ActualHeight / this.Height).Transform( HeadLineTransform.Transform(NosePoint));
                p = new RotateTransform(Angle, CenterPoint.X, CenterPoint.Y).Transform(p);
                return new TranslateTransform(p.X, p.Y).Transform(retval);

            }
        }

        public Point ColarPoint
        {
            get
            {
                return new Point(Width * 0.5, Height * Center * Colar);
            }
        }
        public Point ColarPointR
        {
            get
            {
                return new Point(Width * 0.5 * (1 + ColarWidth), Height * Center * Colar);
            }
        }
        public Point ColarPointL
        {
            get
            {
                return new Point(Width * 0.5 * (1-  ColarWidth) , Height * Center * Colar);
            }
        }
        public Point CenterPoint
        {
            get
            {
                return new Point(Width*0.5, Height*Center);
            }
        }
        public Point CenterPointL
        {
            get
            {
                return new Point(0, Height * Center);
            }
        }
        public Point CenterPointR
        {
            get
            {
                return new Point(Width, Height * Center);
            }
        }
        public Point TailPoint
        {
            get
            {
                return new Point(Width * 0.5, Height * (Center* (1 - Tail) + Tail));
            }
        }
        public Point TailPointR
        {
            get
            {
                return new Point(Width * 0.5 * (1 + TailWidth), Height * (Center * (1 - Tail) + Tail));
            }
        }
        public Point TailPointL
        {
            get
            {
                return new Point(Width * 0.5 * (1 - TailWidth), Height * (Center * (1 - Tail) + Tail));
            }
        }
        public Point EndPoint
        {
            get
            {
                return new Point(Width * 0.5, Height);
            }
        }
        public Point NosePoint
        {
            get
            {
                return new Point(Width * 0.5, 0);
            }
        }
 
        public PointCollection OutlinePoints
        {
            get 
            {

                return pOutlinePoints;
            }
       }

        public Point SupportPoint(int i)
        {
            Point retval = new Point(0, 0);
            switch (i)
            {
                case 0: break;
                case 1: break;
                case 2: break;
                case 6: 
                    break;
                default:
                    break;
            }
            return retval;
        }
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            ReSizeBody();
        }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}