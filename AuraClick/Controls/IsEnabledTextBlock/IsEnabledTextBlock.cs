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
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AuraClick.Controls;

/// <summary>
/// A custom control that displays text and changes its visual state based on its enabled state.
/// </summary>
[TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
[TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]
public partial class IsEnabledTextBlock : Control
{
    /// <summary>
    /// Identifies the <see cref="Text" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        "Text",
        typeof(string),
        typeof(IsEnabledTextBlock),
        null);

    /// <summary>
    /// Initializes a new instance of the <see cref="IsEnabledTextBlock" /> class.
    /// </summary>
    public IsEnabledTextBlock()
    {
        DefaultStyleKey = typeof(IsEnabledTextBlock);
    }

    /// <summary>
    /// Gets or sets the text content of the control.
    /// </summary>
    [Localizable(true)]
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Applies the control template and sets up the initial visual state.
    /// </summary>
    protected override void OnApplyTemplate()
    {
        IsEnabledChanged -= IsEnabledTextBlock_IsEnabledChanged;
        SetEnabledState();
        IsEnabledChanged += IsEnabledTextBlock_IsEnabledChanged;
        base.OnApplyTemplate();
    }

    /// <summary>
    /// Handles the IsEnabledChanged event and updates the visual state.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void IsEnabledTextBlock_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        SetEnabledState();
    }

    /// <summary>
    /// Sets the visual state of the control based on its enabled state.
    /// </summary>
    private void SetEnabledState()
    {
        _ = VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", true);
    }
}