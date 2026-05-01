// Copyright (C) 2025 Ryan Luu
//
// This file is part of Aura Click.
//
// Aura Click is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Aura Click is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with Aura Click. If not, see <https://www.gnu.org/licenses/>.

using Windows.System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using WinRT.Interop;

namespace AuraClick.Helpers;

// TODO: Cleanup this code to be more maintainable
internal static class HotkeyManager
{
    public static bool RegisterHotkey(int hotkeyId, IEnumerable<object>? keys)
    {
        if (!TryGetHotkey(keys, out var modifiers, out var virtualKey))
        {
            return false;
        }

        return RegisterHotkey(hotkeyId, modifiers, virtualKey);
    }

    public static bool RegisterHotkey(int hotkeyId, VirtualKeyModifiers modifiers, VirtualKey virtualKey)
    {
        var hWnd = new HWND(WindowNative.GetWindowHandle(App.MainWindow));
        UnregisterHotkey(hotkeyId);
        return PInvoke.RegisterHotKey(hWnd, hotkeyId, ToWin32Modifiers(modifiers) | HOT_KEY_MODIFIERS.MOD_NOREPEAT, (uint)virtualKey);
    }

    public static bool UnregisterHotkey(int hotkeyId)
    {
        var hWnd = new HWND(WindowNative.GetWindowHandle(App.MainWindow));
        return PInvoke.UnregisterHotKey(hWnd, hotkeyId);
    }

    public static bool TryGetHotkey(IEnumerable<object>? keys, out VirtualKeyModifiers modifiers, out VirtualKey virtualKey)
    {
        modifiers = VirtualKeyModifiers.None;
        virtualKey = VirtualKey.F6;

        if (keys is null)
        {
            return false;
        }

        foreach (var keyItem in keys)
        {
            var keyString = keyItem?.ToString();
            if (string.IsNullOrWhiteSpace(keyString))
            {
                continue;
            }

            if (TryAddModifier(keyString, ref modifiers))
            {
                continue;
            }

            if (TryParseVirtualKey(keyString, out var key))
            {
                virtualKey = key;
            }
        }

        return modifiers != VirtualKeyModifiers.None || virtualKey != VirtualKey.F6;
    }

    private static bool TryAddModifier(string keyString, ref VirtualKeyModifiers modifiers)
    {
        if (keyString.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) || keyString.Equals("Control", StringComparison.OrdinalIgnoreCase))
        {
            modifiers |= VirtualKeyModifiers.Control;
            return true;
        }

        if (keyString.Equals("Shift", StringComparison.OrdinalIgnoreCase))
        {
            modifiers |= VirtualKeyModifiers.Shift;
            return true;
        }

        if (keyString.Equals("Alt", StringComparison.OrdinalIgnoreCase) || keyString.Equals("Menu", StringComparison.OrdinalIgnoreCase))
        {
            modifiers |= VirtualKeyModifiers.Menu;
            return true;
        }

        if (keyString.Equals("Win", StringComparison.OrdinalIgnoreCase) || keyString.Equals("Windows", StringComparison.OrdinalIgnoreCase))
        {
            modifiers |= VirtualKeyModifiers.Windows;
            return true;
        }

