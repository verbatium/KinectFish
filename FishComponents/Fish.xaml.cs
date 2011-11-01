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

namespace FishComponents
{
    /// <summary>
    /// Interaction logic for Test.xaml
    /// </summary>
    public partial class Fish : UserControl , INotifyPropertyChanged
    {
        Point Nose = new Point(0.5, 0);
        Point TailEnd = new Point(0.5, 1);
        public event PropertyChangedEventHandler PropertyChanged;

        // Point[] p = new Point[24]();
  
        public Fish()
        {
            InitializeComponent();
            createBody();
        }


        public void createBody()
        {

        }

        public void ReSizeBody()
        {
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
            control.OnPropertyChanged("Tail");
            control.OnPropertyChanged("TailPoint");
            control.OnPropertyChanged("Center");
            control.OnPropertyChanged("CenterPoint");
            control.OnPropertyChanged("Colar");
            control.OnPropertyChanged("ColarPoint");
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

        public static readonly DependencyProperty HeadAngleProperty = DependencyProperty.Register("HeadAngle", typeof(double), typeof(Fish), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender));
        public double HeadAngle
        {
            get { return (double)GetValue(HeadAngleProperty); }
            set
            {
                SetValue(HeadAngleProperty, value);
                OnPropertyChanged("HeadAngle");
            }
        }

        public static readonly DependencyProperty BodyAngleProperty = DependencyProperty.Register("BodyAngle", typeof(double), typeof(Fish), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender));
        public double BodyAngle
        {
            get { return (double)GetValue(BodyAngleProperty); }
            set
            {
                SetValue(BodyAngleProperty, value);
                OnPropertyChanged("BodyAngle");
            }

        }

        public static readonly DependencyProperty TailAngleProperty = DependencyProperty.Register("TailAngle", typeof(double), typeof(Fish), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender));
        public double TailAngle
        {
            get { return (double)GetValue(TailAngleProperty); }
            set
            {
                SetValue(TailAngleProperty, value);
                OnPropertyChanged("TailAngle");
            }
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

                Point p = new Point(Width / 2, 0);
                //p = Head.RenderTransform.Transform(p);

                x += p.X * kx;
                y += p.Y * ky;
                return new Point(x, y);

            }
        }

        public Point ColarPoint
        {
            get
            {
                return new Point(Width * 0.5, Height * Center * Colar);
            }
        }
        public Point CenterPoint
        {
            get
            {
                return new Point(Width*0.5, Height*Center);
            }
        }
        public Point TailPoint
        {
            get
            {
                return new Point(Width * 0.5, Height * (Center* (1 - Tail) + Tail));
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
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            OnPropertyChanged("NosePoint");
            OnPropertyChanged("EndPoint");
            OnPropertyChanged("TailPoint");
            OnPropertyChanged("CenterPoint");
            OnPropertyChanged("ColarPoint");
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