using Microsoft.Maui.Controls;

namespace MarriageCalculator.Behaviors;

/// <summary>
/// Behavior for staggered animation effects on child elements
/// Animates children with a slight delay for a cascading effect
/// </summary>
public class StaggeredAnimationBehavior : Behavior<Layout>
{
    public static readonly BindableProperty IsTriggeredProperty =
        BindableProperty.Create(nameof(IsTriggered), typeof(bool), typeof(StaggeredAnimationBehavior), false, propertyChanged: OnIsTriggeredChanged);

    public static readonly BindableProperty StaggerDelayProperty =
        BindableProperty.Create(nameof(StaggerDelay), typeof(uint), typeof(StaggeredAnimationBehavior), (uint)50);

    public static readonly BindableProperty AnimationDurationProperty =
        BindableProperty.Create(nameof(AnimationDuration), typeof(uint), typeof(StaggeredAnimationBehavior), (uint)200);

    public bool IsTriggered
    {
        get => (bool)GetValue(IsTriggeredProperty);
        set => SetValue(IsTriggeredProperty, value);
    }

    public uint StaggerDelay
    {
        get => (uint)GetValue(StaggerDelayProperty);
        set => SetValue(StaggerDelayProperty, value);
    }

    public uint AnimationDuration
    {
        get => (uint)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    private Layout? _associatedLayout;

    protected override void OnAttachedTo(Layout bindable)
    {
        _associatedLayout = bindable;
        base.OnAttachedTo(bindable);
    }

    protected override void OnDetachingFrom(Layout bindable)
    {
        _associatedLayout = null;
        base.OnDetachingFrom(bindable);
    }

    private static async void OnIsTriggeredChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StaggeredAnimationBehavior behavior && behavior._associatedLayout != null)
        {
            var isTriggered = (bool)newValue;
            
            if (isTriggered)
            {
                await behavior.AnimateChildren();
            }
        }
    }

    private async Task AnimateChildren()
    {
        if (_associatedLayout == null) return;

        var children = _associatedLayout.Children.OfType<VisualElement>().ToList();
        var animationTasks = new List<Task>();

        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var delay = (uint)(i * StaggerDelay);
            
            // Start each animation with a delay
            animationTasks.Add(AnimateChild(child, delay));
        }

        await Task.WhenAll(animationTasks);
    }

    private async Task AnimateChild(VisualElement child, uint delay)
    {
        // Set initial state
        child.Opacity = 0;
        child.TranslationY = 10;
        child.Scale = 0.95;

        // Wait for the stagger delay
        if (delay > 0)
        {
            await Task.Delay((int)delay);
        }

        // Animate to final state
        var fadeTask = child.FadeTo(1.0, AnimationDuration, Easing.CubicOut);
        var slideTask = child.TranslateTo(0, 0, AnimationDuration, Easing.CubicOut);
        var scaleTask = child.ScaleTo(1.0, AnimationDuration, Easing.CubicOut);

        await Task.WhenAll(fadeTask, slideTask, scaleTask);
    }
}