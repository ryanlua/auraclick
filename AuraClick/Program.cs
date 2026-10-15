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
using Microsoft.UI.Xaml.Settings;
using Microsoft.Windows.AppLifecycle;

namespace AuraClick;

public partial class Program : SingleInstanceApp
{
    [STAThread]
    private static int Main(string[] args)
    {
        // Optional XAML performance changes
        XamlOptionalChanges.EnableChange(XamlChangeId.IconNoGridOptimization);
        XamlOptionalChanges.EnableChange(XamlChangeId.OptimizeApplyStyles);
        XamlOptionalChanges.EnableChange(XamlChangeId.DefaultStyleOptimizations);
        XamlOptionalChanges.EnableChange(XamlChangeId.DeferContextFlyoutInit);

        // Make app single-instanced
        return Run(args, "AuraClickApp", () => new Program(), () => new App());
    }

    protected override void OnActivated(AppActivationArguments args)
    {
        SingleInstanceWindowActivator.Activate();
    }
}