// Copyright (C) 2026 Ryan Luu
//
// This file is part of Aura Click.
//
// Aura Click is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Aura Click is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Aura Click. If not, see <https://www.gnu.org/licenses/>.

using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace AuraClick.Helpers;

/// <summary>
/// Synthesizes mouse clicks on a background task.
/// </summary>
public static class AutoClicker
{
    private static CancellationTokenSource? s_cts;

    /// <summary>
    /// Raised on a background thread when the click limit is reached.
    /// </summary>
    public static event EventHandler? ClickLimitReached;

    /// <summary>
    /// Gets or sets the hours part of the click interval.
    /// </summary>
    public static int HoursDelay { get; set; }

    /// <summary>
    /// Gets or sets the minutes part of the click interval.
    /// </summary>
    public static int MinutesDelay { get; set; }

    /// <summary>
    /// Gets or sets the seconds part of the click interval.
    /// </summary>
    public static int SecondsDelay { get; set; }

    /// <summary>
    /// Gets or sets the milliseconds part of the click interval.
    /// </summary>
    public static int MillisecondsDelay { get; set; } = 100;

    /// <summary>
    /// Gets or sets the mouse button to click: 0 for left, 1 for middle, 2 for right.
    /// </summary>
    public static int MouseButton { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether clicking stops after <see cref="ClickAmount" /> clicks.
    /// </summary>
    public static bool ClickAmountEnabled { get; set; }

    /// <summary>
    /// Gets or sets the number of clicks before clicking stops.
    /// </summary>
    public static int ClickAmount { get; set; } = 100;

    /// <summary>
    /// Gets or sets a value indicating whether a random offset is added to the click interval.
    /// </summary>
    public static bool ClickDelayOffsetEnabled { get; set; }

    /// <summary>
    /// Gets or sets the maximum random offset, in milliseconds, added to the click interval.
    /// </summary>
    public static int ClickDelayOffset { get; set; } = 10;

    /// <summary>
    /// Gets or sets the handle of the window to click in, or 0 to click wherever the cursor is.
    /// When set, the window is brought to the front for each click and the previous window is restored afterwards.
    /// </summary>
    public static nint TargetWindow { get; set; }

    /// <summary>
    /// Gets a value indicating whether the auto clicker is currently running.
    /// </summary>
    public static bool IsRunning => s_cts is { IsCancellationRequested: false };

    /// <summary>
    /// Starts clicking with a snapshot of the current settings, stopping any previous run.
    /// </summary>
    public static void Start()
    {
        Stop();
        CancellationTokenSource cts = s_cts = new();

        INPUT[] click = CreateClickInputs(MouseButton);
        long clickLimit = ClickAmountEnabled ? ClickAmount : long.MaxValue;
        TimeSpan delay = TimeSpan.FromHours(HoursDelay, MinutesDelay, SecondsDelay, MillisecondsDelay);
        int maxOffset = ClickDelayOffsetEnabled ? Math.Max(0, ClickDelayOffset) : 0;
        nint target = TargetWindow;

        _ = Task.Run(async () =>
        {
            for (long clicks = 1; !cts.IsCancellationRequested; clicks++)
            {
                if (target != 0)
                {
                    if (!WindowHelper.Exists(target))
                    {
                        // Target window was closed
                        cts.Cancel();
                        ClickLimitReached?.Invoke(null, EventArgs.Empty);
                        return;
                    }

                    // Games ignore background input, so focus the window, press, hold briefly, release, then go back
                    nint previous = WindowHelper.BringToFront(target);
                    await Task.Delay(80).ConfigureAwait(false);
                    _ = PInvoke.SendInput(click.AsSpan(0, 1), Marshal.SizeOf<INPUT>());
                    await Task.Delay(60).ConfigureAwait(false);
                    _ = PInvoke.SendInput(click.AsSpan(1, 1), Marshal.SizeOf<INPUT>());
                    WindowHelper.Restore(previous, target);
                }
                else
                {
                    _ = PInvoke.SendInput(click, Marshal.SizeOf<INPUT>());
                }

                if (clicks >= clickLimit)
                {
                    cts.Cancel();
                    ClickLimitReached?.Invoke(null, EventArgs.Empty);
                    return;
                }

                TimeSpan offset = TimeSpan.FromMilliseconds(Random.Shared.NextInt64(maxOffset + 1L));
                await Task.Delay(delay + offset, cts.Token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }
        });
    }

    /// <summary>
    /// Stops the auto clicker.
    /// </summary>
    public static void Stop()
    {
        s_cts?.Cancel();
    }

    /// <summary>
    /// Creates the press and release inputs for a single click of the given mouse button.
    /// </summary>
    /// <param name="mouseButton">The mouse button to click: 0 for left, 1 for middle, 2 for right.</param>
    private static INPUT[] CreateClickInputs(int mouseButton)
    {
        (MOUSE_EVENT_FLAGS down, MOUSE_EVENT_FLAGS up) = mouseButton switch
        {
            1 => (MOUSE_EVENT_FLAGS.MOUSEEVENTF_MIDDLEDOWN, MOUSE_EVENT_FLAGS.MOUSEEVENTF_MIDDLEUP),
            2 => (MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTDOWN, MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTUP),
            _ => (MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTDOWN, MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTUP),
        };
        return [MouseInput(down), MouseInput(up)];
    }

    /// <summary>
    /// Creates a mouse input with the given event flags.
    /// </summary>
    private static INPUT MouseInput(MOUSE_EVENT_FLAGS flags)
    {
        return new() { type = INPUT_TYPE.INPUT_MOUSE, Anonymous = new() { mi = new() { dwFlags = flags } } };
    }
}
