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
    /// Interaction logic for Particle.xaml
    /// </summary>
    public partial class Particle : UserControl
    {

        public Particle()
        {

            InitializeComponent();
        }

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register("Status", typeof(Status), typeof(Particle), new UIPropertyMetadata(Particle.StatusValueChanged));
        public Status Status
        {
            get { return (Status)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        private static void StatusValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Particle myclass = (Particle)d;
            myclass.imgStatus.Source = myclass.FindResource(((Status)e.NewValue).ToString() + "Png") as BitmapImage;
     

            ((BitmapImage)myclass.imgStatus.Source).CacheOption = BitmapCacheOption.OnLoad;
        }

        public double Radius
        {
            get { return this.imgStatus.Height / 2; }
            set { this.imgStatus.Height = this.imgStatus.Width = value * 2; }
        }

        private double vy;
        public double VY
        {
            get { return this.vy; }
            set { this.vy = value; }
        }

        private double vx;
        public double VX
        {
            get { return this.vx; }
            set { this.vx = value; }
        }

        private double mass;
        public double Mass
        {
            get { return this.mass; }
            set { this.mass = value; }
        }
        private double x;
        public double X
        {
            get { return this.x; }
            set { this.x = value; }
        }

        private double y;
        public double Y
        {
            get { return this.y; }
            set { this.y = value; }
        }
    }
    public enum Status
    {
        Available = 1,
        Busy = 2,
        Inactive = 3,
        InMtg = 4,
        Away = 5,
        Dnd = 6,
        Offline = 7,
        Unknown = 8,
        Blocked = 9
    }

}
