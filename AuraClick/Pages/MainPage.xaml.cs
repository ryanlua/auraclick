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

using AuraClick.Helpers;
using DevWinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.Windows.BadgeNotifications;
using Windows.System;
using Windows.Win32;
using Windows.Win32.Foundation;
using WinRT.Interop;
using WinUIEx;
using WindowMessageEventArgs = WinUIEx.Messaging.WindowMessageEventArgs;
using WindowMessageMonitor = WinUIEx.Messaging.WindowMessageMonitor;
using WindowState = WinUIEx.WindowState;

namespace AuraClick;

/// <summary>
/// The main content page displayed inside the application window.
/// Add your UI logic, event handlers, and data binding here.
/// </summary>
public sealed partial class MainPage : Page
{
    /// <summary>
    /// The settings page instance.
    /// </summary>
    private static readonly SettingsPage s_settingsPage = new();

    /// <summary>
    /// Keys for the stop shortcut, which always stops the auto clicker.
    /// </summary>
    private static readonly List<object> s_stopShortcut = [new KeyVisualInfo { Key = VirtualKey.End, KeyName = "End" }];

    /// <summary>
    /// Window message monitor for hotkey handling.
    /// </summary>
    private WindowMessageMonitor? _monitor;

    /// <summary>
    /// Hotkey ID for the toggle shortcut.
    /// </summary>
    private const int ToggleHotkeyId = 1;

    /// <summary>
    /// Hotkey ID for the stop shortcut.
    /// </summary>
    private const int StopHotkeyId = 2;

    /// <summary>
    /// Flag to track if the ToggleShortcut dialog is open.
    /// </summary>
    private bool _shortcutDialogOpen;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainPage" /> class.
    /// </summary>
    public MainPage()
    {
        InitializeComponent();
        Loaded += MainPage_Loaded;
        AutoClicker.ClickLimitReached += (_, _) => DispatcherQueue.TryEnqueue(() => StartToggleButton.IsChecked = AutoClicker.IsRunning);

        ToggleShortcut.Keys = CreateDefaultShortcut();
    }

    /// <summary>
    /// Creates the default hotkey keys.
    /// </summary>
    private static List<object> CreateDefaultShortcut()
    {
        return [new KeyVisualInfo { Key = VirtualKey.F6, KeyName = "F6" }];
    }

    /// <summary>
    /// Shows a dialog explaining that the hotkey couldn't be registered.
    /// </summary>
    private async Task ShowHotkeyRegistrationFailedAsync()
    {
        ContentDialog dialog = new()
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "ContentDialogHotkeyFailed/Title".GetLocalized(),
            Content = "ContentDialogHotkeyFailed/Content".GetLocalized(),
            CloseButtonText = "ContentDialogHotkeyFailed/CloseButtonText".GetLocalized(),
            DefaultButton = ContentDialogButton.Close
        };

