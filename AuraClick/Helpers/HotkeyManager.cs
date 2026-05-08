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
    private static readonly HWND hWnd = new(WindowNative.GetWindowHandle(App.MainWindow));

    public static bool RegisterHotkey(int hotkeyId, IEnumerable<object> keys)
    {
        (VirtualKeyModifiers modifiers, VirtualKey virtualKey) = GetHotkey(keys);
        UnregisterHotkey(hotkeyId);
        return PInvoke.RegisterHotKey(hWnd, hotkeyId, HotKeyModifiers(modifiers), (uint)virtualKey);
    }

    public static bool UnregisterHotkey(int hotkeyId) => PInvoke.UnregisterHotKey(hWnd, hotkeyId);

    private static (VirtualKeyModifiers Modifiers, VirtualKey VirtualKey) GetHotkey(IEnumerable<object> keys)
    {
        VirtualKeyModifiers modifiers = VirtualKeyModifiers.None;
        VirtualKey virtualKey = VirtualKey.None;

        foreach (var key in keys)
        {
            if (key is not KeyVisualInfo { Key: VirtualKey keyCode })
                continue;

            switch (keyCode)
            {
                case VirtualKey.Control:
                    modifiers |= VirtualKeyModifiers.Control;
                    break;
                case VirtualKey.Shift:
                    modifiers |= VirtualKeyModifiers.Shift;
                    break;
                case VirtualKey.Menu:
                    modifiers |= VirtualKeyModifiers.Menu;
                    break;
                case VirtualKey.LeftWindows:
                    modifiers |= VirtualKeyModifiers.Windows;
                    break;
                default:
                    virtualKey = keyCode;
                    break;
            }
        }

        return (modifiers, virtualKey);
    }

    private static HOT_KEY_MODIFIERS HotKeyModifiers(VirtualKeyModifiers modifiers)
    {
        HOT_KEY_MODIFIERS result = HOT_KEY_MODIFIERS.MOD_NOREPEAT;

        if (modifiers.HasFlag(VirtualKeyModifiers.Control))
            result |= HOT_KEY_MODIFIERS.MOD_CONTROL;
        if (modifiers.HasFlag(VirtualKeyModifiers.Menu))
            result |= HOT_KEY_MODIFIERS.MOD_ALT;
        if (modifiers.HasFlag(VirtualKeyModifiers.Shift))
            result |= HOT_KEY_MODIFIERS.MOD_SHIFT;
        if (modifiers.HasFlag(VirtualKeyModifiers.Windows))
            result |= HOT_KEY_MODIFIERS.MOD_WIN;

        return result;
    }
}
