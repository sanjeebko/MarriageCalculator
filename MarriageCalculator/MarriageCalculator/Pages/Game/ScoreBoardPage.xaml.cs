using MarriageCalculator.ViewModels;
using Syncfusion.Maui.DataGrid;
using MarriageCalculator.Core.Models;

namespace MarriageCalculator.Pages.Game;

public partial class ScoreBoardPage : ContentPage
{
    public ScoreBoardViewModel ViewModel { get; }
    private SfDataGrid? _scoreDataGrid;
    
    // Store the original orientation state
    private DisplayOrientation _originalDisplayOrientation;
#if ANDROID
    private Android.Content.PM.ScreenOrientation _originalScreenOrientation;
#endif
    
    public ScoreBoardPage(ScoreBoardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        ViewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Store original orientation state before changing
        await StoreOriginalOrientationAsync();
        
        // Force landscape orientation for better table viewing
        await SetLandscapeOrientationAsync();
        
        // Wait a bit for orientation change to complete
        await Task.Delay(200);
        
        // Find the data grid and set up columns
        if (_scoreDataGrid == null)
        {
            _scoreDataGrid = this.FindByName<SfDataGrid>("scoreDataGrid");
            if (_scoreDataGrid != null)
            {
                // Apply enhanced styling for maximum visibility
                _scoreDataGrid.HeaderRowHeight = 42;
                _scoreDataGrid.RowHeight = 50;
                
                // Configure text styling for better visibility
                ConfigureDataGridStyling();
                
                SetupDynamicColumns();
            }
        }
        
        // Refresh the data grid in case data has changed
        if (_scoreDataGrid != null && ViewModel != null)
        {
            _scoreDataGrid.ItemsSource = null;
            _scoreDataGrid.ItemsSource = ViewModel.GameRowsData;
        }
    }

    protected override async void OnDisappearing()
    {
        // Restore the original orientation state
        await RestoreOriginalOrientationAsync();
        
        base.OnDisappearing();
    }

    private async Task StoreOriginalOrientationAsync()
    {
        try
        {
            // Store the current display orientation
            _originalDisplayOrientation = DeviceDisplay.Current.MainDisplayInfo.Orientation;
            
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
#if ANDROID
                // Store the current Android screen orientation setting
                var activity = Platform.CurrentActivity;
                if (activity != null)
                {
                    _originalScreenOrientation = activity.RequestedOrientation;
                }
#endif
            });
            
            System.Diagnostics.Debug.WriteLine($"ScoreBoardPage: Stored original orientation - Display: {_originalDisplayOrientation}");
#if ANDROID
            System.Diagnostics.Debug.WriteLine($"ScoreBoardPage: Stored original Android orientation: {_originalScreenOrientation}");
#endif
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to store original orientation: {ex.Message}");
        }
    }

    private async Task RestoreOriginalOrientationAsync()
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
#if ANDROID
                var activity = Platform.CurrentActivity;
                if (activity != null)
                {
                    // Restore the original Android screen orientation
                    activity.RequestedOrientation = _originalScreenOrientation;
                }
#elif IOS
                // iOS implementation would go here to restore original orientation
                System.Diagnostics.Debug.WriteLine("iOS original orientation restore requested");
#endif
            });
            
            System.Diagnostics.Debug.WriteLine($"ScoreBoardPage: Restored original orientation - Display: {_originalDisplayOrientation}");
#if ANDROID
            System.Diagnostics.Debug.WriteLine($"ScoreBoardPage: Restored original Android orientation: {_originalScreenOrientation}");
#endif
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to restore original orientation: {ex.Message}");
        }
    }

    private async Task SetLandscapeOrientationAsync()
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
#if ANDROID
                var activity = Platform.CurrentActivity;
                if (activity != null)
                {
                    activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
                }
#elif IOS
                // iOS implementation would go here
                System.Diagnostics.Debug.WriteLine("iOS landscape orientation requested");
