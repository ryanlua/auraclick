// Copyright (C) 2025 Ryan Luu
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
using Microsoft.UI.Xaml.Navigation;
using WinUIEx;
using TitleBar = Microsoft.UI.Xaml.Controls.TitleBar;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AuraClick;

/// <summary>
/// An window that displays a page's contents.
/// </summary>
public sealed partial class MainWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow" /> class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        // Set up window
        Title = "AppDisplayName".GetLocalized();
        AppWindow.SetIcon("Assets/WindowIcon.ico");

        // Set up window title bar
        ExtendsContentIntoTitleBar = true;

        // Set up frame
        _ = NavFrame.Navigate(typeof(MainPage));

        // Center the window on the screen
        this.CenterOnScreen();
    }

    /// <summary>
    /// Handles the BackRequested event of the AppTitleBar control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">The event data.</param>
    private void AppTitleBar_BackRequested(TitleBar sender, object args)
    {
        if (NavFrame.CanGoBack)
        {
            NavFrame.GoBack();
        }
    }

    /// <summary>
    /// Handles the Navigated event of the NavFrame control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void NavFrame_Navigated(object sender, NavigationEventArgs e)
    {
        AppTitleBar.IsBackButtonVisible = e.SourcePageType != typeof(MainPage);
    }
}