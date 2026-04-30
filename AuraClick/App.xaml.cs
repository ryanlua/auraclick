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
using Windows.Win32;
using Windows.Win32.System.Threading;

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
    /// Initializes the singleton application object.  This is the first line of authored code
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
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow.Activate();
    }
}