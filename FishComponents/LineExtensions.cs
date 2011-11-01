using System;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
namespace FishComponents
{
    public static class LineExtensions
    {


        public static double LineLenght(this Line l)
        {
            return l.ToVector().Length;
        }

        public  static Vector ToVector(this Line l)
        {
            return new Vector(l.X2-l.X1,l.Y2-l.Y1);
        }


        public static double AngleBetween (this Line l1, Line l2)
        {
            return Vector.AngleBetween( l1.ToVector(), l2.ToVector());
        }

        public static Line Init(this Line l, double X1, double Y1, double X2, double Y2)
        {
           l.X1 = X1;
           l.X2 = X2;
           l.Y1 = Y1;
           l.Y2 = Y2;
           return l;

        }

        /// <summary>
        /// Returns Angle in degrees
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static double Angle(this Vector v)
        {
            return Vector.AngleBetween(new Vector(1, 0), v);
            //return Math.Asin(v.Y / v.Length).RadToDegree();
        }

        public static Vector Rotate(this Vector v, Double angle)
        {
            RotateTransform rt = new RotateTransform(angle);
            Matrix myMatrix = rt.Value;
            return myMatrix.Transform(v);
        }

        public static double RadToDegree(this Double Angle)
        {
            return Angle /Math.PI * 180;
        }

        public static double DegreeToRad(this Double Angle)
        {
            return Angle / 180 * Math.PI;
        }

        public static Vector Tangent(this Vector v, Vector v1, Vector v2) 
        {
            double a1 = v1.Angle();
            double a2 = v2.Angle();
            v = new Vector(v.Length, 0).Rotate(a1 + (a2 - a1) / 2);

//            double a = v.Angle();

            Vector v1b = v1 - v;
            Vector v2b = v2 - v;
            double a1b = Vector.AngleBetween(v1b, v);
            double a2b = Vector.AngleBetween(v, v2b);
            int i = 0;
            while (Math.Round(a1b, 10) != Math.Round(a2b, 10))
            {
                if (a2b > a1b)
                {
                    a1 = v.Angle();
                }
                else if (a1b > a2b)
                {
                    a2 = v.Angle();
                }
                v = new Vector(v.Length, 0).Rotate(a1 + (a2 - a1) / 2);
                a1b = Vector.AngleBetween(v1b, v);
                a2b = Vector.AngleBetween(v, v2b);
  //              a = v.Angle();
  //              Console.WriteLine("{0} {1} {2}", a1b, a2b, a);
                
                i++;
            }
            return v;
        }


    }
}