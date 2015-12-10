using System;
using System.Globalization;
using VideoFeatureMatching.Core;

namespace VideoFeatureMatching.Converters
{
    public class DescripterConverter : BaseConverter<Descripters, int>
    {
        public override int Convert(Descripters value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value;
        }

        public override Descripters ConvertBack(int value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Descripters)value;
        }
    }
}