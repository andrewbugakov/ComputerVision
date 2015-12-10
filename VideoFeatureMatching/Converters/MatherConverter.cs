using System;
using System.Globalization;
using VideoFeatureMatching.Core;

namespace VideoFeatureMatching.Converters
{
    public class MatherConverter : BaseConverter<Matchers, int>
    {
        public override int Convert(Matchers value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value;
        }

        public override Matchers ConvertBack(int value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Matchers)value;
        }
    }
}