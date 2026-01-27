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
/// Helper for creating threads to synthesize mouse input.
/// </summary>
public static class AutoClicker
{
    public static int millisecondsDelay = 100;
    public static int secondsDelay = 0;
    public static int minutesDelay = 0;
    public static int hoursDelay = 0;
    public static int clickAmount = 100;
    public static int mouseButtonType = 0;
    public static int clickDelayOffset = 10;
    public static bool clickAmountEnabled = false;
    public static bool clickDelayOffsetEnabled = false;

    private static Thread? _autoClickerThread;
    private static bool _isAutoClickerRunning;

    /// <summary>
    /// Starts the auto clicker thread.
    /// </summary>
    public static void Start()
    {
        _isAutoClickerRunning = true;
        _autoClickerThread = new Thread(AutoClickerThread);
        _autoClickerThread.Start();
    }

    /// <summary>
    /// Stops the auto clicker thread.
    /// </summary>
    public static void Stop()
    {
        _isAutoClickerRunning = false;
        _autoClickerThread?.Join();
    }

    private static async void AutoClickerThread()
    {
        int clickCount = 0;
        int effectiveClickAmount = clickAmountEnabled ? clickAmount : 0;

        while (_isAutoClickerRunning)
        {
            // Stop if we click more than repeat amount (only if enabled)
            if (effectiveClickAmount > 0 && clickCount >= effectiveClickAmount)
            {
                Stop();
                break;
            }

            // Click mouse and increment click count
            ClickMouse(mouseButtonType);
            clickCount++;

            // Delay before next click
            int effectiveClickDelayOffset = clickDelayOffsetEnabled ? clickDelayOffset : 0;
            int randomClickOffset = effectiveClickDelayOffset > 0 ? new Random().Next(0, effectiveClickDelayOffset) : 0;

            int clickDelay = millisecondsDelay
                             + (secondsDelay * 1000)
                             + (minutesDelay * 60 * 1000)
                             + (hoursDelay * 60 * 60 * 1000)
                             + randomClickOffset;
            await Task.Delay(clickDelay);
        }
    }

    /// <summary>
    /// Clicks the mouse button.
    /// </summary>
    /// <param name="mouseButton">The mouse button to click.</param>
    private static void ClickMouse(int mouseButton)
    {
        switch (mouseButton)
        {
            // Left mouse button
            case 0:
                SendMouseInput(MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTDOWN);
                SendMouseInput(MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTUP);
                break;
            // Middle mouse button
            case 1:
                SendMouseInput(MOUSE_EVENT_FLAGS.MOUSEEVENTF_MIDDLEDOWN);
                SendMouseInput(MOUSE_EVENT_FLAGS.MOUSEEVENTF_MIDDLEUP);
                break;
            // Right mouse button
            case 2:
                SendMouseInput(MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTDOWN);
                SendMouseInput(MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTUP);
                break;
        }
    }

    /// <summary>
    /// Sends a mouse input event.
    /// </summary>
    /// <param name="dwFlags">The mouse event flags that specify the type of mouse event.</param>
    private static void SendMouseInput(MOUSE_EVENT_FLAGS dwFlags)
    {
        INPUT[] inputs =
        [
            new()
            {
                type = INPUT_TYPE.INPUT_MOUSE,
                Anonymous = new INPUT._Anonymous_e__Union { mi = new MOUSEINPUT { dwFlags = dwFlags } }
            }
        ];

        _ = PInvoke.SendInput(inputs, Marshal.SizeOf<INPUT>());
    }
}