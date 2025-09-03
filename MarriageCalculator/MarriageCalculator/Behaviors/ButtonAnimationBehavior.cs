using Microsoft.Maui.Controls;

namespace MarriageCalculator.Behaviors;

/// <summary>
/// Advanced button animation behavior for enhanced user interaction feedback
/// Provides bounce, pulse, shake, and glow effects for buttons
/// </summary>
public class ButtonAnimationBehavior : Behavior<Button>
{
    public static readonly BindableProperty AnimationTypeProperty =
        BindableProperty.Create(nameof(AnimationType), typeof(ButtonAnimationType), typeof(ButtonAnimationBehavior), ButtonAnimationType.Bounce);

    public static readonly BindableProperty DurationProperty =
        BindableProperty.Create(nameof(Duration), typeof(uint), typeof(ButtonAnimationBehavior), (uint)200);

    public static readonly BindableProperty ScaleToProperty =
        BindableProperty.Create(nameof(ScaleTo), typeof(double), typeof(ButtonAnimationBehavior), 0.95);

    public static readonly BindableProperty EnableHapticFeedbackProperty =
        BindableProperty.Create(nameof(EnableHapticFeedback), typeof(bool), typeof(ButtonAnimationBehavior), true);

    public ButtonAnimationType AnimationType
    {
        get => (ButtonAnimationType)GetValue(AnimationTypeProperty);
        set => SetValue(AnimationTypeProperty, value);
    }

