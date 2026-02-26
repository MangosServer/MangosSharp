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

namespace Mangos.World.Tags;

/// <summary>
/// Predefined gameplay tags for common game categories.
/// These correspond to existing enum values but provide a composable tagging system.
/// </summary>
public static class WellKnownTags
{
    // Creature types
    public static readonly GameplayTag CreatureType = new("Creature.Type");
    public static readonly GameplayTag CreatureTypeBeast = new("Creature.Type.Beast");
    public static readonly GameplayTag CreatureTypeDragonkin = new("Creature.Type.Dragonkin");
    public static readonly GameplayTag CreatureTypeDemon = new("Creature.Type.Demon");
    public static readonly GameplayTag CreatureTypeElemental = new("Creature.Type.Elemental");
    public static readonly GameplayTag CreatureTypeGiant = new("Creature.Type.Giant");
    public static readonly GameplayTag CreatureTypeUndead = new("Creature.Type.Undead");
    public static readonly GameplayTag CreatureTypeHumanoid = new("Creature.Type.Humanoid");
    public static readonly GameplayTag CreatureTypeCritter = new("Creature.Type.Critter");
    public static readonly GameplayTag CreatureTypeMechanical = new("Creature.Type.Mechanical");

    // Creature roles
    public static readonly GameplayTag CreatureRoleBoss = new("Creature.Role.Boss");
    public static readonly GameplayTag CreatureRoleBossRaid = new("Creature.Role.Boss.Raid");
    public static readonly GameplayTag CreatureRoleBossDungeon = new("Creature.Role.Boss.Dungeon");
    public static readonly GameplayTag CreatureRoleElite = new("Creature.Role.Elite");
    public static readonly GameplayTag CreatureRoleRareElite = new("Creature.Role.RareElite");
    public static readonly GameplayTag CreatureRoleGuard = new("Creature.Role.Guard");
    public static readonly GameplayTag CreatureRoleVendor = new("Creature.Role.Vendor");
    public static readonly GameplayTag CreatureRoleQuestGiver = new("Creature.Role.QuestGiver");
    public static readonly GameplayTag CreatureRoleTrainer = new("Creature.Role.Trainer");

    // Spell schools
    public static readonly GameplayTag SpellSchool = new("Spell.School");
    public static readonly GameplayTag SpellSchoolPhysical = new("Spell.School.Physical");
    public static readonly GameplayTag SpellSchoolHoly = new("Spell.School.Holy");
    public static readonly GameplayTag SpellSchoolFire = new("Spell.School.Fire");
    public static readonly GameplayTag SpellSchoolNature = new("Spell.School.Nature");
    public static readonly GameplayTag SpellSchoolFrost = new("Spell.School.Frost");
    public static readonly GameplayTag SpellSchoolShadow = new("Spell.School.Shadow");
    public static readonly GameplayTag SpellSchoolArcane = new("Spell.School.Arcane");

    // Effect types (crowd control)
    public static readonly GameplayTag EffectCC = new("Effect.CC");
    public static readonly GameplayTag EffectCCStun = new("Effect.CC.Stun");
    public static readonly GameplayTag EffectCCRoot = new("Effect.CC.Root");
    public static readonly GameplayTag EffectCCSilence = new("Effect.CC.Silence");
    public static readonly GameplayTag EffectCCFear = new("Effect.CC.Fear");
    public static readonly GameplayTag EffectCCPolymorph = new("Effect.CC.Polymorph");
    public static readonly GameplayTag EffectCCSleep = new("Effect.CC.Sleep");
    public static readonly GameplayTag EffectCCBanish = new("Effect.CC.Banish");

    // Immunities
    public static readonly GameplayTag Immunity = new("Immunity");
    public static readonly GameplayTag ImmunityStun = new("Immunity.Stun");
    public static readonly GameplayTag ImmunityFear = new("Immunity.Fear");
    public static readonly GameplayTag ImmunityRoot = new("Immunity.Root");
    public static readonly GameplayTag ImmunityPolymorph = new("Immunity.Polymorph");
    public static readonly GameplayTag ImmunityTaunt = new("Immunity.Taunt");
}
