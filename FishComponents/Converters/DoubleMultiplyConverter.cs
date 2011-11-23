using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace FishComponents.Converters
{
    class DoubleMultiplyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //CultureInfo provider = new CultureInfo("fr-FR");
            //provider.NumberFormat.NumberDecimalSeparator = ".";
            //decimal newValue = decimal.Parse((string)Tail.ToString(), CultureInfo.InvariantCulture);
            return (double)value * double.Parse((string)parameter, CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
