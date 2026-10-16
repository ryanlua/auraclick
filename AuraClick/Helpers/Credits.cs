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

using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace AuraClick.Helpers;

/// <summary>
/// Represents a person credited in the app.
/// </summary>
/// <param name="Name">The username of the person.</param>
/// <param name="ProfilePicture">The profile picture of the person.</param>
public sealed record CreditPerson(string Name, ImageSource ProfilePicture);

/// <summary>
/// Provides the people credited in the app.
/// </summary>
// TODO: Replace the sample data with the generated credits.json (#118).
public static class Credits
{
    /// <summary>
    /// Gets the people who contributed to the GitHub repository.
    /// </summary>
    public static CreditPerson[] Contributors { get; } = [Create("ryanlua", "https://github.com/ryanlua.png?size=80")];

    /// <summary>
    /// Gets the people who translated the app on Crowdin.
    /// </summary>
    public static CreditPerson[] Translators { get; } = CreateSample("translator", 14);

    /// <summary>
    /// Gets the people who boosted the Discord server.
    /// </summary>
    public static CreditPerson[] Boosters { get; } = CreateSample("booster", 4);

    private static CreditPerson Create(string name, string avatarUrl)
    {
        return new(name, new BitmapImage(new Uri(avatarUrl)));
    }

    private static CreditPerson[] CreateSample(string prefix, int count)
    {
        return [.. Enumerable.Range(1, count).Select(i => Create($"{prefix}{i}", $"https://github.com/identicons/{prefix}{i}.png"))];
    }
}
