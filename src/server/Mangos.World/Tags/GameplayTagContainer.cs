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

using System.Collections.Generic;
using System.Linq;

namespace Mangos.World.Tags;

/// <summary>
/// A container of gameplay tags attached to a game entity.
/// Inspired by Unreal Engine's FGameplayTagContainer.
///
/// Supports hierarchical matching: HasTag("Creature.Type") returns true
/// if the container holds "Creature.Type.Undead".
/// </summary>
public sealed class GameplayTagContainer
{
    private readonly HashSet<GameplayTag> _tags = new();

    public int Count => _tags.Count;

    public void AddTag(GameplayTag tag) => _tags.Add(tag);

    public void RemoveTag(GameplayTag tag) => _tags.Remove(tag);

    /// <summary>
    /// Returns true if the container has the exact tag or any child tag.
    /// </summary>
    public bool HasTag(GameplayTag tag)
    {
        return _tags.Any(t => t == tag || t.IsChildOf(tag));
    }

    /// <summary>
    /// Returns true if the container has any of the specified tags (or their children).
    /// </summary>
    public bool HasAnyTag(params GameplayTag[] tags)
    {
        foreach (var tag in tags)
        {
            if (HasTag(tag)) return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if the container has all of the specified tags (or their children).
    /// </summary>
    public bool HasAllTags(params GameplayTag[] tags)
    {
        foreach (var tag in tags)
        {
            if (!HasTag(tag)) return false;
        }
        return true;
    }

    /// <summary>
    /// Returns all tags in the container.
    /// </summary>
    public IReadOnlyCollection<GameplayTag> GetAllTags() => _tags;

    public void Clear() => _tags.Clear();
}