    public uint Duration
    {
        get => (uint)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public double ScaleTo
    {
        get => (double)GetValue(ScaleToProperty);
        set => SetValue(ScaleToProperty, value);
    }

    public bool EnableHapticFeedback
    {
        get => (bool)GetValue(EnableHapticFeedbackProperty);
        set => SetValue(EnableHapticFeedbackProperty, value);
    }

    private Button? _associatedButton;

    protected override void OnAttachedTo(Button bindable)
    {
        _associatedButton = bindable;
        bindable.Pressed += OnButtonPressed;
        bindable.Released += OnButtonReleased;
        bindable.Clicked += OnButtonClicked;
        base.OnAttachedTo(bindable);
    }

    protected override void OnDetachingFrom(Button bindable)
    {
        bindable.Pressed -= OnButtonPressed;
        bindable.Released -= OnButtonReleased;
        bindable.Clicked -= OnButtonClicked;
        _associatedButton = null;
        base.OnDetachingFrom(bindable);
    }

    private async void OnButtonPressed(object? sender, EventArgs e)
    {
        if (_associatedButton == null) return;

        await AnimatePress();
    }

    private async void OnButtonReleased(object? sender, EventArgs e)
    {
        if (_associatedButton == null) return;

        await AnimateRelease();
    }

    private async void OnButtonClicked(object? sender, EventArgs e)
    {
        if (_associatedButton == null) return;

        await AnimateClick();

        // Optional: Add haptic feedback here if needed
        // Note: Haptic feedback implementation can vary by platform
        if (EnableHapticFeedback)
        {
            try
            {
                // TODO: Implement platform-specific haptic feedback
                System.Diagnostics.Debug.WriteLine("Button clicked with haptic feedback");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Haptic feedback error: {ex.Message}");
            }
        }
    }

    private async Task AnimatePress()
    {
        if (_associatedButton == null) return;

        switch (AnimationType)
        {
            case ButtonAnimationType.Bounce:
                await _associatedButton.ScaleTo(ScaleTo, Duration / 2, Easing.CubicOut);
                break;

            case ButtonAnimationType.Pulse:
                await _associatedButton.ScaleTo(1.1, Duration / 2, Easing.CubicOut);
                break;

            case ButtonAnimationType.Shake:
                await _associatedButton.TranslateTo(-5, 0, Duration / 4, Easing.CubicOut);
                break;

            case ButtonAnimationType.Glow:
                await _associatedButton.FadeTo(0.7, Duration / 2, Easing.CubicOut);
                break;

            case ButtonAnimationType.Rotate:
                await _associatedButton.RotateTo(5, Duration / 2, Easing.CubicOut);
                break;

            case ButtonAnimationType.Flip:
                await _associatedButton.RotateYTo(90, Duration / 2, Easing.CubicOut);
                break;
        }
    }

    private async Task AnimateRelease()
    {
        if (_associatedButton == null) return;

        switch (AnimationType)
        {
            case ButtonAnimationType.Bounce:
                await _associatedButton.ScaleTo(1.0, Duration / 2, Easing.BounceOut);
                break;

            case ButtonAnimationType.Pulse:
                await _associatedButton.ScaleTo(1.0, Duration / 2, Easing.BounceOut);
                break;

            case ButtonAnimationType.Shake:
                await _associatedButton.TranslateTo(0, 0, Duration / 2, Easing.BounceOut);
                break;

            case ButtonAnimationType.Glow:
                await _associatedButton.FadeTo(1.0, Duration / 2, Easing.CubicOut);
                break;

            case ButtonAnimationType.Rotate:
                await _associatedButton.RotateTo(0, Duration / 2, Easing.BounceOut);
                break;

            case ButtonAnimationType.Flip:
                await _associatedButton.RotateYTo(0, Duration / 2, Easing.BounceOut);
                break;
        }
    }

    private async Task AnimateClick()
    {
        if (_associatedButton == null) return;

        // Additional click animation based on type
        switch (AnimationType)
        {
            case ButtonAnimationType.Bounce:
                await _associatedButton.ScaleTo(1.05, 100, Easing.CubicOut);
                await _associatedButton.ScaleTo(1.0, 100, Easing.CubicIn);
                break;

            case ButtonAnimationType.Pulse:
                // Create pulsing effect
                var pulseTask1 = _associatedButton.ScaleTo(1.15, 150, Easing.CubicOut);
                var pulseTask2 = _associatedButton.FadeTo(0.8, 150, Easing.CubicOut);
                await Task.WhenAll(pulseTask1, pulseTask2);
                
                var resetTask1 = _associatedButton.ScaleTo(1.0, 150, Easing.CubicIn);
                var resetTask2 = _associatedButton.FadeTo(1.0, 150, Easing.CubicIn);
                await Task.WhenAll(resetTask1, resetTask2);
                break;

            case ButtonAnimationType.Shake:
                // Shake animation
                await _associatedButton.TranslateTo(-10, 0, 50);
                await _associatedButton.TranslateTo(10, 0, 50);
                await _associatedButton.TranslateTo(-5, 0, 50);
                await _associatedButton.TranslateTo(5, 0, 50);
                await _associatedButton.TranslateTo(0, 0, 50);
                break;

            case ButtonAnimationType.Glow:
                // Glow effect with color change if possible
                await _associatedButton.ScaleTo(1.08, 200, Easing.CubicOut);
                await _associatedButton.ScaleTo(1.0, 200, Easing.CubicIn);
                break;

            case ButtonAnimationType.Rotate:
                // Full rotation
                await _associatedButton.RotateTo(360, 400, Easing.CubicInOut);
                _associatedButton.Rotation = 0; // Reset rotation
                break;

            case ButtonAnimationType.Flip:
                // Card flip effect
                await _associatedButton.RotateYTo(180, 200, Easing.CubicOut);
                await _associatedButton.RotateYTo(0, 200, Easing.CubicIn);
                break;
        }
    }
}

/// <summary>
/// Types of animations available for buttons
/// </summary>
public enum ButtonAnimationType
{
    /// <summary>
    /// Bounce effect - scales down and up with bounce
    /// </summary>
    Bounce,
    
    /// <summary>
    /// Pulse effect - scales up and down with glow
    /// </summary>
    Pulse,
    
    /// <summary>
    /// Shake effect - horizontal movement
    /// </summary>
    Shake,
    
    /// <summary>
    /// Glow effect - opacity and scale changes
    /// </summary>
    Glow,
    
    /// <summary>
    /// Rotate effect - rotation animation
    /// </summary>
    Rotate,
    
    /// <summary>
    /// Flip effect - 3D flip animation
    /// </summary>
    Flip
}