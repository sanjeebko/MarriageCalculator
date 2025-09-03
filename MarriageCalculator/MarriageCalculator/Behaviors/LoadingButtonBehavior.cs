using Microsoft.Maui.Controls;
using System.Windows.Input;

namespace MarriageCalculator.Behaviors;

/// <summary>
/// Loading button behavior that shows animated loading states
/// Provides spinner, progress, and pulse loading animations
/// </summary>
public class LoadingButtonBehavior : Behavior<Button>
{
    public static readonly BindableProperty IsLoadingProperty =
        BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(LoadingButtonBehavior), false, propertyChanged: OnIsLoadingChanged);

    public static readonly BindableProperty LoadingTextProperty =
        BindableProperty.Create(nameof(LoadingText), typeof(string), typeof(LoadingButtonBehavior), "Loading...");

    public static readonly BindableProperty LoadingAnimationTypeProperty =
        BindableProperty.Create(nameof(LoadingAnimationType), typeof(LoadingAnimationType), typeof(LoadingButtonBehavior), LoadingAnimationType.Spinner);

    public static readonly BindableProperty DisableWhenLoadingProperty =
        BindableProperty.Create(nameof(DisableWhenLoading), typeof(bool), typeof(LoadingButtonBehavior), true);

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public string LoadingText
    {
        get => (string)GetValue(LoadingTextProperty);
        set => SetValue(LoadingTextProperty, value);
    }

    public LoadingAnimationType LoadingAnimationType
    {
        get => (LoadingAnimationType)GetValue(LoadingAnimationTypeProperty);
        set => SetValue(LoadingAnimationTypeProperty, value);
    }

    public bool DisableWhenLoading
    {
        get => (bool)GetValue(DisableWhenLoadingProperty);
        set => SetValue(DisableWhenLoadingProperty, value);
    }

    private Button? _associatedButton;
    private string? _originalText;
    private bool _originalIsEnabled;
    private CancellationTokenSource? _animationCancellationToken;

    protected override void OnAttachedTo(Button bindable)
    {
        _associatedButton = bindable;
        _originalText = bindable.Text;
        _originalIsEnabled = bindable.IsEnabled;
        base.OnAttachedTo(bindable);
    }

    protected override void OnDetachingFrom(Button bindable)
    {
        StopLoadingAnimation();
        _associatedButton = null;
        base.OnDetachingFrom(bindable);
    }

    private static void OnIsLoadingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LoadingButtonBehavior behavior)
        {
            var isLoading = (bool)newValue;
            if (isLoading)
            {
                behavior.StartLoadingAnimation();
            }
            else
            {
                behavior.StopLoadingAnimation();
            }
        }
    }

    private async void StartLoadingAnimation()
    {
        if (_associatedButton == null) return;

        // Store original state
        _originalText = _associatedButton.Text;
        _originalIsEnabled = _associatedButton.IsEnabled;

        // Update button state
        if (DisableWhenLoading)
        {
            _associatedButton.IsEnabled = false;
        }

        // Cancel any existing animation
        _animationCancellationToken?.Cancel();
        _animationCancellationToken = new CancellationTokenSource();

        try
        {
            switch (LoadingAnimationType)
            {
                case LoadingAnimationType.Spinner:
                    await StartSpinnerAnimation(_animationCancellationToken.Token);
                    break;

                case LoadingAnimationType.Pulse:
                    await StartPulseAnimation(_animationCancellationToken.Token);
                    break;

                case LoadingAnimationType.Progress:
                    await StartProgressAnimation(_animationCancellationToken.Token);
                    break;

                case LoadingAnimationType.Dots:
                    await StartDotsAnimation(_animationCancellationToken.Token);
                    break;

                case LoadingAnimationType.Fade:
                    await StartFadeAnimation(_animationCancellationToken.Token);
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            // Animation was cancelled, this is expected
        }
    }

    private void StopLoadingAnimation()
    {
        if (_associatedButton == null) return;

        // Cancel animation
        _animationCancellationToken?.Cancel();

        // Restore original state
        _associatedButton.Text = _originalText ?? string.Empty;
        _associatedButton.IsEnabled = _originalIsEnabled;
        _associatedButton.Opacity = 1.0;
        _associatedButton.Scale = 1.0;
        _associatedButton.Rotation = 0;
    }

    private async Task StartSpinnerAnimation(CancellationToken cancellationToken)
    {
        if (_associatedButton == null) return;

        _associatedButton.Text = "? " + LoadingText;

        while (!cancellationToken.IsCancellationRequested)
        {
            await _associatedButton.RotateTo(360, 1000, Easing.Linear);
            _associatedButton.Rotation = 0;
        }
    }

    private async Task StartPulseAnimation(CancellationToken cancellationToken)
    {
        if (_associatedButton == null) return;

        _associatedButton.Text = LoadingText;

        while (!cancellationToken.IsCancellationRequested)
        {
            await _associatedButton.ScaleTo(1.05, 500, Easing.CubicInOut);
            await _associatedButton.ScaleTo(1.0, 500, Easing.CubicInOut);
        }
    }

    private async Task StartProgressAnimation(CancellationToken cancellationToken)
    {
        if (_associatedButton == null) return;

        var progressSteps = new[] { "?", "?", "?", "?", "?", "?", "?", "?" };
        var stepIndex = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            _associatedButton.Text = $"{progressSteps[stepIndex]} {LoadingText}";
            stepIndex = (stepIndex + 1) % progressSteps.Length;
            await Task.Delay(150, cancellationToken);
        }
    }

    private async Task StartDotsAnimation(CancellationToken cancellationToken)
    {
        if (_associatedButton == null) return;

        var dotStates = new[] { "", ".", "..", "..." };
        var stateIndex = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            _associatedButton.Text = LoadingText + dotStates[stateIndex];
            stateIndex = (stateIndex + 1) % dotStates.Length;
            await Task.Delay(300, cancellationToken);
        }
    }

    private async Task StartFadeAnimation(CancellationToken cancellationToken)
    {
        if (_associatedButton == null) return;

        _associatedButton.Text = LoadingText;

        while (!cancellationToken.IsCancellationRequested)
        {
            await _associatedButton.FadeTo(0.5, 800, Easing.CubicInOut);
            await _associatedButton.FadeTo(1.0, 800, Easing.CubicInOut);
        }
    }
}

/// <summary>
/// Types of loading animations available for buttons
/// </summary>
public enum LoadingAnimationType
{
    /// <summary>
    /// Spinning animation with rotation
    /// </summary>
    Spinner,
    
    /// <summary>
    /// Pulsing scale animation
    /// </summary>
    Pulse,
    
    /// <summary>
    /// Progress bar animation
    /// </summary>
    Progress,
    
    /// <summary>
    /// Animated dots
    /// </summary>
    Dots,
    
    /// <summary>
    /// Fade in/out animation
    /// </summary>
    Fade
}