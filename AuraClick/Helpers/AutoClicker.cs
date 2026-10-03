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
// but WITHOUT ANY WARRANTY without even the implied warranty of
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
    private static CancellationTokenSource? _cts;

    /// <summary>
    /// Raised on a background thread when the click limit is reached.
    /// </summary>
    public static event EventHandler? ClickLimitReached;

    public static int HoursDelay { get; set; }

    public static int MinutesDelay { get; set; }

    public static int SecondsDelay { get; set; }

    public static int MillisecondsDelay { get; set; } = 100;

    public static int MouseButton { get; set; }

    public static bool ClickAmountEnabled { get; set; }

    public static int ClickAmount { get; set; } = 100;

    public static bool ClickDelayOffsetEnabled { get; set; }

    public static int ClickDelayOffset { get; set; } = 10;

    /// <summary>
    /// Gets a value indicating whether the auto clicker is currently running.
    /// </summary>
    public static bool IsRunning => _cts is { IsCancellationRequested: false };

    /// <summary>
    /// Starts clicking with a snapshot of the current settings, stopping any previous run.
    /// </summary>
    public static void Start()
    {
        Stop();
        CancellationTokenSource cts = _cts = new();

        INPUT[] click = CreateClickInputs(MouseButton);
        long clickLimit = ClickAmountEnabled ? ClickAmount : long.MaxValue;
        TimeSpan delay = TimeSpan.FromHours(HoursDelay, MinutesDelay, SecondsDelay, MillisecondsDelay);
        int maxOffset = ClickDelayOffsetEnabled ? Math.Max(0, ClickDelayOffset) : 0;

        _ = Task.Run(async () =>
        {
            for (long clicks = 1; !cts.IsCancellationRequested; clicks++)
            {
                _ = PInvoke.SendInput(click, Marshal.SizeOf<INPUT>());
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
        _cts?.Cancel();
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

    private static INPUT MouseInput(MOUSE_EVENT_FLAGS flags)
    {
        return new() { type = INPUT_TYPE.INPUT_MOUSE, Anonymous = new() { mi = new() { dwFlags = flags } } };
    }
}
