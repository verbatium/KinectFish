using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ShapeGame2.Converters
{
    class TransformConverter : IMultiValueConverter
    {


        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

    
           Point p = (Point)values[1];

           Path path = (Path)values[0];
           p = path.RenderTransform.Transform(p);
  
           return p;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