#endif
            });
            
            System.Diagnostics.Debug.WriteLine("ScoreBoardPage: Landscape orientation set");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to set landscape orientation: {ex.Message}");
        }
    }

    private void SetupDynamicColumns()
    {
        if (_scoreDataGrid == null) return;

        // Clear existing player columns (keep the Game Number column)
        var columnsToRemove = _scoreDataGrid.Columns
            .Where(c => c.MappingName != "GameNumber")
            .ToList();
        
        foreach (var column in columnsToRemove)
        {
            _scoreDataGrid.Columns.Remove(column);
        }

        // Enhanced game number column styling for maximum visibility
        var gameNumberColumn = _scoreDataGrid.Columns.FirstOrDefault(c => c.MappingName == "GameNumber");
        if (gameNumberColumn is DataGridTextColumn textColumn)
        {
            textColumn.Width = 80; // Increased for maximum visibility
            textColumn.HeaderText = "Game #";
            textColumn.HeaderTextAlignment = TextAlignment.Center;
        }

        // Calculate optimal column width with updated game column width
        var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
        var screenWidth = displayInfo.Width / displayInfo.Density; // Convert to device-independent pixels
        var gameColumnWidth = 80; // Increased from 75 to 80 for maximum game number visibility
        var frameMargins = 16; // Frame margins
        var framePadding = 8; // Frame padding
        var gridMargins = 0; // No additional grid margins
        var scrollbarSpace = 5; // Scrollbar space
        var totalReservedSpace = gameColumnWidth + frameMargins + framePadding + gridMargins + scrollbarSpace;
        var availableWidth = screenWidth - totalReservedSpace;
        
        var playerCount = ViewModel.PlayerNames.Count;
        
        // Calculate column width with better precision
        var idealColumnWidth = playerCount > 0 ? availableWidth / playerCount : 85;
        var columnWidth = Math.Max(65, Math.Floor(idealColumnWidth - 10)); // Reduced minimum to 65 to accommodate larger game column

        foreach (var playerName in ViewModel.PlayerNames)
        {
            // Create a template column for custom formatting and styling
            var column = new DataGridTemplateColumn
            {
                MappingName = $"[{playerName}]",
                HeaderText = playerName,
                Width = columnWidth,
                CellTemplate = CreatePlayerCellTemplate(playerName)
            };
            
            _scoreDataGrid.Columns.Add(column);
        }
        
        System.Diagnostics.Debug.WriteLine($"ScoreBoardPage: Set up {playerCount} columns with width {columnWidth:F0} (Screen: {screenWidth:F0}, Available: {availableWidth:F0}, Reserved: {totalReservedSpace:F0})");
    }

    private DataTemplate CreatePlayerCellTemplate(string playerName)
    {
        return new DataTemplate(() =>
        {
            var capsuleControl = new Controls.ScoreCapsuleControl
            {
                PlayerName = playerName
            };

            // Create bindings for the control properties
            var maalBinding = new MultiBinding
            {
                Converter = new CapsuleMaalConverter(playerName)
            };
            maalBinding.Bindings.Add(new Binding("."));

            var pointsBinding = new MultiBinding
            {
                Converter = new CapsulePointsConverter(playerName)
            };
            pointsBinding.Bindings.Add(new Binding("."));

            var winnerBinding = new MultiBinding
            {
                Converter = new WinnerVisibilityConverter(playerName)
            };
            winnerBinding.Bindings.Add(new Binding("."));

            var seenBinding = new MultiBinding
            {
                Converter = new HasSeenConverter(playerName)
            };
            seenBinding.Bindings.Add(new Binding("."));

            // Apply bindings to the control
            capsuleControl.SetBinding(Controls.ScoreCapsuleControl.MaalValueProperty, maalBinding);
            capsuleControl.SetBinding(Controls.ScoreCapsuleControl.PointsValueProperty, pointsBinding);
            capsuleControl.SetBinding(Controls.ScoreCapsuleControl.IsWinnerProperty, winnerBinding);
            capsuleControl.SetBinding(Controls.ScoreCapsuleControl.HasSeenProperty, seenBinding);

            return capsuleControl;
        });
    }

    private void ConfigureDataGridStyling()
    {
        if (_scoreDataGrid == null) return;

        try
        {
            System.Diagnostics.Debug.WriteLine("ScoreBoardPage: DataGrid styling configured for better text visibility");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to configure DataGrid styling: {ex.Message}");
        }
    }
}

// Converter to extract Maal value for capsule
public class CapsuleMaalConverter : IMultiValueConverter
{
    private readonly string _playerName;

    public CapsuleMaalConverter(string playerName)
    {
        _playerName = playerName;
    }

    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (values[0] is GameRowData gameRowData)
        {
            if (gameRowData.PlayerData.TryGetValue(_playerName, out var data))
            {
                var lines = data.Split('\n');
                if (lines.Length >= 1)
                {
                    var maalLine = lines[0]; // "Maal: X"
                    var colonIndex = maalLine.IndexOf(':');
                    if (colonIndex > 0 && colonIndex < maalLine.Length - 1)
                    {
                        var maalValue = maalLine.Substring(colonIndex + 1).Trim();
                        return int.TryParse(maalValue, out var result) ? result : 0; // Return as int for MaalValue property
                    }
                }
            }
        }
        return 0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Converter to extract Points value for capsule
public class CapsulePointsConverter : IMultiValueConverter
{
    private readonly string _playerName;

    public CapsulePointsConverter(string playerName)
    {
        _playerName = playerName;
    }

    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (values[0] is GameRowData gameRowData)
        {
            if (gameRowData.PlayerData.TryGetValue(_playerName, out var data))
            {
                var lines = data.Split('\n');
                if (lines.Length >= 2)
                {
                    var pointsLine = lines[1]; // "Points: Y"
                    var colonIndex = pointsLine.IndexOf(':');
                    if (colonIndex > 0 && colonIndex < pointsLine.Length - 1)
                    {
                        var pointsValue = pointsLine.Substring(colonIndex + 1).Trim();
                        var spaceIndex = pointsValue.IndexOf(' ');
                        if (spaceIndex > 0)
                        {
                            pointsValue = pointsValue.Substring(0, spaceIndex);
                        }
                        return pointsValue; // Return as string for PointsValue property
                    }
                }
            }
        }
        return "0";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Converter for trophy visibility
public class WinnerVisibilityConverter : IMultiValueConverter
{
    private readonly string _playerName;

    public WinnerVisibilityConverter(string playerName)
    {
        _playerName = playerName;
    }

    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (values[0] is GameRowData gameRowData)
        {
            return gameRowData.IsWinner(_playerName);
        }
        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Converter for HasSeen status
public class HasSeenConverter : IMultiValueConverter
{
    private readonly string _playerName;

    public HasSeenConverter(string playerName)
    {
        _playerName = playerName;
    }

    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (values[0] is GameRowData gameRowData)
        {
            return gameRowData.HasSeen(_playerName);
        }
        return true; // Default to seen
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}