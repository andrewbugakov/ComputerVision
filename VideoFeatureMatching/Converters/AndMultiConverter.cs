using System;
using System.Globalization;
using System.Linq;

namespace VideoFeatureMatching.Converters
{
    public class AndMultiConverter : BaseMultiConverter<bool, bool>
    {
        public override bool Convert(bool[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Aggregate(true, (current, value) => current & value);
        }
    }
}