using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls;

namespace ShapeGame
{
    public class PolyFish
    {
       public  Polyline polyline2= new Polyline();
        double _angle;
        RotateTransform rotateTransform2 = new RotateTransform();
        TranslateTransform translate = new TranslateTransform();
        TransformGroup transformations = new TransformGroup();
        public double angle
        {
            get
            {
                return _angle;
            }
            set
            {
                _angle = value;
                rotateTransform2.Angle = value;
                //polyline2.RenderTransform = rotateTransform2;
            }
        }
        
        public PolyFish()
        {
            // Create a Polyline.
            polyline2.Points.Add(new Point(25, 25));
            polyline2.Points.Add(new Point(0, 50));
            polyline2.Points.Add(new Point(25, 75));
            polyline2.Points.Add(new Point(50, 50));
            polyline2.Points.Add(new Point(25, 25));
            polyline2.Points.Add(new Point(25, 0));
            polyline2.Stroke = Brushes.Blue;
            polyline2.StrokeThickness = 10;
            _angle = 0;
            // Create a RotateTransform to rotate
            // the Polyline 45 degrees about the
            // point (25,50).
            rotateTransform2.CenterX = 25;
            rotateTransform2.CenterY = 50;
            transformations.Children.Add(rotateTransform2);
            transformations.Children.Add(translate);
            polyline2.RenderTransform = transformations;;  
        }
        public void Draw(UIElementCollection children)
        {
            children.Add(polyline2);
        }

        public void resizePlayfield(int width, int height)
        {

            translate.X = width / 2;
            translate.Y = height /2;
        }
    }
}