        return false;
    }

    private static HOT_KEY_MODIFIERS ToWin32Modifiers(VirtualKeyModifiers modifiers)
    {
        HOT_KEY_MODIFIERS result = 0;

        if ((modifiers & VirtualKeyModifiers.Control) != 0) result |= HOT_KEY_MODIFIERS.MOD_CONTROL;
        if ((modifiers & VirtualKeyModifiers.Menu) != 0) result |= HOT_KEY_MODIFIERS.MOD_ALT;
        if ((modifiers & VirtualKeyModifiers.Shift) != 0) result |= HOT_KEY_MODIFIERS.MOD_SHIFT;
        if ((modifiers & VirtualKeyModifiers.Windows) != 0) result |= HOT_KEY_MODIFIERS.MOD_WIN;

        return result;
    }

    private static bool TryParseVirtualKey(string keyString, out VirtualKey key)
    {
        key = VirtualKey.F6;
        var trimmed = keyString.Trim();

        if (trimmed.Length == 0)
        {
            return false;
        }

        if (Enum.TryParse(trimmed, ignoreCase: true, out VirtualKey parsedKey))
        {
            key = parsedKey;
            return true;
        }

        if (trimmed.Length == 1 && char.IsLetterOrDigit(trimmed[0]) && Enum.TryParse(char.ToUpperInvariant(trimmed[0]).ToString(), ignoreCase: true, out parsedKey))
        {
            key = parsedKey;
            return true;
        }

        return trimmed.Equals("Space", StringComparison.OrdinalIgnoreCase) ? Set(out key, VirtualKey.Space)
            : trimmed.Equals("Enter", StringComparison.OrdinalIgnoreCase) || trimmed.Equals("Return", StringComparison.OrdinalIgnoreCase) ? Set(out key, VirtualKey.Enter)
            : trimmed.Equals("Escape", StringComparison.OrdinalIgnoreCase) || trimmed.Equals("Esc", StringComparison.OrdinalIgnoreCase) ? Set(out key, VirtualKey.Escape)
            : trimmed.Equals("Tab", StringComparison.OrdinalIgnoreCase) ? Set(out key, VirtualKey.Tab)
            : trimmed.Equals("Backspace", StringComparison.OrdinalIgnoreCase) || trimmed.Equals("Back", StringComparison.OrdinalIgnoreCase) ? Set(out key, VirtualKey.Back)
            : trimmed.Equals("Delete", StringComparison.OrdinalIgnoreCase) || trimmed.Equals("Del", StringComparison.OrdinalIgnoreCase) ? Set(out key, VirtualKey.Delete)
            : trimmed.Equals("Insert", StringComparison.OrdinalIgnoreCase) || trimmed.Equals("Ins", StringComparison.OrdinalIgnoreCase) ? Set(out key, VirtualKey.Insert)
            : trimmed.Equals("Home", StringComparison.OrdinalIgnoreCase) ? Set(out key, VirtualKey.Home)
            : trimmed.Equals("End", StringComparison.OrdinalIgnoreCase) ? Set(out key, VirtualKey.End)
            : trimmed.Equals("PageUp", StringComparison.OrdinalIgnoreCase) ? Set(out key, VirtualKey.PageUp)
            : trimmed.Equals("PageDown", StringComparison.OrdinalIgnoreCase) ? Set(out key, VirtualKey.PageDown)
            : trimmed.Equals("Up", StringComparison.OrdinalIgnoreCase) ? Set(out key, VirtualKey.Up)
            : trimmed.Equals("Down", StringComparison.OrdinalIgnoreCase) ? Set(out key, VirtualKey.Down)
            : trimmed.Equals("Left", StringComparison.OrdinalIgnoreCase) ? Set(out key, VirtualKey.Left)
            : trimmed.Equals("Right", StringComparison.OrdinalIgnoreCase) ? Set(out key, VirtualKey.Right)
            : trimmed.Equals("Pause", StringComparison.OrdinalIgnoreCase) ? Set(out key, VirtualKey.Pause)
            : trimmed.Equals("PrintScreen", StringComparison.OrdinalIgnoreCase) || trimmed.Equals("Print", StringComparison.OrdinalIgnoreCase) ? Set(out key, VirtualKey.Snapshot)
            : trimmed.Equals("NumLock", StringComparison.OrdinalIgnoreCase) ? Set(out key, VirtualKey.NumberKeyLock)
            : trimmed.Equals("CapsLock", StringComparison.OrdinalIgnoreCase) ? Set(out key, VirtualKey.CapitalLock)
            : trimmed.Equals("ScrollLock", StringComparison.OrdinalIgnoreCase) ? Set(out key, VirtualKey.Scroll)
            : false;
    }

    private static bool Set(out VirtualKey key, VirtualKey value)
    {
        key = value;
        return true;
    }
}
