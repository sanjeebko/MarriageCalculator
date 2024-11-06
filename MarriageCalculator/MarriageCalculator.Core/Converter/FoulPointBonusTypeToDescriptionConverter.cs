using System.Globalization;
using MarriageCalculator.Core.Extensions;
using MarriageCalculator.Core.Models;

namespace MarriageCalculator.Core.Converter
{
    public class FoulPointBonusTypeToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FoulPointBonusType foulBonusType)
            {
                return foulBonusType.GetDescription();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
