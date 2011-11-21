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
using System.Windows.Media.Animation;
using System.ComponentModel;

namespace ShapeGame2
{
    /// <summary>
    /// Interaction logic for RedVortex.xaml
    /// </summary>
    public partial class SingleVortex : UserControl, INotifyPropertyChanged
    {
        public bool Finished = false;
        public bool Blue = false;
        private Storyboard storyboard;


        // Blindly copied from Fish.xaml.cs
        public event PropertyChangedEventHandler PropertyChanged;
        public static readonly DependencyProperty vortexOffsetProperty = DependencyProperty.Register("vortexOffsetOffset", typeof(double), typeof(SingleVortex), new FrameworkPropertyMetadata(new double(), FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnProportionChanged)));
        public double vortexOffset
        {
            get { return (double)GetValue(vortexOffsetProperty); }
            set { SetValue(vortexOffsetProperty, value); }
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

        private static void OnProportionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            SingleVortex control = (SingleVortex)obj;

            RoutedPropertyChangedEventArgs<double> e = new RoutedPropertyChangedEventArgs<double>(
                (double)args.OldValue, (double)args.NewValue, ProportionChangedEvent);
            control.ReSizeBody();
            control.OnProportionChanged(e);

        }
        public void ReSizeBody()
        {
            OnPropertyChanged("vortexOffset");
        }
        /// <summary>
        /// Identifies the TailChanged routed event.
        /// </summary>
        public static readonly RoutedEvent ProportionChangedEvent = EventManager.RegisterRoutedEvent("TailChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>), typeof(SingleVortex));

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

        public SingleVortex()
        {
            InitializeComponent();
            storyboard = (Storyboard) this.FindResource("Flow");
        }

        public Point GetCenter()
        {
            double x = (double)this.GetValue(Canvas.LeftProperty);
            x += Vortex.ActualWidth / 2;
            x += vortexOffset;
            double y = (double)this.GetValue(Canvas.TopProperty);
            y += Vortex.ActualHeight / 2;
            y += vortexTranslateTransform.Y;
            return new Point(x, y);
        }

        public void PlayfieldResized(double tunnelWidth)
        {
            if (Blue)
                vortexOffset = tunnelWidth / 3;
            else
                vortexOffset = -tunnelWidth / 3;

            TransformGroup tg = Vortex.RenderTransform as TransformGroup;
            ((ScaleTransform)(tg.Children[0])).ScaleX = tunnelWidth / Vortex.Width;
            ((ScaleTransform)(tg.Children[0])).ScaleY = tunnelWidth / Vortex.Width;
        }

        

        public void paintBlue()
        {
            Random random = new Random();
            GradientBrush gb = (GradientBrush)Vortex.Fill;
            gb.GradientStops[0].Color = Color.FromArgb(255, 23, (byte)(random.Next(0, 10)), 255); //#FF1704FF
            gb.GradientStops[1].Color = Color.FromArgb(255, 2, 63, 250);//#FF023FFA
            gb.GradientStops[2].Color = Color.FromArgb(189, 1, 106, 247);//#BD016AF7
            gb.GradientStops[3].Color = Color.FromArgb(61, 1, 106, 247);//#0000E2FF
            gb.GradientStops[4].Color = Color.FromArgb(0, 1, 106, 247);//#0000E2FF
            Blue = true;
            ((DoubleAnimation)storyboard.Children[1]).To = 150.0; // rotate clockwise
        }
        public void Randomize()
        {
            Random random = new Random();
            GradientBrush gb = (GradientBrush) Vortex.Fill;
            gb.GradientStops[0].Color = Color.FromArgb(255, 255, (byte)(50+random.Next(0,50)), 4);
            gb.GradientStops[1].Offset = random.NextDouble() / 3;
            TransformGroup tg = Vortex.RenderTransform as TransformGroup;
            //((ScaleTransform)(tg.Children[0])).ScaleX = (1 + random.NextDouble());
            //((ScaleTransform)(tg.Children[0])).ScaleY = (1 + random.NextDouble());

            ((SkewTransform)(tg.Children[1])).AngleX = random.Next(-20, 20);
            ((SkewTransform)(tg.Children[1])).AngleY = random.Next(-20, 20);

            ((RotateTransform)(tg.Children[2])).Angle = random.Next(-20, 20);
        }

        private void AnimationFinished(object sender, EventArgs e)
        {
            Finished = true;
        }

        public double speed
        {
            get {return storyboard.SpeedRatio;}
            set { storyboard.SetSpeedRatio(value); }
        }
    }
}
