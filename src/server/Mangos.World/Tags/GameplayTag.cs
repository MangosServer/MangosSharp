//
// Copyright (C) 2013-2025 getMaNGOS <https://www.getmangos.eu>
//
// This program is free software. You can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation. either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY. Without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using System;

namespace Mangos.World.Tags;

/// <summary>
/// Hierarchical gameplay tag for categorizing game entities, abilities, and effects.
/// Inspired by Unreal Engine's FGameplayTag system from the Gameplay Ability System (GAS).
///
/// Tags use dot-separated hierarchical names (e.g., "Creature.Type.Undead").
/// A tag "Creature.Type.Undead" is considered a child of "Creature.Type" and "Creature".
/// </summary>
public readonly record struct GameplayTag : IEquatable<GameplayTag>
{
    public string Name { get; init; }

    public GameplayTag(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Returns true if this tag is a child of (or equal to) the given parent tag.
    /// E.g., "Creature.Type.Undead".IsChildOf("Creature.Type") == true
    /// </summary>
    public bool IsChildOf(GameplayTag parent)
    {
        if (string.IsNullOrEmpty(parent.Name)) return true;
        return Name.StartsWith(parent.Name, StringComparison.Ordinal) &&
               (Name.Length == parent.Name.Length || Name[parent.Name.Length] == '.');
    }

    public override string ToString() => Name;
}
