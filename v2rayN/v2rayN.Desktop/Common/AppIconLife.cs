using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media.Transformation;

namespace v2rayN.Desktop.Common;

// Idle life for the title-bar app icon: it breathes, and every few seconds it plays the signature move of the current
// system proxy mode (eraser wipes, gear works in a small circle, signpost points both ways, lock nods calmly) or a
// shared hop / head shake / lean; Celebrate() plays a double hop (used when the mode changes).
// Every pose is a RenderTransform set through a TransformOperationsTransition (transitions are safe in this build;
// a keyframed RenderTransform is not). All poses share one operation layout so they interpolate component-wise.
public sealed class AppIconLife
{
    private static readonly Easing Snappy = new SpringEasing(1, 600, 30);
    private static readonly Easing Bouncy = new SpringEasing(1, 380, 18);
    private static readonly Easing Enter = new SplineEasing(0.22, 1, 0.36, 1);
    private static readonly Easing Fall = new SplineEasing(0.4, 0, 1, 1);
    private static readonly Easing Breath = new SplineEasing(0.4, 0, 0.2, 1);

    private readonly Image _icon;
    private readonly Random _random = new();
    private int _celebrate;
    private ESysProxyType _mode = ESysProxyType.Unchanged;
    private bool _running;

    private AppIconLife(Image icon)
    {
        _icon = icon;
        _icon.RenderTransformOrigin = new RelativePoint(0.5, 1, RelativeUnit.Relative);
        _icon.RenderTransform = Pose();
    }

    public static AppIconLife Attach(Image icon)
    {
        var life = new AppIconLife(icon);
        icon.AttachedToVisualTree += (_, _) => life.Start();
        icon.DetachedFromVisualTree += (_, _) => life._running = false;
        if (TopLevel.GetTopLevel(icon) is not null)
        {
            life.Start();
        }

        return life;
    }

    /// <summary>Plays an excited double hop at the next step of the loop.</summary>
    public void Celebrate() => _celebrate++;

    /// <summary>Picks the personality that matches the icon of the current system proxy mode.</summary>
    public void SetMode(ESysProxyType mode) => _mode = mode;

    private void Start()
    {
        if (_running)
        {
            return;
        }

        _running = true;
        _ = LoopAsync();
    }

    private async Task LoopAsync()
    {
        var nextAction = Environment.TickCount64 + 2500;
        while (_running)
        {
            if (TopLevel.GetTopLevel(_icon) is not Window { IsVisible: true } window || window.WindowState == WindowState.Minimized)
            {
                await Task.Delay(1000);
                continue;
            }

            if (_celebrate > 0)
            {
                _celebrate = 0;
                await HopAsync(5);
                await HopAsync(3);
                nextAction = Environment.TickCount64 + 4000;
                continue;
            }

            if (Environment.TickCount64 >= nextAction)
            {
                // Six in ten moves are the mode's signature move; the rest are shared ones.
                if (_random.Next(10) < 6)
                {
                    await SignatureAsync();
                }
                else
                {
                    switch (_random.Next(3))
                    {
                        case 0:
                            await HopAsync(_mode == ESysProxyType.Unchanged ? 2 : 4);
                            break;
                        case 1:
                            await ShakeAsync();
                            break;
                        default:
                            await LeanAsync();
                            break;
                    }
                }

                // The lock is the calm one: it acts least often.
                nextAction = Environment.TickCount64 + (_mode == ESysProxyType.Unchanged ? _random.Next(6000, 10000) : _random.Next(3500, 7000));
                continue;
            }

            // Breathe: a slow rise of 4 % and back (slower for the calm lock).
            var breath = _mode == ESysProxyType.Unchanged ? 1600 : 1100;
            await To(Pose(sy: 1.04, sx: 0.985), breath, Breath);
            await To(Pose(), breath, Breath);
        }
    }

    private Task SignatureAsync() => _mode switch
    {
        ESysProxyType.ForcedClear => WipeAsync(),
        ESysProxyType.ForcedChange => WorkAsync(),
        ESysProxyType.Pac => PointAsync(),
        _ => NodAsync(),
    };

    // Eraser: rubs quickly left and right.
    private async Task WipeAsync()
    {
        for (var i = 0; i < 3; i++)
        {
            await To(Pose(x: -3, angle: -5, sx: 1.04, sy: 0.97), 85, Enter);
            await To(Pose(x: 3, angle: 5, sx: 1.04, sy: 0.97), 95, Enter);
        }

        await To(Pose(), 360, Bouncy);
    }

    // Gear: busy, rocks around a small circle.
    private async Task WorkAsync()
    {
        double[][] path = [[2, 0, 7], [0, -2, 0], [-2, 0, -7], [0, 1, 0], [2, 0, 7], [0, -2, 0], [-2, 0, -7]];
        foreach (var step in path)
        {
            await To(Pose(x: step[0], y: step[1], angle: step[2]), 140, Breath);
        }

        await To(Pose(), 380, Bouncy);
    }

    // Signpost: points one way, then the other.
    private async Task PointAsync()
    {
        await To(Pose(x: -2, angle: -11, sy: 1.03), 240, Enter);
        await Task.Delay(450);
        await To(Pose(x: 2, angle: 11, sy: 1.03), 320, Enter);
        await Task.Delay(450);
        await To(Pose(), 380, Snappy);
    }

    // Lock: a calm, slow nod.
    private async Task NodAsync()
    {
        await To(Pose(sx: 1.05, sy: 0.93), 380, Breath);
        await To(Pose(), 520, Bouncy);
    }

    private async Task HopAsync(double height)
    {
        await To(Pose(sx: 1.14, sy: 0.84), 110, Enter);              // crouch
        await To(Pose(y: -height, sx: 0.9, sy: 1.12), 170, Enter);   // launch, stretched
        await To(Pose(y: -height - 1, sx: 1, sy: 1), 90, Enter);      // apex
        await To(Pose(sx: 1.12, sy: 0.86), 150, Fall);               // land, squashed
        await To(Pose(), 380, Bouncy);                               // settle with a wobble
    }

    private async Task ShakeAsync()
    {
        await To(Pose(angle: -12), 120, Enter);
        await To(Pose(angle: 10), 150, Enter);
        await To(Pose(angle: -6), 130, Enter);
        await To(Pose(), 420, Bouncy);
    }

    private async Task LeanAsync()
    {
        var side = _random.Next(2) == 0 ? -1 : 1;
        await To(Pose(x: side * 2, angle: side * 9, sy: 1.03), 260, Enter);
        await Task.Delay(700);
        await To(Pose(), 360, Snappy);
    }

    private async Task To(TransformOperations pose, int ms, Easing easing)
    {
        _icon.Transitions =
        [
            new TransformOperationsTransition
            {
                Property = Visual.RenderTransformProperty,
                Duration = TimeSpan.FromMilliseconds(ms),
                Easing = easing,
            },
        ];
        _icon.RenderTransform = pose;
        await Task.Delay(ms);
    }

    // One fixed operation layout (translate, rotate, scale) so every pose interpolates with every other.
    private static TransformOperations Pose(double x = 0, double y = 0, double angle = 0, double sx = 1, double sy = 1)
    {
        var builder = TransformOperations.CreateBuilder(3);
        builder.AppendTranslate(x, y);
        builder.AppendRotate(angle * Math.PI / 180);
        builder.AppendScale(sx, sy);
        return builder.Build();
    }
}
