using System;
using System.Globalization;
using System.Windows.Data;

namespace VideoFeatureMatching.Converters
{
    public class CombiningConverter3 : CombiningConverter
    {
        public IValueConverter Converter3 { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object convertedValue = base.Convert(value, targetType, parameter, culture);
            return Converter3.Convert(convertedValue, targetType, parameter, culture);
        }
    }
}