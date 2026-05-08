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

using DevWinUI;
using Windows.System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using WinRT.Interop;

namespace AuraClick.Helpers;

internal static class HotkeyManager
{
    public static bool RegisterHotkey(int hotkeyId, IEnumerable<object>? keys)
    {
        if (!TryGetHotkey(keys, out var modifiers, out var virtualKey))
        {
            return false;
        }

        var hWnd = new HWND(WindowNative.GetWindowHandle(App.MainWindow));
        UnregisterHotkey(hotkeyId);
        return PInvoke.RegisterHotKey(hWnd, hotkeyId, HotKeyModifiers(modifiers), (uint)virtualKey);
    }

    public static bool UnregisterHotkey(int hotkeyId)
        => PInvoke.UnregisterHotKey(new HWND(WindowNative.GetWindowHandle(App.MainWindow)), hotkeyId);

    private static bool TryGetHotkey(IEnumerable<object>? keys, out VirtualKeyModifiers modifiers, out VirtualKey virtualKey)
    {
        modifiers = VirtualKeyModifiers.None;
        virtualKey = VirtualKey.None;

        if (keys is null)
        {
            return false;
        }

        foreach (var key in keys)
        {
            if (key is not KeyVisualInfo keyInfo || keyInfo.Key is not VirtualKey keyCode)
            {
                continue;
            }

            if (TryGetModifier(keyCode, out var modifier))
            {
                modifiers |= modifier;
            }
            else
            {
                virtualKey = keyCode;
            }
        }

        return virtualKey != VirtualKey.None;
    }

    private static bool TryGetModifier(VirtualKey keyCode, out VirtualKeyModifiers modifier)
    {
        modifier = keyCode switch
        {
            VirtualKey.Control => VirtualKeyModifiers.Control,
            VirtualKey.Shift => VirtualKeyModifiers.Shift,
            VirtualKey.Menu => VirtualKeyModifiers.Menu,
            VirtualKey.LeftWindows => VirtualKeyModifiers.Windows,
            _ => VirtualKeyModifiers.None,
        };

        return modifier != VirtualKeyModifiers.None;
    }

    private static HOT_KEY_MODIFIERS HotKeyModifiers(VirtualKeyModifiers modifiers)
    {
        HOT_KEY_MODIFIERS result = HOT_KEY_MODIFIERS.MOD_NOREPEAT;

        if (modifiers.HasFlag(VirtualKeyModifiers.Control))
        {
            result |= HOT_KEY_MODIFIERS.MOD_CONTROL;
        }

        if (modifiers.HasFlag(VirtualKeyModifiers.Menu))
        {
            result |= HOT_KEY_MODIFIERS.MOD_ALT;
        }

        if (modifiers.HasFlag(VirtualKeyModifiers.Shift))
        {
            result |= HOT_KEY_MODIFIERS.MOD_SHIFT;
        }

        if (modifiers.HasFlag(VirtualKeyModifiers.Windows))
    {
            result |= HOT_KEY_MODIFIERS.MOD_WIN;
        }

        return result;
    }
}
