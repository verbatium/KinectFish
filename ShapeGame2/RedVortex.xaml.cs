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
    /// Interaction logic for RedVortex.xaml
    /// </summary>
    public partial class RedVortex : UserControl
    {
        public bool Finished = false;
        public bool Blue = false;
        public RedVortex()
        {
            InitializeComponent();
            
        }
        public void paintBlue()
        {
            Random random = new Random();
            GradientBrush gb = (GradientBrush)Vortex.Fill;
            gb.GradientStops[0].Color = Color.FromArgb(255, 23, (byte)(random.Next(0, 10)), 255); //#FF1704FF
            gb.GradientStops[1].Color = Color.FromArgb(255, 2, 63, 250);//#FF023FFA
            gb.GradientStops[2].Color = Color.FromArgb(189, 1, 106, 247);//#BD016AF7
            gb.GradientStops[3].Color = Color.FromArgb(0, 0, 226, 255);//#0000E2FF
            Blue = true;
        }
        public void Randomize()
        {
            
                label1.Content = Canvas.GetLeft(this);
            Random random = new Random();
            GradientBrush gb = (GradientBrush) Vortex.Fill;
            gb.GradientStops[0].Color = Color.FromArgb(255, 255, (byte)(50+random.Next(0,50)), 4);
            gb.GradientStops[1].Offset = random.NextDouble() / 3;
            TransformGroup tg = Vortex.RenderTransform as TransformGroup;
            ((ScaleTransform)(tg.Children[0])).ScaleX = (1 + random.NextDouble());
            ((ScaleTransform)(tg.Children[0])).ScaleY = (1 + random.NextDouble());

            ((SkewTransform)(tg.Children[1])).AngleX = random.Next(-20, 20);
            ((SkewTransform)(tg.Children[1])).AngleY = random.Next(-20, 20);

            ((RotateTransform)(tg.Children[2])).Angle = random.Next(-20, 20);
        }

        private void AnimationFinished(object sender, EventArgs e)
        {
            Finished = true;
        }
    }
}
