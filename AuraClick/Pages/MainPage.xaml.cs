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

using AuraClick.Helpers;
using DevWinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.Windows.BadgeNotifications;
using Windows.System;
using Windows.Win32;
using WindowMessageEventArgs = WinUIEx.Messaging.WindowMessageEventArgs;
using WindowMessageMonitor = WinUIEx.Messaging.WindowMessageMonitor;

namespace AuraClick;

/// <summary>
/// The main page containing all controls displayed on the main window.
/// </summary>
public sealed partial class MainPage
{
    /// <summary>
    /// The settings page instance.
    /// </summary>
    private static readonly SettingsPage settingsPage = new();

    /// <summary>
    /// Window message monitor for hotkey handling.
    /// </summary>
    private WindowMessageMonitor? monitor;

    /// <summary>
    /// Hotkey ID for the toggle shortcut.
    /// </summary>
    private const int ToggleHotkeyId = 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainPage" /> class.
    /// </summary>
    public MainPage()
    {
        InitializeComponent();
        Loaded += MainPage_Loaded;

        ToggleShortcut.Keys = CreateDefaultShortcut();
        ToolTipService.SetToolTip(ToggleButtonStart, "ToggleButtonStartTooltipStart".GetLocalized());
    }

    private static List<object> CreateDefaultShortcut()
    {
        return [new KeyVisualInfo { Key = VirtualKey.F6, KeyName = "F6" }];
    }

    /// <summary>
    /// Determines whether a control should be enabled based on a checkbox state and a running-state toggle.
    /// </summary>
    private bool IsControlEnabled(bool? isChecked, bool? isRunning)
    {
        return (isChecked ?? false) && !(isRunning ?? false);
    }

    /// <summary>
    /// Handles the Loaded event of the MainPage control.
    /// </summary>
    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Set badge notification
        SetNotificationBadge(BadgeNotificationGlyph.Paused);

        // Set up window message monitor
        MainWindow window = App.MainWindow;
        monitor = new(window);
        monitor.WindowMessageReceived += OnWindowMessageReceived;

        // Register hotkey
        HotkeyManager.RegisterHotkey(ToggleHotkeyId, ToggleShortcut.Keys);
    }

    /// <summary>
    /// Handles the WindowMessageReceived event of the WindowMessageMonitor control.
    /// </summary>
    private void OnWindowMessageReceived(object? sender, WindowMessageEventArgs e)
    {
        // Allow hotkey only on main page
        if (Frame.Content is not MainPage)
        {
            return;
        }

        if (e.Message.MessageId == PInvoke.WM_HOTKEY && e.Message.WParam == ToggleHotkeyId)
        {
            ToggleButtonStart.IsChecked = !ToggleButtonStart.IsChecked;
        }
    }

    /// <summary>
    /// Refreshes the notification badge using the current application state and settings.
    /// </summary>
    public static void RefreshNotificationBadge()
    {
        SetNotificationBadge(AutoClicker.IsRunning ? BadgeNotificationGlyph.Playing : BadgeNotificationGlyph.Paused);
    }

    /// <summary>
    /// Sets the notification badge.
    /// </summary>
    private static void SetNotificationBadge(BadgeNotificationGlyph glyph)
    {
        if (glyph == BadgeNotificationGlyph.Paused && settingsPage.NotificationBadgePaused)
        {
            BadgeNotificationManager.Current.SetBadgeAsGlyph(glyph);
        }
        else if (glyph == BadgeNotificationGlyph.Playing && settingsPage.NotificationBadgePlaying)
        {
            BadgeNotificationManager.Current.SetBadgeAsGlyph(glyph);
        }
        else
        {
            BadgeNotificationManager.Current.ClearBadge();
        }
    }

    /// <summary>
    /// Handles the Checked event of the ToggleButtonStart control.
    /// </summary>
    private async void ToggleButtonStart_OnChecked(object sender, RoutedEventArgs e)
    {
        ToggleButtonStart.IsEnabled = false;

        FontIconStart.Glyph = "\uEDB4";
        SetNotificationBadge(BadgeNotificationGlyph.Playing);
        ToolTipService.SetToolTip(ToggleButtonStart, "ToggleButtonStartTooltipStop".GetLocalized());
        AutoClicker.Start();

        await Task.Delay(1000);
        ToggleButtonStart.IsEnabled = true;
    }

    /// <summary>
    /// Handles the Unchecked event of the ToggleButtonStart control.
    /// </summary>
    private void ToggleButtonStart_OnUnchecked(object sender, RoutedEventArgs e)
    {
        ToggleButtonStart.IsEnabled = true;
        FontIconStart.Glyph = "\uEE4A";
        SetNotificationBadge(BadgeNotificationGlyph.Paused);
        ToolTipService.SetToolTip(ToggleButtonStart, "ToggleButtonStartTooltipStart".GetLocalized());
        AutoClicker.Stop();
    }

    /// <summary>
    /// Handles the Click event of the SettingsButton control.
    /// </summary>
    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        _ = Frame.Navigate(typeof(SettingsPage), null, new SuppressNavigationTransitionInfo());
    }

    /// <summary>
    /// Handles the PrimaryButtonClick event of the Shortcut control.
    /// </summary>
    private void ToggleShortcut_PrimaryButtonClick(object sender, ContentDialogButtonClickEventArgs e)
    {
        ToggleShortcut.UpdatePreviewKeys();
        ToggleShortcut.CloseContentDialog();
        HotkeyManager.RegisterHotkey(ToggleHotkeyId, ToggleShortcut.Keys);
    }

    /// <summary>
    /// Handles the SecondaryButtonClick event of the Shortcut control.
    /// </summary>
    private void ToggleShortcut_SecondaryButtonClick(object sender, ContentDialogButtonClickEventArgs e)
    {
        ToggleShortcut.Keys = CreateDefaultShortcut();
        ToggleShortcut.UpdatePreviewKeys();
        ToggleShortcut.CloseContentDialog();
        HotkeyManager.RegisterHotkey(ToggleHotkeyId, ToggleShortcut.Keys);
    }
}
