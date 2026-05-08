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

using Microsoft.UI.Xaml;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Win32;
using Windows.Win32.System.Threading;
using WinUI3Localizer;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AuraClick;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App
{
    /// <summary>
    /// The main window of the application.
    /// </summary>
    public static readonly MainWindow MainWindow = new();

    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();

        PROCESS_POWER_THROTTLING_STATE powerThrottling = new()
        {
            ControlMask = PInvoke.PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
            StateMask = PInvoke.PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
            Version = PInvoke.PROCESS_POWER_THROTTLING_CURRENT_VERSION
        };

        // Set the process power throttling to efficiency mode
        unsafe
        {
            _ = PInvoke.SetProcessInformation(PInvoke.GetCurrentProcess(),
                PROCESS_INFORMATION_CLASS.ProcessPowerThrottling, &powerThrottling,
                (uint)sizeof(PROCESS_POWER_THROTTLING_STATE));
        }

        // Set the process priority to idle
        _ = PInvoke.SetPriorityClass(PInvoke.GetCurrentProcess(), PROCESS_CREATION_FLAGS.IDLE_PRIORITY_CLASS);
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        await InitializeLocalizer();
        await ApplySavedLanguage();

        MainWindow.Activate();
    }

    private static async Task InitializeLocalizer()
    {
        // Initialize a "Strings" folder in the "LocalFolder" for the packaged app.
        StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        StorageFolder stringsFolder = await localFolder.CreateFolderAsync(
            "Strings",
            CreationCollisionOption.OpenIfExists);

        // Create string resources file from app resources if they don't exist.
        string resourceFileName = "Resources.resw";
        StorageFolder installedStringsFolder = await Package.Current.InstalledLocation.GetFolderAsync(stringsFolder.Name);
        IReadOnlyList<StorageFolder> languageFolders = await installedStringsFolder.GetFoldersAsync();

        foreach (StorageFolder languageFolder in languageFolders)
        {
            await CreateStringResourceFileIfNotExists(stringsFolder, languageFolder.Name, resourceFileName);
        }

        ILocalizer localizer = await new LocalizerBuilder()
            .AddStringResourcesFolderForLanguageDictionaries(stringsFolder.Path)
            .SetOptions(options =>
            {
                options.DefaultLanguage = "en-US";
            })
            .Build();
    }

    private static async Task CreateStringResourceFileIfNotExists(StorageFolder stringsFolder, string language, string resourceFileName)
    {
        StorageFolder languageFolder = await stringsFolder.CreateFolderAsync(
            language,
            CreationCollisionOption.OpenIfExists);

        if (await languageFolder.TryGetItemAsync(resourceFileName) is null)
        {
            string resourceFilePath = Path.Combine(stringsFolder.Name, language, resourceFileName);
            StorageFile resourceFile = await LoadStringResourcesFileFromAppResource(resourceFilePath);
            _ = await resourceFile.CopyAsync(languageFolder);
        }
    }

    private static async Task<StorageFile> LoadStringResourcesFileFromAppResource(string filePath)
    {
        Uri resourcesFileUri = new($"ms-appx:///{filePath}");
        return await StorageFile.GetFileFromApplicationUriAsync(resourcesFileUri);
    }

    private static async Task ApplySavedLanguage()
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        if (localSettings.Values["LanguageSelectedIndex"] is int savedIndex)
        {
            var languages = Localizer.Get()
                .GetAvailableLanguages()
                .OrderBy(x => new System.Globalization.CultureInfo(x).DisplayName)
                .ToList();

            if (savedIndex >= 0 && savedIndex < languages.Count)
            {
                await Localizer.Get().SetLanguage(languages[savedIndex]);
            }
        }
    }
}