        _ = await dialog.ShowAsync();
    }

    /// <summary>
    /// Registers the selected hotkey and shows an error if registration fails.
    /// </summary>
    private async Task ApplyHotkeySelectionAsync()
    {
        bool registered = HotkeyManager.RegisterHotkey(ToggleHotkeyId, ToggleShortcut.Keys);
        SetShortcutError(!registered);

        if (!registered)
        {
            await ShowHotkeyRegistrationFailedAsync();
        }
    }

    /// <summary>
    /// Shows or clears the error state on the hotkey's key visuals.
    /// </summary>
    private void SetShortcutError(bool isError)
    {
        // Shortcut doesn't expose an error state, and its key visuals are regenerated whenever its keys change.
        ToggleShortcut.UpdateLayout();
        foreach (KeyVisual keyVisual in ToggleShortcut.FindDescendants().OfType<KeyVisual>())
        {
            keyVisual.IsError = isError;
        }

        ToolTipService.SetToolTip(ToggleShortcut, isError ? "ContentDialogHotkeyFailed/Content".GetLocalized() : null);
    }

    /// <summary>
    /// Handles the Loaded event of the MainPage control.
    /// </summary>
    private async void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Set badge notification
        RefreshNotificationBadge();

        // Set up window message monitor
        if (_monitor is null)
        {
            MainWindow window = App.MainWindow;
            _monitor = new(window);
            _monitor.WindowMessageReceived += OnWindowMessageReceived;
        }

        // Register hotkey
        await ApplyHotkeySelectionAsync();

        if (s_settingsPage.IsStartButtonTeachingTipDismissed)
        {
            return;
        }

        // Delay toggle button teaching tooltip until page transition is complete
        await Task.Delay(500);
        StartButtonTeachingTip.IsOpen = true;
    }

    /// <summary>
    /// Handles the Closed event of the StartButtonTeachingTip control.
    /// </summary>
    private void StartButtonTeachingTip_Closed(TeachingTip sender, TeachingTipClosedEventArgs args)
    {
        if (args.Reason != TeachingTipCloseReason.Programmatic)
        {
            s_settingsPage.IsStartButtonTeachingTipDismissed = true;
        }
    }

    /// <summary>
    /// Handles the WindowMessageReceived event of the window message monitor.
    /// </summary>
    private void OnWindowMessageReceived(object? sender, WindowMessageEventArgs e)
    {
        // Stop hotkey works regardless of the current page or dialog
        if (e.Message.MessageId == PInvoke.WM_HOTKEY && e.Message.WParam == StopHotkeyId)
        {
            StartToggleButton.IsChecked = false;
            AutoClicker.Stop();
            return;
        }

        // Allow hotkey only on main page
        if (Frame.Content is not MainPage)
        {
            return;
        }

        // Disable hotkey when ToggleShortcut dialog is open
        if (_shortcutDialogOpen)
        {
            return;
        }

        if (e.Message.MessageId == PInvoke.WM_HOTKEY && e.Message.WParam == ToggleHotkeyId)
        {
            StartToggleButton.IsChecked = !StartToggleButton.IsChecked;
        }
    }

    /// <summary>
    /// Refreshes the notification badge using the current application state and settings.
    /// </summary>
    public static void RefreshNotificationBadge()
    {
        if (AutoClicker.IsRunning && s_settingsPage.NotificationBadgePlaying)
        {
            BadgeNotificationManager.Current.SetBadgeAsGlyph(BadgeNotificationGlyph.Playing);
        }
        else if (!AutoClicker.IsRunning && s_settingsPage.NotificationBadgePaused)
        {
            BadgeNotificationManager.Current.SetBadgeAsGlyph(BadgeNotificationGlyph.Paused);
        }
        else
        {
            BadgeNotificationManager.Current.ClearBadge();
        }
    }

    /// <summary>
    /// Refreshes the taskbar progress indicator using the current application state and settings.
    /// </summary>
    public static void RefreshProgressIndicator()
    {
        HWND hwnd = new(WindowNative.GetWindowHandle(App.MainWindow));

        if (AutoClicker.IsRunning && s_settingsPage.ProgressIndicator)
        {
            TaskbarHelper.SetProgressState(hwnd, TaskbarStates.Indeterminate);
        }
        else
        {
            TaskbarHelper.SetProgressState(hwnd, TaskbarStates.NoProgress);
        }
    }

    /// <summary>
    /// Handles the Checked event of the StartToggleButton control.
    /// </summary>
    private async void StartToggleButton_Checked(object sender, RoutedEventArgs e)
    {
        StartToggleButton.IsEnabled = false;

        StartFontIcon.Glyph = "\uEDB4";
        ToolTipService.SetToolTip(StartToggleButton, "ToggleButtonPause/ToolTipService/ToolTip".GetLocalized());
        _ = HotkeyManager.RegisterHotkey(StopHotkeyId, s_stopShortcut);
        AutoClicker.Start();
        RefreshNotificationBadge();
        RefreshProgressIndicator();

        if (s_settingsPage.AutomaticMinimizeMinimize)
        {
            App.MainWindow.Minimize();
        }

        await Task.Delay(1000);
        StartToggleButton.IsEnabled = true;
    }

    /// <summary>
    /// Handles the Unchecked event of the StartToggleButton control.
    /// </summary>
    private void StartToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
        StartToggleButton.IsEnabled = true;
        StartFontIcon.Glyph = "\uEE4A";
        ToolTipService.SetToolTip(StartToggleButton, "ToggleButtonStart/ToolTipService/ToolTip".GetLocalized());
        AutoClicker.Stop();
        _ = HotkeyManager.UnregisterHotkey(StopHotkeyId);
        RefreshNotificationBadge();
        RefreshProgressIndicator();

        // Restore would also un-maximize a maximized window, so only restore when minimized
        if (s_settingsPage.AutomaticMinimizeRestore && App.MainWindow.WindowState == WindowState.Minimized)
        {
            App.MainWindow.Restore();
        }
    }

    /// <summary>
    /// Handles the Click event of the SettingsButton control.
    /// </summary>
    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        _ = Frame.Navigate(typeof(SettingsPage), null, new SuppressNavigationTransitionInfo());
    }

    /// <summary>
    /// Handles the PrimaryButtonClick event of the ToggleShortcut control.
    /// </summary>
    private async void ToggleShortcut_PrimaryButtonClick(object sender, ContentDialogButtonClickEventArgs e)
    {
        ToggleShortcut.UpdatePreviewKeys();
        ToggleShortcut.CloseContentDialog();
        await ApplyHotkeySelectionAsync();
    }

    /// <summary>
    /// Handles the SecondaryButtonClick event of the ToggleShortcut control.
    /// </summary>
    private async void ToggleShortcut_SecondaryButtonClick(object sender, ContentDialogButtonClickEventArgs e)
    {
        ToggleShortcut.Keys = CreateDefaultShortcut();
        ToggleShortcut.UpdatePreviewKeys();
        ToggleShortcut.CloseContentDialog();
        await ApplyHotkeySelectionAsync();
    }

    /// <summary>
    /// Handles the OpenedContentDialog event of the ToggleShortcut control.
    /// </summary>
    private void ToggleShortcut_OpenedContentDialog(object sender, ContentDialogOpenedEventArgs e)
    {
        _shortcutDialogOpen = true;
    }

    /// <summary>
    /// Handles the ClosedContentDialog event of the ToggleShortcut control.
    /// </summary>
    private void ToggleShortcut_ClosedContentDialog(object sender, ContentDialogClosedEventArgs e)
    {
        _shortcutDialogOpen = false;
    }
}
