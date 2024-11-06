using System.Globalization;
using MarriageCalculator.Core.Extensions;
using MarriageCalculator.Core.Models;

namespace MarriageCalculator.Core.Converter
{
    public class CurrencyToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Currency currency)
            {
                return currency.ToDescriptionString();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
