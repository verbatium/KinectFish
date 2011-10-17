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
        public RedVortex()
        {
            InitializeComponent();
        }
        public void Randomize()
        {
            Random random = new Random();
            TransformGroup tg = Vortex.RenderTransform as TransformGroup;
            ((ScaleTransform)(tg.Children[0])).ScaleX = (1 + random.NextDouble()) / 2;
            ((ScaleTransform)(tg.Children[0])).ScaleY = (1 + random.NextDouble()) / 2;

            ((SkewTransform)(tg.Children[1])).AngleX = random.Next(-20, 20);
            ((SkewTransform)(tg.Children[1])).AngleY = random.Next(-20, 20);
        }

        private void AnimationFinished(object sender, EventArgs e)
        {
            Finished = true;
        }
    }
}
