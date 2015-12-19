using System;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VideoFeatureMatching.Core;

namespace VideoFeatureMatching.Converters
{
    public class PlayPauseImageSourceConverter : BaseConverter<PlayerStates, ImageSource>
    {
        public override ImageSource Convert(PlayerStates value, Type targetType, object parameter, CultureInfo culture)
        {
            var uri = value == PlayerStates.Playing
                ? "Assets/ic_pause.png"
                : "Assets/ic_launch.png";

            return new BitmapImage(new Uri(uri, UriKind.Relative));
        }
    }
}