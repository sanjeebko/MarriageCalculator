using System.Globalization;
using MarriageCalculator.ViewModels;

namespace MarriageCalculator.Converter
{
    public class IsSelectedPlayerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PlayerMaal item && parameter is CollectionView collectionView)
            {
                // Get the ViewModel from the CollectionView's BindingContext
                if (collectionView.BindingContext is MarriageGameViewModel viewModel)
                {
                    return item == viewModel.SelectedPlayer;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
