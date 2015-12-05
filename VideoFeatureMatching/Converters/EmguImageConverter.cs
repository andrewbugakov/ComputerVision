using System;
using System.Globalization;
using System.Windows.Media;
using Emgu.CV;
using VideoFeatureMatching.Utils;

namespace VideoFeatureMatching.Converters
{
    public class EmguImageConverter : BaseConverter<IImage, ImageSource>
    {
        public override ImageSource Convert(IImage value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? null : value.ToBitmapSource();
        }
    }
}