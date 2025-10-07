using Microsoft.Maui.Controls;

namespace MarriageCalculator.Behaviors;

/// <summary>
/// Behavior for smooth expand/collapse animations on any visual element
/// Provides fade, scale, and slide effects for showing/hiding content
/// </summary>
public class ExpandCollapseAnimationBehavior : Behavior<VisualElement>
{
    public static readonly BindableProperty IsExpandedProperty =
        BindableProperty.Create(nameof(IsExpanded), typeof(bool), typeof(ExpandCollapseAnimationBehavior), false, propertyChanged: OnIsExpandedChanged);

    public static readonly BindableProperty AnimationDurationProperty =
        BindableProperty.Create(nameof(AnimationDuration), typeof(uint), typeof(ExpandCollapseAnimationBehavior), (uint)300);

    public static readonly BindableProperty EnableFadeProperty =
        BindableProperty.Create(nameof(EnableFade), typeof(bool), typeof(ExpandCollapseAnimationBehavior), true);

    public static readonly BindableProperty EnableScaleProperty =
        BindableProperty.Create(nameof(EnableScale), typeof(bool), typeof(ExpandCollapseAnimationBehavior), true);

    public static readonly BindableProperty EnableSlideProperty =
        BindableProperty.Create(nameof(EnableSlide), typeof(bool), typeof(ExpandCollapseAnimationBehavior), true);

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public uint AnimationDuration
    {
        get => (uint)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    public bool EnableFade
    {
        get => (bool)GetValue(EnableFadeProperty);
        set => SetValue(EnableFadeProperty, value);
    }

    public bool EnableScale
    {
        get => (bool)GetValue(EnableScaleProperty);
        set => SetValue(EnableScaleProperty, value);
    }

    public bool EnableSlide
    {
        get => (bool)GetValue(EnableSlideProperty);
        set => SetValue(EnableSlideProperty, value);
    }

    private VisualElement? _associatedElement;
    private bool _isInitialized = false;

    protected override void OnAttachedTo(VisualElement bindable)
    {
        _associatedElement = bindable;
        
        // Use Dispatcher instead of deprecated Device.StartTimer
        bindable.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(10), () =>
        {
            SetInitialState();
            _isInitialized = true;
        });
        
        base.OnAttachedTo(bindable);
    }

    protected override void OnDetachingFrom(VisualElement bindable)
    {
        _associatedElement = null;
        _isInitialized = false;
        base.OnDetachingFrom(bindable);
    }

    private static async void OnIsExpandedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ExpandCollapseAnimationBehavior behavior && behavior._associatedElement != null && behavior._isInitialized)
        {
            var isExpanded = (bool)newValue;
            
            if (isExpanded)
            {
                await behavior.AnimateExpand();
            }
            else
            {
                await behavior.AnimateCollapse();
            }
        }
    }

    private void SetInitialState()
    {
        if (_associatedElement == null) return;

        if (IsExpanded)
        {
            // Element should be visible and in expanded state
            _associatedElement.IsVisible = true;
            _associatedElement.Opacity = 1.0;
            _associatedElement.Scale = 1.0;
            _associatedElement.TranslationY = 0;
        }
        else
        {
            // Element should be hidden and in collapsed state
            SetCollapsedState();
        }
    }

    private async Task AnimateExpand()
    {
        if (_associatedElement == null) return;

        // Make element visible first
        _associatedElement.IsVisible = true;
        
        // Set initial collapsed state for animation (without changing visibility)
        if (EnableFade)
            _associatedElement.Opacity = 0.0;

        if (EnableScale)
            _associatedElement.Scale = 0.95;

        if (EnableSlide)
            _associatedElement.TranslationY = -10;

        // Create animation tasks
        var animationTasks = new List<Task>();

        if (EnableFade)
        {
            animationTasks.Add(_associatedElement.FadeTo(1.0, AnimationDuration, Easing.CubicOut));
        }

        if (EnableScale)
        {
            animationTasks.Add(_associatedElement.ScaleTo(1.0, AnimationDuration, Easing.CubicOut));
        }

        if (EnableSlide)
        {
            animationTasks.Add(_associatedElement.TranslateTo(0, 0, AnimationDuration, Easing.CubicOut));
        }

        // Run all animations in parallel
        await Task.WhenAll(animationTasks);
    }

    private async Task AnimateCollapse()
    {
        if (_associatedElement == null) return;

        // Create animation tasks
        var animationTasks = new List<Task>();

        if (EnableFade)
        {
            animationTasks.Add(_associatedElement.FadeTo(0.0, AnimationDuration, Easing.CubicIn));
        }

        if (EnableScale)
        {
            animationTasks.Add(_associatedElement.ScaleTo(0.95, AnimationDuration, Easing.CubicIn));
        }

        if (EnableSlide)
        {
            animationTasks.Add(_associatedElement.TranslateTo(0, -10, AnimationDuration, Easing.CubicIn));
        }

        // Run all animations in parallel
        await Task.WhenAll(animationTasks);
        
        // Hide element after animation completes
        _associatedElement.IsVisible = false;
    }

    private void SetCollapsedState()
    {
        if (_associatedElement == null) return;

        if (EnableFade)
            _associatedElement.Opacity = 0.0;

        if (EnableScale)
            _associatedElement.Scale = 0.95;

        if (EnableSlide)
            _associatedElement.TranslationY = -10;

        _associatedElement.IsVisible = false;
    }
}