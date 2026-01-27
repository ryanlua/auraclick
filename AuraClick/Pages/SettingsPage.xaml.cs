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
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel;
using Windows.Storage;
using WinUIEx;
using SystemBackdrop = Microsoft.UI.Xaml.Media.SystemBackdrop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AuraClick;

/// <summary>
/// Represents the settings page.
/// </summary>
public sealed partial class SettingsPage
{
    /// <summary>
    /// Application settings container in the local app data store.
    /// </summary>
    private readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsPage" /> class.
    /// </summary>
    public SettingsPage()
    {
        InitializeComponent();

        // Initialize saved settings
        ThemeSelectedIndex = ThemeSelectedIndex;
        BackdropSelectedIndex = BackdropSelectedIndex;
        IsAlwaysOnTop = IsAlwaysOnTop;

        // Initialize app name and version
        AppAboutSettingsExpander.Header = AppName;
        AppVersionTextBlock.Text = AppVersion;
    }

    /// <summary>
    /// Gets or sets the if the playing notification badge is enabled.
    /// </summary>
    public bool NotificationBadgePlaying
    {
        get => (bool)(localSettings.Values[nameof(NotificationBadgePlaying)] ?? true);
        set => localSettings.Values[nameof(NotificationBadgePlaying)] = value;
    }

    /// <summary>
    /// Gets or sets the if the paused notification badge is enabled.
    /// </summary>
    public bool NotificationBadgePaused
    {
        get => (bool)(localSettings.Values[nameof(NotificationBadgePaused)] ?? true);
        set => localSettings.Values[nameof(NotificationBadgePaused)] = value;
    }

    /// <summary>
    /// Gets the localized application name.
    /// </summary>
    private static string AppName => "AppDisplayName".GetLocalized();

    /// <summary>
    /// Gets the application version.
    /// </summary>
    private static string AppVersion
    {
        get
        {
            PackageVersion version = Package.Current.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }

    /// <summary>
    /// Gets the main application window.
    /// </summary>
    private static WindowEx MainWindow => App.MainWindow;

    /// <summary>
    /// Gets or sets the selected theme index.
    /// </summary>
    private int ThemeSelectedIndex
    {
        get => (int)(localSettings.Values[nameof(ThemeSelectedIndex)] ?? 0);
        set
        {
            localSettings.Values[nameof(ThemeSelectedIndex)] = value;
            ((FrameworkElement)MainWindow.Content).RequestedTheme = value switch
            {
                1 => ElementTheme.Light,
                2 => ElementTheme.Dark,
                _ => ElementTheme.Default
            };
        }
    }

    /// <summary>
    /// Gets or sets the selected backdrop index.
    /// </summary>
    private int BackdropSelectedIndex
    {
        get => (int)(localSettings.Values[nameof(BackdropSelectedIndex)] ?? 0);
        set
        {
            localSettings.Values[nameof(BackdropSelectedIndex)] = value;

            // HACK: Prevent changing the backdrop so it doesn't flash between pages
            SystemBackdrop currentBackdrop = MainWindow.SystemBackdrop;
            bool needsChange = value switch
            {
                1 => currentBackdrop is not MicaBackdrop { Kind: MicaKind.BaseAlt },
                2 => currentBackdrop is not DesktopAcrylicBackdrop,
                _ => currentBackdrop is not MicaBackdrop ||
                     (currentBackdrop is MicaBackdrop mica && mica.Kind != MicaKind.Base)
            };

            if (needsChange)
            {
                MainWindow.SystemBackdrop = value switch
                {
                    1 => new MicaBackdrop { Kind = MicaKind.BaseAlt },
                    2 => new DesktopAcrylicBackdrop(),
                    _ => new MicaBackdrop()
                };
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the application window is always on top.
    /// </summary>
    private bool IsAlwaysOnTop
    {
        get => (bool)(localSettings.Values[nameof(IsAlwaysOnTop)] ?? true);
        set
        {
            localSettings.Values[nameof(IsAlwaysOnTop)] = value;
            MainWindow.IsAlwaysOnTop = value;
        }
    }
}