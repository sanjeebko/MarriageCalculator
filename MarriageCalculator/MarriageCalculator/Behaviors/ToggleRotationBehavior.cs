using Microsoft.Maui.Controls;

namespace MarriageCalculator.Behaviors;

/// <summary>
/// Behavior for rotating toggle buttons smoothly
/// Rotates the button based on a boolean state
/// </summary>
public class ToggleRotationBehavior : Behavior<Button>
{
    public static readonly BindableProperty IsToggledProperty =
        BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(ToggleRotationBehavior), false, propertyChanged: OnIsToggledChanged);

    public static readonly BindableProperty RotationDurationProperty =
        BindableProperty.Create(nameof(RotationDuration), typeof(uint), typeof(ToggleRotationBehavior), (uint)300);

    public static readonly BindableProperty RotationAngleProperty =
        BindableProperty.Create(nameof(RotationAngle), typeof(double), typeof(ToggleRotationBehavior), 180.0);

    public bool IsToggled
    {
        get => (bool)GetValue(IsToggledProperty);
        set => SetValue(IsToggledProperty, value);
    }

    public uint RotationDuration
    {
        get => (uint)GetValue(RotationDurationProperty);
        set => SetValue(RotationDurationProperty, value);
    }

    public double RotationAngle
    {
        get => (double)GetValue(RotationAngleProperty);
        set => SetValue(RotationAngleProperty, value);
    }

    private Button? _associatedButton;

    protected override void OnAttachedTo(Button bindable)
    {
        _associatedButton = bindable;
        
        // Set initial rotation based on IsToggled
        if (IsToggled)
        {
            _associatedButton.Rotation = RotationAngle;
        }
        
        base.OnAttachedTo(bindable);
    }

    protected override void OnDetachingFrom(Button bindable)
    {
        _associatedButton = null;
        base.OnDetachingFrom(bindable);
    }

    private static async void OnIsToggledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ToggleRotationBehavior behavior && behavior._associatedButton != null)
        {
            var isToggled = (bool)newValue;
            
            var targetRotation = isToggled ? behavior.RotationAngle : 0.0;
            
            await behavior._associatedButton.RotateTo(targetRotation, behavior.RotationDuration, Easing.CubicInOut);
        }
    }
}