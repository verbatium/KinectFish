using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

// TODO Fix this class
namespace ShapeGame2.Converters
{
    class DoubleMultiplyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (double)value * double.Parse((string)parameter) ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
