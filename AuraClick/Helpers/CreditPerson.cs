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
/// <param name="AvatarUri">The location of the profile picture of the person.</param>
public sealed record CreditPerson(string Name, Uri AvatarUri)
{
    /// <summary>
    /// Gets the profile picture of the person.
    /// </summary>
    public ImageSource ProfilePicture { get; } = new BitmapImage(AvatarUri)
    {
        DecodePixelType = DecodePixelType.Logical,
        DecodePixelWidth = 40,
    };
}
