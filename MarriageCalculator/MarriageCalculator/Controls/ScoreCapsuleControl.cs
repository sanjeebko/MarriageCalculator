using Microsoft.Maui.Controls;

namespace MarriageCalculator.Controls;

/// <summary>
/// A reusable capsule-style control for displaying game score data
/// Features two sections (Maal/Points), winner highlighting, and seen/unseen indicators
/// </summary>
public class ScoreCapsuleControl : ContentView
{
    #region Bindable Properties
    
    public static readonly BindableProperty MaalValueProperty =
        BindableProperty.Create(nameof(MaalValue), typeof(int), typeof(ScoreCapsuleControl), 0, propertyChanged: OnMaalValueChanged);

    public static readonly BindableProperty PointsValueProperty =
        BindableProperty.Create(nameof(PointsValue), typeof(string), typeof(ScoreCapsuleControl), "0", propertyChanged: OnPointsValueChanged);

    public static readonly BindableProperty IsWinnerProperty =
        BindableProperty.Create(nameof(IsWinner), typeof(bool), typeof(ScoreCapsuleControl), false, propertyChanged: OnIsWinnerChanged);

    public static readonly BindableProperty HasSeenProperty =
        BindableProperty.Create(nameof(HasSeen), typeof(bool), typeof(ScoreCapsuleControl), true, propertyChanged: OnHasSeenChanged);

    public static readonly BindableProperty PlayerNameProperty =
        BindableProperty.Create(nameof(PlayerName), typeof(string), typeof(ScoreCapsuleControl), string.Empty);

    #endregion

    #region Public Properties

    public int MaalValue
    {
        get => (int)GetValue(MaalValueProperty);
        set => SetValue(MaalValueProperty, value);
    }

    public string PointsValue
    {
        get => (string)GetValue(PointsValueProperty);
        set => SetValue(PointsValueProperty, value);
    }

    public bool IsWinner
    {
        get => (bool)GetValue(IsWinnerProperty);
        set => SetValue(IsWinnerProperty, value);
    }

    public bool HasSeen
    {
        get => (bool)GetValue(HasSeenProperty);
        set => SetValue(HasSeenProperty, value);
    }

    public string PlayerName
    {
        get => (string)GetValue(PlayerNameProperty);
        set => SetValue(PlayerNameProperty, value);
    }

    #endregion

    #region Private Fields

    private Frame? _capsuleFrame;
    private Grid? _leftSection;
    private Grid? _rightSection;
    private Label? _maalLabel;
    private Label? _pointsLabel;
    private Label? _trophyIcon;
    private Label? _eyeIcon;

    #endregion

    #region Constructor

    public ScoreCapsuleControl()
    {
        CreateCapsuleLayout();
        UpdateAllVisualStates();
    }

    #endregion

    #region Layout Creation

