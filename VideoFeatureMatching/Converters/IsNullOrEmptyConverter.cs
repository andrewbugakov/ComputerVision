using System;
using System.Globalization;

namespace VideoFeatureMatching.Converters
{
    public class IsNullOrEmptyConverter : BaseConverter<string, bool>
    {
        public override bool Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            return String.IsNullOrWhiteSpace(value);
        }
    }
}