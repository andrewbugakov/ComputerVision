using System;
using System.Globalization;

namespace VideoFeatureMatching.Converters
{
    public class EnumEqualConverter : BaseConverter<Enum, bool>
    {
        public override bool Convert(Enum value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(value, parameter);
        }
    }
}