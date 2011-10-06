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
        public Polyline polyline2 = new Polyline();
        RotateTransform rotateTransform = new RotateTransform();
        TranslateTransform translateTransform = new TranslateTransform();
        TransformGroup transformations = new TransformGroup();

        public double angle
        {
            get
            {
                return rotateTransform.Angle;
            }
            set
            {
                rotateTransform.Angle = value;
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


            // Create a RotateTransform to rotate
            // the Polyline 45 degrees about the
            // point (25,50).
            rotateTransform.CenterX = 25;
            rotateTransform.CenterY = 50;
            transformations.Children.Add(rotateTransform);
            //TODO: here we can add scale transformation

            //Move it to position on the screen
            transformations.Children.Add(translateTransform);
            //apply all transformations  to the shape
            polyline2.RenderTransform = transformations; ;
        }
        public void Draw(UIElementCollection children)
        {
            children.Add(polyline2);
        }

        public void resizePlayfield(int width, int height)
        {
            translateTransform.X = width / 2;
            translateTransform.Y = height / 2;
        }
    }
}
