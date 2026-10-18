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
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Globalization;
using Windows.ApplicationModel;
using Windows.Globalization;
using Windows.Storage;
using WinUIEx;
using AppInstance = Microsoft.Windows.AppLifecycle.AppInstance;
using SystemBackdrop = Microsoft.UI.Xaml.Media.SystemBackdrop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AuraClick;

/// <summary>
/// Represents the settings page.
/// </summary>
public sealed partial class SettingsPage : Page
{
    /// <summary>
    /// Application settings container in the local app data store.
    /// </summary>
    private readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

    /// <summary>
    /// Languages the app is localized into.
    /// </summary>
    private static readonly List<string> Languages = [.. ApplicationLanguages.ManifestLanguages];

    /// <summary>
    /// Language override the app was started with; resources only reload on restart.
    /// </summary>
    private static readonly string StartupLanguageOverride = ApplicationLanguages.PrimaryLanguageOverride;

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

        foreach (string language in Languages)
        {
            LanguageComboBox.Items.Add(CultureInfo.GetCultureInfo(language).NativeName);
        }

        LanguageRestartInfoBar.IsOpen = IsRestartRequired;

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
        set
        {
            localSettings.Values[nameof(NotificationBadgePlaying)] = value;
            MainPage.RefreshNotificationBadge();
        }
    }

    /// <summary>
    /// Gets or sets the if the paused notification badge is enabled.
    /// </summary>
    public bool NotificationBadgePaused
    {
        get => (bool)(localSettings.Values[nameof(NotificationBadgePaused)] ?? true);
        set
        {
            localSettings.Values[nameof(NotificationBadgePaused)] = value;
            MainPage.RefreshNotificationBadge();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the taskbar button shows a progress indicator while clicking.
    /// </summary>
    public bool ProgressIndicator
    {
        get => (bool)(localSettings.Values[nameof(ProgressIndicator)] ?? true);
        set
        {
            localSettings.Values[nameof(ProgressIndicator)] = value;
            MainPage.RefreshProgressIndicator();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the window is minimized when clicking starts.
    /// </summary>
    public bool AutomaticMinimizeMinimize
    {
        get => (bool)(localSettings.Values[nameof(AutomaticMinimizeMinimize)] ?? false);
        set => localSettings.Values[nameof(AutomaticMinimizeMinimize)] = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the window is unminimized when clicking stops.
    /// </summary>
    public bool AutomaticMinimizeRestore
    {
        get => (bool)(localSettings.Values[nameof(AutomaticMinimizeRestore)] ?? false);
        set => localSettings.Values[nameof(AutomaticMinimizeRestore)] = value;
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
    /// Gets or sets the selected language index, where 0 follows the system language.
    /// </summary>
    private int LanguageSelectedIndex
    {
        get => Languages.FindIndex(language => language.Equals(ApplicationLanguages.PrimaryLanguageOverride, StringComparison.OrdinalIgnoreCase)) + 1;
        set
        {
            if (value >= 0 && value != LanguageSelectedIndex)
            {
                ApplicationLanguages.PrimaryLanguageOverride = value == 0 ? string.Empty : Languages[value - 1];
                LanguageRestartInfoBar.IsOpen = IsRestartRequired;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the selected language differs from the one the app started with.
    /// </summary>
    private static bool IsRestartRequired =>
        !ApplicationLanguages.PrimaryLanguageOverride.Equals(StartupLanguageOverride, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Handles the Click event of the language restart button.
    /// </summary>
    private void LanguageRestartButton_Click(object sender, RoutedEventArgs e)
    {
        _ = AppInstance.Restart(string.Empty);
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