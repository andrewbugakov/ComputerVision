using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace VideoFeatureMatching.Converters
{
    public abstract class BaseMultiConverter<TFrom, TTo> : MarkupExtension, IMultiValueConverter
    {
        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(values.Cast<TFrom>().ToArray(), targetType, parameter, culture);
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return ConvertBack((TTo)value, targetTypes, parameter, culture).Cast<object>().ToArray();
        }

        public abstract TTo Convert(TFrom[] value, Type targetType, object parameter, CultureInfo culture);

        public virtual TFrom[] ConvertBack(TTo value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}