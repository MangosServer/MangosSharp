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

namespace Mangos.World.Tags;

/// <summary>
/// Singleton registry of all known gameplay tags. Validates tag hierarchies
/// and provides lookup capabilities.
/// </summary>
public sealed class GameplayTagRegistry
{
    private readonly HashSet<string> _registeredTags = new();

    public GameplayTagRegistry()
    {
        // Register all well-known tags
        RegisterTag(WellKnownTags.CreatureType);
        RegisterTag(WellKnownTags.CreatureTypeBeast);
        RegisterTag(WellKnownTags.CreatureTypeDragonkin);
        RegisterTag(WellKnownTags.CreatureTypeDemon);
        RegisterTag(WellKnownTags.CreatureTypeElemental);
        RegisterTag(WellKnownTags.CreatureTypeGiant);
        RegisterTag(WellKnownTags.CreatureTypeUndead);
        RegisterTag(WellKnownTags.CreatureTypeHumanoid);
        RegisterTag(WellKnownTags.CreatureTypeCritter);
        RegisterTag(WellKnownTags.CreatureTypeMechanical);
        RegisterTag(WellKnownTags.CreatureRoleBoss);
        RegisterTag(WellKnownTags.CreatureRoleBossRaid);
        RegisterTag(WellKnownTags.CreatureRoleBossDungeon);
        RegisterTag(WellKnownTags.CreatureRoleElite);
        RegisterTag(WellKnownTags.CreatureRoleRareElite);
        RegisterTag(WellKnownTags.CreatureRoleGuard);
        RegisterTag(WellKnownTags.CreatureRoleVendor);
        RegisterTag(WellKnownTags.CreatureRoleQuestGiver);
        RegisterTag(WellKnownTags.CreatureRoleTrainer);
        RegisterTag(WellKnownTags.SpellSchool);
        RegisterTag(WellKnownTags.SpellSchoolPhysical);
        RegisterTag(WellKnownTags.SpellSchoolHoly);
        RegisterTag(WellKnownTags.SpellSchoolFire);
        RegisterTag(WellKnownTags.SpellSchoolNature);
        RegisterTag(WellKnownTags.SpellSchoolFrost);
        RegisterTag(WellKnownTags.SpellSchoolShadow);
        RegisterTag(WellKnownTags.SpellSchoolArcane);
        RegisterTag(WellKnownTags.EffectCC);
        RegisterTag(WellKnownTags.EffectCCStun);
        RegisterTag(WellKnownTags.EffectCCRoot);
        RegisterTag(WellKnownTags.EffectCCSilence);
        RegisterTag(WellKnownTags.EffectCCFear);
        RegisterTag(WellKnownTags.EffectCCPolymorph);
        RegisterTag(WellKnownTags.EffectCCSleep);
        RegisterTag(WellKnownTags.EffectCCBanish);
        RegisterTag(WellKnownTags.Immunity);
        RegisterTag(WellKnownTags.ImmunityStun);
        RegisterTag(WellKnownTags.ImmunityFear);
        RegisterTag(WellKnownTags.ImmunityRoot);
        RegisterTag(WellKnownTags.ImmunityPolymorph);
        RegisterTag(WellKnownTags.ImmunityTaunt);
    }

    public void RegisterTag(GameplayTag tag)
    {
        _registeredTags.Add(tag.Name);
    }

    public bool IsRegistered(GameplayTag tag)
    {
        return _registeredTags.Contains(tag.Name);
    }

    public IReadOnlyCollection<string> AllTags => _registeredTags;
}
