using System;
using System.Globalization;
using VideoFeatureMatching.Core;

namespace VideoFeatureMatching.Converters
{
    public class DetectorConverter : BaseConverter<Detectors, int>
    {
        public override int Convert(Detectors value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int) value;
        }

        public override Detectors ConvertBack(int value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Detectors) value;
        }
    }
}