    private void CreateCapsuleLayout()
    {
        // Main capsule frame with rounded corners - SLIGHTLY INCREASED HEIGHT
        _capsuleFrame = new Frame
        {
            CornerRadius = 16, // Slightly increased from 15 to 16
            Padding = new Thickness(0),
            Margin = new Thickness(0.5, 0.5),
            HeightRequest = 36, // Increased from 32 to 36
            HasShadow = false,
            BorderColor = Colors.Transparent,
            BackgroundColor = Colors.Transparent
        };

        // Main container grid
        var mainGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, // Maal section
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }  // Points section
            },
            ColumnSpacing = 0
        };

        // Left section (Maal) - Green
        _leftSection = new Grid
        {
            BackgroundColor = Color.FromRgb(67, 160, 71),
            Padding = new Thickness(4) // Slightly increased from 3 to 4
        };

        _maalLabel = new Label
        {
            FontSize = 12, // Increased from 11 to 12
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };

        _leftSection.Children.Add(_maalLabel);

        // Right section (Points) - Orange/Yellow
        _rightSection = new Grid
        {
            BackgroundColor = Color.FromRgb(255, 193, 7),
            Padding = new Thickness(4) // Slightly increased from 3 to 4
        };

        _pointsLabel = new Label
        {
            FontSize = 12, // Increased from 11 to 12
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };

        _rightSection.Children.Add(_pointsLabel);

        // Trophy icon for winners - SLIGHTLY LARGER
        _trophyIcon = new Label
        {
            Text = "\uF074", // FontelloCode.Winner
            FontFamily = "Fontello",
            FontSize = 9, // Slightly increased from 8 to 9
            TextColor = Colors.Gold,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(3, 2, 0, 0), // Slightly increased top margin
            IsVisible = false
        };

        // Eye icon for seen/unseen - SLIGHTLY LARGER
        _eyeIcon = new Label
        {
            FontFamily = "Fontello",
            FontSize = 9, // Slightly increased from 8 to 9
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 2, 3, 0) // Slightly increased top margin
        };

        // Add sections to grid
        Grid.SetColumn(_leftSection, 0);
        Grid.SetColumn(_rightSection, 1);
        Grid.SetColumn(_trophyIcon, 0);
        Grid.SetColumn(_eyeIcon, 1);

        mainGrid.Children.Add(_leftSection);
        mainGrid.Children.Add(_rightSection);
        mainGrid.Children.Add(_trophyIcon);
        mainGrid.Children.Add(_eyeIcon);

        _capsuleFrame.Content = mainGrid;
        Content = _capsuleFrame;
    }

    #endregion

    #region Property Changed Handlers

    private static void OnMaalValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ScoreCapsuleControl control)
        {
            control.UpdateMaalDisplay();
        }
    }

    private static void OnPointsValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ScoreCapsuleControl control)
        {
            control.UpdatePointsDisplay();
        }
    }

    private static void OnIsWinnerChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ScoreCapsuleControl control)
        {
            control.UpdateWinnerAppearance();
        }
    }

    private static void OnHasSeenChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ScoreCapsuleControl control)
        {
            control.UpdateSeenAppearance();
        }
    }

    #endregion

    #region Update Methods

    private void UpdateMaalDisplay()
    {
        if (_maalLabel != null)
        {
            _maalLabel.Text = MaalValue.ToString();
        }
    }

    private void UpdatePointsDisplay()
    {
        if (_pointsLabel != null)
        {
            _pointsLabel.Text = PointsValue;
        }
    }

    private void UpdateWinnerAppearance()
    {
        if (_leftSection != null)
        {
            _leftSection.BackgroundColor = IsWinner 
                ? Color.FromRgb(76, 175, 80)  // Lighter green for winner
                : Color.FromRgb(67, 160, 71); // Standard green
        }

        if (_trophyIcon != null)
        {
            _trophyIcon.IsVisible = IsWinner;
        }
    }

    private void UpdateSeenAppearance()
    {
        if (_eyeIcon != null)
        {
            _eyeIcon.Text = HasSeen ? "\uE800" : "\uE801"; // Seen vs Unseen icon
            _eyeIcon.Opacity = HasSeen ? 1.0 : 0.3; // Full opacity for seen, dimmed for unseen
        }
    }

    private void UpdateAllVisualStates()
    {
        UpdateMaalDisplay();
        UpdatePointsDisplay();
        UpdateWinnerAppearance();
        UpdateSeenAppearance();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Updates the control with new player statistics
    /// </summary>
    public void UpdatePlayerStats(int maal, string points, bool isWinner, bool hasSeen)
    {
        MaalValue = maal;
        PointsValue = points;
        IsWinner = isWinner;
        HasSeen = hasSeen;
    }

    /// <summary>
    /// Updates the control with PlayerStats object
    /// </summary>
    public void UpdatePlayerStats(PlayerStats stats)
    {
        if (stats != null)
        {
            UpdatePlayerStats(stats.Maal, stats.Point, stats.Winner, stats.Seen);
        }
    }

    #endregion
}