using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace ShapeGame
{
    public class Fishbone
    {
        Line line;
        double angle = 0.0;
        double length = 100.0;

        public Fishbone(int posx, int posy)
        {
            
            line = new Line();
            line.StrokeThickness = 2;
            line.X1 = posx;
            line.Y1 = posy;
            ChangeAngle(angle);
            line.Stroke = new SolidColorBrush(Color.FromRgb(200, 200, 200));
            line.StrokeEndLineCap = PenLineCap.Round;
            line.StrokeStartLineCap = PenLineCap.Round;
        }

        public void Draw(UIElementCollection children)
        {
            children.Add(line);
        }
        public void ChangeAngle(double angle)
        {
            line.X2 = line.X1 - length * Math.Cos(angle * Math.PI / 360.0 + Math.PI / 2);
            line.Y2 = line.Y1 + length * Math.Sin(angle * Math.PI / 360.0 + Math.PI / 2);
        }
        public void resizePlayfield(int width, int height)
        {
            line.X1 = width/2;
            line.Y1 = height - length - 20;
        }
    }
}

