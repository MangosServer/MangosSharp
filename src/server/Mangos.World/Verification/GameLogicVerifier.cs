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

using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Player;
using Mangos.World.Globals;
using Mangos.World.Network;
using Mangos.World.Objects;
using Mangos.World.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Mangos.World.Verification;

public class GameLogicVerifier
{
    private Timer _verificationTimer;
    private readonly object _lock = new();
    private bool _isRunning;
    private readonly List<VerificationResult> _recentResults = new();
    private const int MaxRecentResults = 100;

    // Valid race/class combinations for WoW 1.12.1 vanilla
    private static readonly HashSet<(Races Race, Classes Class)> ValidRaceClassCombinations = new()
    {
        (Races.RACE_HUMAN, Classes.CLASS_WARRIOR),
        (Races.RACE_HUMAN, Classes.CLASS_PALADIN),
        (Races.RACE_HUMAN, Classes.CLASS_ROGUE),
        (Races.RACE_HUMAN, Classes.CLASS_PRIEST),
        (Races.RACE_HUMAN, Classes.CLASS_MAGE),
        (Races.RACE_HUMAN, Classes.CLASS_WARLOCK),
        (Races.RACE_DWARF, Classes.CLASS_WARRIOR),
        (Races.RACE_DWARF, Classes.CLASS_PALADIN),
        (Races.RACE_DWARF, Classes.CLASS_HUNTER),
        (Races.RACE_DWARF, Classes.CLASS_ROGUE),
        (Races.RACE_DWARF, Classes.CLASS_PRIEST),
        (Races.RACE_NIGHT_ELF, Classes.CLASS_WARRIOR),
        (Races.RACE_NIGHT_ELF, Classes.CLASS_HUNTER),
        (Races.RACE_NIGHT_ELF, Classes.CLASS_ROGUE),
        (Races.RACE_NIGHT_ELF, Classes.CLASS_PRIEST),
        (Races.RACE_NIGHT_ELF, Classes.CLASS_DRUID),
        (Races.RACE_GNOME, Classes.CLASS_WARRIOR),
        (Races.RACE_GNOME, Classes.CLASS_ROGUE),
        (Races.RACE_GNOME, Classes.CLASS_MAGE),
        (Races.RACE_GNOME, Classes.CLASS_WARLOCK),
        (Races.RACE_ORC, Classes.CLASS_WARRIOR),
        (Races.RACE_ORC, Classes.CLASS_HUNTER),
        (Races.RACE_ORC, Classes.CLASS_ROGUE),
        (Races.RACE_ORC, Classes.CLASS_SHAMAN),
        (Races.RACE_ORC, Classes.CLASS_WARLOCK),
        (Races.RACE_UNDEAD, Classes.CLASS_WARRIOR),
        (Races.RACE_UNDEAD, Classes.CLASS_ROGUE),
        (Races.RACE_UNDEAD, Classes.CLASS_PRIEST),
        (Races.RACE_UNDEAD, Classes.CLASS_MAGE),
        (Races.RACE_UNDEAD, Classes.CLASS_WARLOCK),
        (Races.RACE_TAUREN, Classes.CLASS_WARRIOR),
        (Races.RACE_TAUREN, Classes.CLASS_HUNTER),
        (Races.RACE_TAUREN, Classes.CLASS_SHAMAN),
        (Races.RACE_TAUREN, Classes.CLASS_DRUID),
        (Races.RACE_TROLL, Classes.CLASS_WARRIOR),
        (Races.RACE_TROLL, Classes.CLASS_HUNTER),
        (Races.RACE_TROLL, Classes.CLASS_ROGUE),
        (Races.RACE_TROLL, Classes.CLASS_PRIEST),
        (Races.RACE_TROLL, Classes.CLASS_SHAMAN),
        (Races.RACE_TROLL, Classes.CLASS_MAGE),
    };

    // Valid map IDs for WoW 1.12.1 vanilla
    private static readonly HashSet<uint> ValidMapIds = new()
    {
        0,   // Eastern Kingdoms
        1,   // Kalimdor
        13,  // Testing
        25,  // Scott Test
        29,  // Test
        30,  // Alterac Valley
        33,  // Shadowfang Keep
        34,  // Stormwind Stockade
        36,  // Deadmines
        43,  // Wailing Caverns
        44,  // Monastery (Scarlet)
        47,  // Razorfen Kraul
        48,  // Blackfathom Deeps
        70,  // Uldaman
        90,  // Gnomeregan
        109, // Sunken Temple
        129, // Razorfen Downs
        169, // Emerald Dream
        189, // Scarlet Monastery
        209, // Zul'Farrak
        229, // Blackrock Spire
        230, // Blackrock Depths
        249, // Onyxia's Lair
        269, // Opening of the Dark Portal
        289, // Scholomance
        309, // Zul'Gurub
        329, // Stratholme
        349, // Maraudon
        369, // Deeprun Tram
        389, // Ragefire Chasm
        409, // Molten Core
        429, // Dire Maul
        449, // Alliance PvP Barracks
        450, // Horde PvP Barracks
        451, // Development Land
        469, // Blackwing Lair
        489, // Warsong Gulch
        509, // Ruins of Ahn'Qiraj
        529, // Arathi Basin
        531, // Ahn'Qiraj Temple
        533, // Naxxramas
    };

    // World map coordinate bounds (approximate for WoW vanilla maps)
    private const float MapCoordinateMin = -17066.6f;
    private const float MapCoordinateMax = 17066.6f;

    /// <summary>
    /// Starts periodic verification checks.
    /// </summary>
    /// <param name="intervalMs">The interval in milliseconds between verification runs. Default is 60000 (60 seconds).</param>
    public void Start(int intervalMs = 60000)
    {
        lock (_lock)
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;
            _verificationTimer = new Timer(OnVerificationTimerElapsed, null, intervalMs, intervalMs);
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.INFORMATION, "[VERIFIER] Game logic verifier started with interval {0}ms.", intervalMs);
        }
    }

    /// <summary>
    /// Stops periodic verification checks.
    /// </summary>
    public void Stop()
    {
        lock (_lock)
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;
            _verificationTimer?.Dispose();
            _verificationTimer = null;
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.INFORMATION, "[VERIFIER] Game logic verifier stopped.");
        }
    }

    private void OnVerificationTimerElapsed(object state)
    {
        try
        {
            RunAllVerifications();
        }
        catch (Exception ex)
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.FAILED, "[VERIFIER] Unhandled exception during verification cycle: {0}", ex.Message);
        }
    }

    /// <summary>
    /// Runs all verification checks immediately and returns the results.
    /// </summary>
    public List<VerificationResult> RunAllVerifications()
    {
        var results = new List<VerificationResult>();

        results.Add(SafeRunCheck(VerifyCharacterIntegrity));
        results.Add(SafeRunCheck(VerifyCreatureState));
        results.Add(SafeRunCheck(VerifyItemIntegrity));
        results.Add(SafeRunCheck(VerifySpellState));
        results.Add(SafeRunCheck(VerifyWorldObjects));
        results.Add(SafeRunCheck(VerifyNetworkState));
        results.Add(SafeRunCheck(VerifyQuestState));
        results.Add(SafeRunCheck(VerifyCombatState));
        results.Add(SafeRunCheck(VerifyMapConsistency));

        lock (_lock)
        {
            foreach (var result in results)
            {
                _recentResults.Add(result);
            }

            while (_recentResults.Count > MaxRecentResults)
            {
                _recentResults.RemoveAt(0);
            }
        }

        LogResults(results);
        return results;
    }

    /// <summary>
    /// Returns a copy of the recent verification results.
    /// </summary>
    public List<VerificationResult> GetRecentResults()
    {
        lock (_lock)
        {
            return new List<VerificationResult>(_recentResults);
        }
    }

    private VerificationResult SafeRunCheck(Func<VerificationResult> check)
    {
        try
        {
            return check();
        }
        catch (Exception ex)
        {
            return new VerificationResult
            {
                CheckName = check.Method.Name,
                Status = VerificationStatus.Error,
                Message = $"Exception during check: {ex.Message}",
                IssuesFound = 1,
                Details = { ex.ToString() }
            };
        }
    }

    private void LogResults(List<VerificationResult> results)
    {
        var totalIssues = results.Sum(r => r.IssuesFound);
        var failedCount = results.Count(r => r.Status == VerificationStatus.Failed);
        var warningCount = results.Count(r => r.Status == VerificationStatus.Warning);
        var errorCount = results.Count(r => r.Status == VerificationStatus.Error);

        if (totalIssues == 0)
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.DEBUG, "[VERIFIER] All {0} verification checks passed.", results.Count);
            return;
        }

        WorldServiceLocator.WorldServer.Log.WriteLine(LogType.WARNING,
            "[VERIFIER] Verification complete: {0} checks run, {1} issues found ({2} failed, {3} warnings, {4} errors).",
            results.Count, totalIssues, failedCount, warningCount, errorCount);

        foreach (var result in results)
        {
            if (result.Status == VerificationStatus.Passed)
            {
                continue;
            }

            var logType = result.Status switch
            {
                VerificationStatus.Warning => LogType.WARNING,
                VerificationStatus.Failed => LogType.FAILED,
                VerificationStatus.Error => LogType.CRITICAL,
                _ => LogType.DEBUG
            };

            WorldServiceLocator.WorldServer.Log.WriteLine(logType,
                "[VERIFIER] [{0}] {1} - {2} issue(s): {3}",
                result.Status, result.CheckName, result.IssuesFound, result.Message);

            foreach (var detail in result.Details.Take(10))
            {
                WorldServiceLocator.WorldServer.Log.WriteLine(logType, "[VERIFIER]   - {0}", detail);
            }

            if (result.Details.Count > 10)
            {
                WorldServiceLocator.WorldServer.Log.WriteLine(logType,
                    "[VERIFIER]   ... and {0} more issues.", result.Details.Count - 10);
            }
        }
    }

    // ========================================================================
    // 1. Verify Character Data Integrity
    // ========================================================================
    public VerificationResult VerifyCharacterIntegrity()
    {
        var result = new VerificationResult
        {
            CheckName = "CharacterIntegrity",
            Status = VerificationStatus.Passed,
            Message = "All character data is valid."
        };

        Dictionary<ulong, WS_PlayerData.CharacterObject> characters;
        WorldServiceLocator.WorldServer.CHARACTERs_Lock.EnterReadLock();
        try
        {
            characters = new Dictionary<ulong, WS_PlayerData.CharacterObject>(WorldServiceLocator.WorldServer.CHARACTERs);
        }
        finally
        {
            WorldServiceLocator.WorldServer.CHARACTERs_Lock.ExitReadLock();
        }

        foreach (var kvp in characters)
        {
            var guid = kvp.Key;
            var character = kvp.Value;

            if (character == null)
            {
                result.Details.Add($"Character GUID {guid}: null character object in CHARACTERs dictionary.");
                result.IssuesFound++;
                continue;
            }

            // Check valid race/class combination
            if (!ValidRaceClassCombinations.Contains((character.Race, character.Classe)))
            {
                result.Details.Add($"Character '{character.Name}' (GUID {guid}): invalid race/class combination Race={character.Race}, Class={character.Classe}.");
                result.IssuesFound++;
            }

            // Check HP is not negative or exceeding maximum
            if (character.Life.Current < 0)
            {
                result.Details.Add($"Character '{character.Name}' (GUID {guid}): negative HP ({character.Life.Current}).");
                result.IssuesFound++;
            }
            if (character.Life.Maximum > 0 && character.Life.Current > character.Life.Maximum)
            {
                result.Details.Add($"Character '{character.Name}' (GUID {guid}): HP ({character.Life.Current}) exceeds maximum ({character.Life.Maximum}).");
                result.IssuesFound++;
            }

            // Check mana is within valid bounds
            if (character.Mana.Current < 0)
            {
                result.Details.Add($"Character '{character.Name}' (GUID {guid}): negative mana ({character.Mana.Current}).");
                result.IssuesFound++;
            }
            if (character.Mana.Maximum > 0 && character.Mana.Current > character.Mana.Maximum)
            {
                result.Details.Add($"Character '{character.Name}' (GUID {guid}): mana ({character.Mana.Current}) exceeds maximum ({character.Mana.Maximum}).");
                result.IssuesFound++;
            }

            // Check rage is within valid bounds (0-1000, displayed as 0-100)
            if (character.Rage.Current < 0)
            {
                result.Details.Add($"Character '{character.Name}' (GUID {guid}): negative rage ({character.Rage.Current}).");
                result.IssuesFound++;
            }
            if (character.Rage.Current > 1000)
            {
                result.Details.Add($"Character '{character.Name}' (GUID {guid}): rage ({character.Rage.Current}) exceeds maximum (1000).");
                result.IssuesFound++;
            }

            // Check energy is within valid bounds (0-100 normally)
            if (character.Energy.Current < 0)
            {
                result.Details.Add($"Character '{character.Name}' (GUID {guid}): negative energy ({character.Energy.Current}).");
                result.IssuesFound++;
            }
            if (character.Energy.Maximum > 0 && character.Energy.Current > character.Energy.Maximum)
            {
                result.Details.Add($"Character '{character.Name}' (GUID {guid}): energy ({character.Energy.Current}) exceeds maximum ({character.Energy.Maximum}).");
                result.IssuesFound++;
            }

            // Check level is between 1-60
            if (character.Level < 1 || character.Level > 60)
            {
                result.Details.Add($"Character '{character.Name}' (GUID {guid}): invalid level ({character.Level}), expected 1-60.");
                result.IssuesFound++;
            }

            // Check position coordinates are within valid map bounds
            if (character.positionX < MapCoordinateMin || character.positionX > MapCoordinateMax ||
                character.positionY < MapCoordinateMin || character.positionY > MapCoordinateMax)
            {
                result.Details.Add($"Character '{character.Name}' (GUID {guid}): position out of bounds ({character.positionX}, {character.positionY}, {character.positionZ}) on map {character.MapID}.");
                result.IssuesFound++;
            }

            // Check equipped items exist and reference valid item templates
            if (character.Items != null)
            {
                foreach (var itemKvp in character.Items)
                {
                    var slot = itemKvp.Key;
                    var item = itemKvp.Value;

                    if (item == null)
                    {
                        result.Details.Add($"Character '{character.Name}' (GUID {guid}): null item in slot {slot}.");
                        result.IssuesFound++;
                        continue;
                    }

                    if (!WorldServiceLocator.WorldServer.ITEMDatabase.ContainsKey(item.ItemEntry))
                    {
                        result.Details.Add($"Character '{character.Name}' (GUID {guid}): item in slot {slot} references unknown item template {item.ItemEntry}.");
                        result.IssuesFound++;
                    }
                }
            }
        }

        if (result.IssuesFound > 0)
        {
            result.Status = VerificationStatus.Warning;
            result.Message = $"Found {result.IssuesFound} character integrity issue(s) across {characters.Count} online character(s).";
        }
        else
        {
            result.Message = $"All {characters.Count} online character(s) passed integrity checks.";
        }

        return result;
    }

    // ========================================================================
    // 2. Verify Creature State Consistency
    // ========================================================================
    public VerificationResult VerifyCreatureState()
    {
        var result = new VerificationResult
        {
            CheckName = "CreatureState",
            Status = VerificationStatus.Passed,
            Message = "All creature states are valid."
        };

        Dictionary<ulong, WS_Creatures.CreatureObject> creatures;
        WorldServiceLocator.WorldServer.WORLD_CREATUREs_Lock.EnterReadLock();
        try
        {
            creatures = new Dictionary<ulong, WS_Creatures.CreatureObject>(WorldServiceLocator.WorldServer.WORLD_CREATUREs);
        }
        finally
        {
            WorldServiceLocator.WorldServer.WORLD_CREATUREs_Lock.ExitReadLock();
        }

        foreach (var kvp in creatures)
        {
            var guid = kvp.Key;
            var creature = kvp.Value;

            if (creature == null)
            {
                result.Details.Add($"Creature GUID {guid}: null creature object in WORLD_CREATUREs dictionary.");
                result.IssuesFound++;
                continue;
            }

            // Check creatures have valid AI scripts
            if (creature.aiScript == null)
            {
                result.Details.Add($"Creature '{creature.Name}' (GUID {guid}, ID {creature.ID}): no AI script assigned.");
                result.IssuesFound++;
            }

            // Check creature health is not above maximum
            if (creature.Life.Maximum > 0 && creature.Life.Current > creature.Life.Maximum)
            {
                result.Details.Add($"Creature '{creature.Name}' (GUID {guid}): HP ({creature.Life.Current}) exceeds maximum ({creature.Life.Maximum}).");
                result.IssuesFound++;
            }

            // Check creature health is not negative
            if (creature.Life.Current < 0)
            {
                result.Details.Add($"Creature '{creature.Name}' (GUID {guid}): negative HP ({creature.Life.Current}).");
                result.IssuesFound++;
            }

            // Check creatures are on valid maps
            if (!ValidMapIds.Contains(creature.MapID))
            {
                result.Details.Add($"Creature '{creature.Name}' (GUID {guid}): on invalid map ID {creature.MapID}.");
                result.IssuesFound++;
            }

            // Check creature positions are within map bounds
            if (creature.positionX < MapCoordinateMin || creature.positionX > MapCoordinateMax ||
                creature.positionY < MapCoordinateMin || creature.positionY > MapCoordinateMax)
            {
                result.Details.Add($"Creature '{creature.Name}' (GUID {guid}): position out of bounds ({creature.positionX}, {creature.positionY}, {creature.positionZ}).");
                result.IssuesFound++;
            }

            // Detect orphaned creatures (no valid spawn point - spawn coordinates all zero and not a summoned creature)
            if (creature.SpawnX == 0f && creature.SpawnY == 0f && creature.SpawnZ == 0f && creature.SummonedBy == 0)
            {
                result.Details.Add($"Creature '{creature.Name}' (GUID {guid}): potential orphan - spawn coordinates are all zero and not summoned.");
                result.IssuesFound++;
            }

            // Check creature template exists in the database
            if (!WorldServiceLocator.WorldServer.CREATURESDatabase.ContainsKey(creature.ID))
            {
                result.Details.Add($"Creature GUID {guid}: references unknown creature template ID {creature.ID}.");
                result.IssuesFound++;
            }
        }

        if (result.IssuesFound > 0)
        {
            result.Status = VerificationStatus.Warning;
            result.Message = $"Found {result.IssuesFound} creature state issue(s) across {creatures.Count} creature(s).";
        }
        else
        {
            result.Message = $"All {creatures.Count} creature(s) passed state checks.";
        }

        return result;
    }

    // ========================================================================
    // 3. Verify Item Integrity
    // ========================================================================
    public VerificationResult VerifyItemIntegrity()
    {
        var result = new VerificationResult
        {
            CheckName = "ItemIntegrity",
            Status = VerificationStatus.Passed,
            Message = "All items are valid."
        };

        Dictionary<ulong, ItemObject> items;
        lock (WorldServiceLocator.WorldServer.WORLD_ITEMs)
        {
            items = new Dictionary<ulong, ItemObject>(WorldServiceLocator.WorldServer.WORLD_ITEMs);
        }

        foreach (var kvp in items)
        {
            var guid = kvp.Key;
            var item = kvp.Value;

            if (item == null)
            {
                result.Details.Add($"Item GUID {guid}: null item object in WORLD_ITEMs dictionary.");
                result.IssuesFound++;
                continue;
            }

            // Check items reference valid item templates
            if (!WorldServiceLocator.WorldServer.ITEMDatabase.ContainsKey(item.ItemEntry))
            {
                result.Details.Add($"Item GUID {guid}: references unknown item template {item.ItemEntry}.");
                result.IssuesFound++;
                continue;
            }

            var itemInfo = WorldServiceLocator.WorldServer.ITEMDatabase[item.ItemEntry];

            // Check stack sizes don't exceed maximum
            if (itemInfo.Stackable > 0 && item.StackCount > itemInfo.Stackable)
            {
                result.Details.Add($"Item '{itemInfo.Name}' (GUID {guid}): stack count ({item.StackCount}) exceeds maximum stackable ({itemInfo.Stackable}).");
                result.IssuesFound++;
            }

            // Check stack count is not negative or zero (should be at least 1)
            if (item.StackCount <= 0)
            {
                result.Details.Add($"Item '{itemInfo.Name}' (GUID {guid}): invalid stack count ({item.StackCount}).");
                result.IssuesFound++;
            }

            // Check item durability is not negative
            if (item.Durability < 0)
            {
                result.Details.Add($"Item '{itemInfo.Name}' (GUID {guid}): negative durability ({item.Durability}).");
                result.IssuesFound++;
            }

            // Check item durability does not exceed template maximum
            if (itemInfo.Durability > 0 && item.Durability > itemInfo.Durability)
            {
                result.Details.Add($"Item '{itemInfo.Name}' (GUID {guid}): durability ({item.Durability}) exceeds template maximum ({itemInfo.Durability}).");
                result.IssuesFound++;
            }

            // Detect orphaned items (no owner and not in world items)
            if (item.OwnerGUID != 0 && !WorldServiceLocator.WorldServer.CHARACTERs.ContainsKey(item.OwnerGUID))
            {
                result.Details.Add($"Item '{itemInfo.Name}' (GUID {guid}): owner GUID {item.OwnerGUID} is not an online character (may be offline - informational).");
                // This is informational, not necessarily an error since owners can be offline
            }
        }

        if (result.IssuesFound > 0)
        {
            result.Status = VerificationStatus.Warning;
            result.Message = $"Found {result.IssuesFound} item integrity issue(s) across {items.Count} item(s).";
        }
        else
        {
            result.Message = $"All {items.Count} item(s) passed integrity checks.";
        }

        return result;
    }

    // ========================================================================
    // 4. Verify Spell State Consistency
    // ========================================================================
    public VerificationResult VerifySpellState()
    {
        var result = new VerificationResult
        {
            CheckName = "SpellState",
            Status = VerificationStatus.Passed,
            Message = "All spell states are valid."
        };

        var maxAuraEffects = WorldServiceLocator.GlobalConstants.MAX_AURA_EFFECTs;

        Dictionary<ulong, WS_PlayerData.CharacterObject> characters;
        WorldServiceLocator.WorldServer.CHARACTERs_Lock.EnterReadLock();
        try
        {
            characters = new Dictionary<ulong, WS_PlayerData.CharacterObject>(WorldServiceLocator.WorldServer.CHARACTERs);
        }
        finally
        {
            WorldServiceLocator.WorldServer.CHARACTERs_Lock.ExitReadLock();
        }

        foreach (var kvp in characters)
        {
            var guid = kvp.Key;
            var character = kvp.Value;

            if (character == null || character.ActiveSpells == null)
            {
                continue;
            }

            var activeAuraCount = 0;

            for (var i = 0; i < character.ActiveSpells.Length && i < maxAuraEffects; i++)
            {
                var activeSpell = character.ActiveSpells[i];
                if (activeSpell == null)
                {
                    continue;
                }

                activeAuraCount++;

                // Check active auras reference valid spells
                if (!WorldServiceLocator.WSSpells.SPELLs.ContainsKey(activeSpell.SpellID))
                {
                    result.Details.Add($"Character '{character.Name}' (GUID {guid}): aura slot {i} references unknown spell ID {activeSpell.SpellID}.");
                    result.IssuesFound++;
                }

                // Detect expired auras that weren't cleaned up
                // SpellDuration of 0 with a non-infinite original duration could indicate an expired aura
                if (activeSpell.SpellDuration == 0 && activeSpell.SpellID != 0)
                {
                    // Check if the spell has an infinite duration (-1 means infinite)
                    if (WorldServiceLocator.WSSpells.SPELLs.ContainsKey(activeSpell.SpellID))
                    {
                        var spellInfo = WorldServiceLocator.WSSpells.SPELLs[activeSpell.SpellID];
                        if (!spellInfo.IsPassive)
                        {
                            result.Details.Add($"Character '{character.Name}' (GUID {guid}): aura slot {i} spell '{activeSpell.SpellID}' has zero duration remaining (possibly expired and not cleaned up).");
                            result.IssuesFound++;
                        }
                    }
                }
            }

            // Check aura slot counts don't exceed maximum
            if (activeAuraCount > maxAuraEffects)
            {
                result.Details.Add($"Character '{character.Name}' (GUID {guid}): active aura count ({activeAuraCount}) exceeds maximum ({maxAuraEffects}).");
                result.IssuesFound++;
            }
        }

        // Also check creature auras
        Dictionary<ulong, WS_Creatures.CreatureObject> creatures;
        WorldServiceLocator.WorldServer.WORLD_CREATUREs_Lock.EnterReadLock();
        try
        {
            creatures = new Dictionary<ulong, WS_Creatures.CreatureObject>(WorldServiceLocator.WorldServer.WORLD_CREATUREs);
        }
        finally
        {
            WorldServiceLocator.WorldServer.WORLD_CREATUREs_Lock.ExitReadLock();
        }

        foreach (var kvp in creatures)
        {
            var guid = kvp.Key;
            var creature = kvp.Value;

            if (creature == null || creature.ActiveSpells == null)
            {
                continue;
            }

            for (var i = 0; i < creature.ActiveSpells.Length && i < maxAuraEffects; i++)
            {
                var activeSpell = creature.ActiveSpells[i];
                if (activeSpell == null)
                {
                    continue;
                }

                // Check active auras reference valid spells
                if (!WorldServiceLocator.WSSpells.SPELLs.ContainsKey(activeSpell.SpellID))
                {
                    result.Details.Add($"Creature '{creature.Name}' (GUID {guid}): aura slot {i} references unknown spell ID {activeSpell.SpellID}.");
                    result.IssuesFound++;
                }
            }
        }

        if (result.IssuesFound > 0)
        {
            result.Status = VerificationStatus.Warning;
            result.Message = $"Found {result.IssuesFound} spell state issue(s).";
        }
        else
        {
            result.Message = $"Spell state checks passed for {characters.Count} character(s) and {creatures.Count} creature(s).";
        }

        return result;
    }

    // ========================================================================
    // 5. Verify World Object Consistency
    // ========================================================================
    public VerificationResult VerifyWorldObjects()
    {
        var result = new VerificationResult
        {
            CheckName = "WorldObjects",
            Status = VerificationStatus.Passed,
            Message = "All world objects are consistent."
        };

        // Check WORLD_CREATUREs dictionary matches WORLD_CREATUREsKeys
        Dictionary<ulong, WS_Creatures.CreatureObject> creatures;
        int keysCount;
        WorldServiceLocator.WorldServer.WORLD_CREATUREs_Lock.EnterReadLock();
        try
        {
            creatures = new Dictionary<ulong, WS_Creatures.CreatureObject>(WorldServiceLocator.WorldServer.WORLD_CREATUREs);
            keysCount = WorldServiceLocator.WorldServer.WORLD_CREATUREsKeys.Count;
        }
        finally
        {
            WorldServiceLocator.WorldServer.WORLD_CREATUREs_Lock.ExitReadLock();
        }

        if (creatures.Count != keysCount)
        {
            result.Details.Add($"WORLD_CREATUREs count ({creatures.Count}) does not match WORLD_CREATUREsKeys count ({keysCount}).");
            result.IssuesFound++;
        }

        // Check for GUID collisions across object types
        var allGuids = new HashSet<ulong>();
        var guidCollisions = 0;

        // Collect character GUIDs
        Dictionary<ulong, WS_PlayerData.CharacterObject> characters;
        WorldServiceLocator.WorldServer.CHARACTERs_Lock.EnterReadLock();
        try
        {
            characters = new Dictionary<ulong, WS_PlayerData.CharacterObject>(WorldServiceLocator.WorldServer.CHARACTERs);
        }
        finally
        {
            WorldServiceLocator.WorldServer.CHARACTERs_Lock.ExitReadLock();
        }

        foreach (var guid in characters.Keys)
        {
            if (!allGuids.Add(guid))
            {
                result.Details.Add($"GUID collision detected: character GUID {guid} conflicts with another object.");
                guidCollisions++;
            }
        }

        // Collect creature GUIDs
        foreach (var guid in creatures.Keys)
        {
            if (!allGuids.Add(guid))
            {
                result.Details.Add($"GUID collision detected: creature GUID {guid} conflicts with another object.");
                guidCollisions++;
            }
        }

        // Collect game object GUIDs
        Dictionary<ulong, WS_GameObjects.GameObject> gameObjects;
        lock (WorldServiceLocator.WorldServer.WORLD_GAMEOBJECTs)
        {
            gameObjects = new Dictionary<ulong, WS_GameObjects.GameObject>(WorldServiceLocator.WorldServer.WORLD_GAMEOBJECTs);
        }

        foreach (var guid in gameObjects.Keys)
        {
            if (!allGuids.Add(guid))
            {
                result.Details.Add($"GUID collision detected: game object GUID {guid} conflicts with another object.");
                guidCollisions++;
            }
        }

        if (guidCollisions > 0)
        {
            result.IssuesFound += guidCollisions;
        }

        // Check all GameObjects have valid templates
        foreach (var kvp in gameObjects)
        {
            var guid = kvp.Key;
            var gameObject = kvp.Value;

            if (gameObject == null)
            {
                result.Details.Add($"GameObject GUID {guid}: null object in WORLD_GAMEOBJECTs dictionary.");
                result.IssuesFound++;
                continue;
            }

            if (!WorldServiceLocator.WorldServer.GAMEOBJECTSDatabase.ContainsKey(gameObject.ID))
            {
                result.Details.Add($"GameObject GUID {guid}: references unknown template ID {gameObject.ID}.");
                result.IssuesFound++;
            }
        }

        // Check transport objects are functioning
        Dictionary<ulong, WS_Transports.TransportObject> transports;
        WorldServiceLocator.WorldServer.WORLD_TRANSPORTs_Lock.EnterReadLock();
        try
        {
            transports = new Dictionary<ulong, WS_Transports.TransportObject>(WorldServiceLocator.WorldServer.WORLD_TRANSPORTs);
        }
        finally
        {
            WorldServiceLocator.WorldServer.WORLD_TRANSPORTs_Lock.ExitReadLock();
        }

        foreach (var kvp in transports)
        {
            var guid = kvp.Key;
            var transport = kvp.Value;

            if (transport == null)
            {
                result.Details.Add($"Transport GUID {guid}: null transport object.");
                result.IssuesFound++;
            }
        }

        if (result.IssuesFound > 0)
        {
            result.Status = VerificationStatus.Warning;
            result.Message = $"Found {result.IssuesFound} world object consistency issue(s).";
        }
        else
        {
            result.Message = $"World objects consistent: {characters.Count} characters, {creatures.Count} creatures, {gameObjects.Count} game objects, {transports.Count} transports.";
        }

        return result;
    }

    // ========================================================================
    // 6. Verify Network State
    // ========================================================================
    public VerificationResult VerifyNetworkState()
    {
        var result = new VerificationResult
        {
            CheckName = "NetworkState",
            Status = VerificationStatus.Passed,
            Message = "All network states are valid."
        };

        Dictionary<uint, WS_Network.ClientClass> clients;
        lock (WorldServiceLocator.WorldServer.CLIENTs)
        {
            clients = new Dictionary<uint, WS_Network.ClientClass>(WorldServiceLocator.WorldServer.CLIENTs);
        }

        Dictionary<ulong, WS_PlayerData.CharacterObject> characters;
        WorldServiceLocator.WorldServer.CHARACTERs_Lock.EnterReadLock();
        try
        {
            characters = new Dictionary<ulong, WS_PlayerData.CharacterObject>(WorldServiceLocator.WorldServer.CHARACTERs);
        }
        finally
        {
            WorldServiceLocator.WorldServer.CHARACTERs_Lock.ExitReadLock();
        }

        foreach (var kvp in clients)
        {
            var index = kvp.Key;
            var client = kvp.Value;

            if (client == null)
            {
                result.Details.Add($"Client index {index}: null client object in CLIENTs dictionary.");
                result.IssuesFound++;
                continue;
            }

            // Check character references are valid for each client
            if (client.Character != null)
            {
                if (!characters.ContainsKey(client.Character.GUID))
                {
                    result.Details.Add($"Client index {index}: references character GUID {client.Character.GUID} not found in CHARACTERs dictionary (stale reference).");
                    result.IssuesFound++;
                }
            }
        }

        // Check that all characters with FullyLoggedIn have valid client references
        foreach (var kvp in characters)
        {
            var guid = kvp.Key;
            var character = kvp.Value;

            if (character == null)
            {
                continue;
            }

            if (character.FullyLoggedIn && character.client == null)
            {
                result.Details.Add($"Character '{character.Name}' (GUID {guid}): fully logged in but has no client reference.");
                result.IssuesFound++;
            }
        }

        if (result.IssuesFound > 0)
        {
            result.Status = VerificationStatus.Warning;
            result.Message = $"Found {result.IssuesFound} network state issue(s) across {clients.Count} client(s).";
        }
        else
        {
            result.Message = $"Network state valid: {clients.Count} connected client(s), {characters.Count} online character(s).";
        }

        return result;
    }

    // ========================================================================
    // 7. Verify Quest State
    // ========================================================================
    public VerificationResult VerifyQuestState()
    {
        var result = new VerificationResult
        {
            CheckName = "QuestState",
            Status = VerificationStatus.Passed,
            Message = "All quest states are valid."
        };

        Dictionary<ulong, WS_PlayerData.CharacterObject> characters;
        WorldServiceLocator.WorldServer.CHARACTERs_Lock.EnterReadLock();
        try
        {
            characters = new Dictionary<ulong, WS_PlayerData.CharacterObject>(WorldServiceLocator.WorldServer.CHARACTERs);
        }
        finally
        {
            WorldServiceLocator.WorldServer.CHARACTERs_Lock.ExitReadLock();
        }

        foreach (var kvp in characters)
        {
            var guid = kvp.Key;
            var character = kvp.Value;

            if (character == null || character.TalkQuests == null)
            {
                continue;
            }

            // Check player quest logs reference valid quests
            for (var i = 0; i < character.TalkQuests.Length; i++)
            {
                var quest = character.TalkQuests[i];
                if (quest == null)
                {
                    continue;
                }

                // Check quest ID is valid (positive)
                if (quest.ID <= 0)
                {
                    result.Details.Add($"Character '{character.Name}' (GUID {guid}): quest slot {i} has invalid quest ID {quest.ID}.");
                    result.IssuesFound++;
                }

                // Check quest objective counts are within bounds
                if (quest.Progress != null)
                {
                    for (var j = 0; j < quest.Progress.Length; j++)
                    {
                        if (quest.ObjectivesCount != null && j < quest.ObjectivesCount.Length)
                        {
                            // Progress should not exceed the objective count significantly
                            if (quest.Progress[j] > quest.ObjectivesCount[j] && quest.ObjectivesCount[j] > 0)
                            {
                                result.Details.Add($"Character '{character.Name}' (GUID {guid}): quest '{quest.Title}' (ID {quest.ID}) objective {j} progress ({quest.Progress[j]}) exceeds count ({quest.ObjectivesCount[j]}).");
                                result.IssuesFound++;
                            }
                        }
                    }
                }

                if (quest.ProgressItem != null)
                {
                    for (var j = 0; j < quest.ProgressItem.Length; j++)
                    {
                        if (quest.ObjectivesItemCount != null && j < quest.ObjectivesItemCount.Length)
                        {
                            if (quest.ProgressItem[j] > quest.ObjectivesItemCount[j] && quest.ObjectivesItemCount[j] > 0)
                            {
                                result.Details.Add($"Character '{character.Name}' (GUID {guid}): quest '{quest.Title}' (ID {quest.ID}) item objective {j} progress ({quest.ProgressItem[j]}) exceeds count ({quest.ObjectivesItemCount[j]}).");
                                result.IssuesFound++;
                            }
                        }
                    }
                }

                // Verify quest slot value matches the index
                if (quest.Slot != i)
                {
                    result.Details.Add($"Character '{character.Name}' (GUID {guid}): quest '{quest.Title}' (ID {quest.ID}) slot value ({quest.Slot}) does not match array index ({i}).");
                    result.IssuesFound++;
                }
            }
        }

        if (result.IssuesFound > 0)
        {
            result.Status = VerificationStatus.Warning;
            result.Message = $"Found {result.IssuesFound} quest state issue(s).";
        }
        else
        {
            result.Message = $"Quest state checks passed for {characters.Count} character(s).";
        }

        return result;
    }

    // ========================================================================
    // 8. Verify Combat State
    // ========================================================================
    public VerificationResult VerifyCombatState()
    {
        var result = new VerificationResult
        {
            CheckName = "CombatState",
            Status = VerificationStatus.Passed,
            Message = "All combat states are valid."
        };

        Dictionary<ulong, WS_PlayerData.CharacterObject> characters;
        WorldServiceLocator.WorldServer.CHARACTERs_Lock.EnterReadLock();
        try
        {
            characters = new Dictionary<ulong, WS_PlayerData.CharacterObject>(WorldServiceLocator.WorldServer.CHARACTERs);
        }
        finally
        {
            WorldServiceLocator.WorldServer.CHARACTERs_Lock.ExitReadLock();
        }

        Dictionary<ulong, WS_Creatures.CreatureObject> creatures;
        WorldServiceLocator.WorldServer.WORLD_CREATUREs_Lock.EnterReadLock();
        try
        {
            creatures = new Dictionary<ulong, WS_Creatures.CreatureObject>(WorldServiceLocator.WorldServer.WORLD_CREATUREs);
        }
        finally
        {
            WorldServiceLocator.WorldServer.WORLD_CREATUREs_Lock.ExitReadLock();
        }

        foreach (var kvp in characters)
        {
            var guid = kvp.Key;
            var character = kvp.Value;

            if (character == null || character.inCombatWith == null)
            {
                continue;
            }

            // Check units in combat have valid targets
            if (character.inCombatWith.Count > 0)
            {
                var invalidTargets = 0;
                foreach (var combatGuid in character.inCombatWith.ToList())
                {
                    // The combat target should exist as either a character or creature
                    var targetExists = characters.ContainsKey(combatGuid) || creatures.ContainsKey(combatGuid);
                    if (!targetExists)
                    {
                        invalidTargets++;
                    }
                }

                if (invalidTargets > 0)
                {
                    result.Details.Add($"Character '{character.Name}' (GUID {guid}): in combat with {invalidTargets} invalid target(s) (targets no longer exist).");
                    result.IssuesFound++;
                }
            }

            // Check attack timer state is consistent
            if (character.attackState != null)
            {
                // If attacking, check that the Victim reference is valid
                if (character.attackState.Victim != null && !character.attackState.Victim.Exist)
                {
                    result.Details.Add($"Character '{character.Name}' (GUID {guid}): attack timer has a victim that no longer exists.");
                    result.IssuesFound++;
                }
            }

            // Check the target GUID is valid if the character is targeting something
            if (character.TargetGUID != 0)
            {
                var targetValid = characters.ContainsKey(character.TargetGUID) ||
                                  creatures.ContainsKey(character.TargetGUID) ||
                                  WorldServiceLocator.WorldServer.WORLD_GAMEOBJECTs.ContainsKey(character.TargetGUID);
                if (!targetValid)
                {
                    // Targeting an entity that no longer exists is a minor issue
                    result.Details.Add($"Character '{character.Name}' (GUID {guid}): targeting non-existent GUID {character.TargetGUID}.");
                    result.IssuesFound++;
                }
            }
        }

        // Check creatures stuck in combat
        foreach (var kvp in creatures)
        {
            var guid = kvp.Key;
            var creature = kvp.Value;

            if (creature == null || creature.aiScript == null)
            {
                continue;
            }

            // Check if creature is in combat state but might be stuck
            if (creature.aiScript.State == AIState.AI_ATTACKING && creature.aiScript.aiTarget == null)
            {
                result.Details.Add($"Creature '{creature.Name}' (GUID {guid}): in AI_ATTACKING state but has no valid target (stuck in combat).");
                result.IssuesFound++;
            }
        }

        if (result.IssuesFound > 0)
        {
            result.Status = VerificationStatus.Warning;
            result.Message = $"Found {result.IssuesFound} combat state issue(s).";
        }
        else
        {
            result.Message = $"Combat state checks passed for {characters.Count} character(s) and {creatures.Count} creature(s).";
        }

        return result;
    }

    // ========================================================================
    // 9. Verify Map/Movement Consistency
    // ========================================================================
    public VerificationResult VerifyMapConsistency()
    {
        var result = new VerificationResult
        {
            CheckName = "MapConsistency",
            Status = VerificationStatus.Passed,
            Message = "All map states are consistent."
        };

        Dictionary<ulong, WS_PlayerData.CharacterObject> characters;
        WorldServiceLocator.WorldServer.CHARACTERs_Lock.EnterReadLock();
        try
        {
            characters = new Dictionary<ulong, WS_PlayerData.CharacterObject>(WorldServiceLocator.WorldServer.CHARACTERs);
        }
        finally
        {
            WorldServiceLocator.WorldServer.CHARACTERs_Lock.ExitReadLock();
        }

        foreach (var kvp in characters)
        {
            var guid = kvp.Key;
            var character = kvp.Value;

            if (character == null)
            {
                continue;
            }

            // Verify map IDs correspond to valid maps
            if (!ValidMapIds.Contains(character.MapID))
            {
                result.Details.Add($"Character '{character.Name}' (GUID {guid}): on invalid map ID {character.MapID}.");
                result.IssuesFound++;
            }

            // Check characters are in loaded map cells
            // CellX and CellY should be within valid range (0-63 for the standard 64x64 grid)
            if (character.CellX > 63 || character.CellY > 63)
            {
                result.Details.Add($"Character '{character.Name}' (GUID {guid}): cell coordinates out of bounds (CellX={character.CellX}, CellY={character.CellY}).");
                result.IssuesFound++;
            }

            // Check characters on transports have valid transport references
            if (character.OnTransport != null)
            {
                var transportGuid = character.OnTransport.GUID;
                Dictionary<ulong, WS_Transports.TransportObject> transports;
                WorldServiceLocator.WorldServer.WORLD_TRANSPORTs_Lock.EnterReadLock();
                try
                {
                    transports = new Dictionary<ulong, WS_Transports.TransportObject>(WorldServiceLocator.WorldServer.WORLD_TRANSPORTs);
                }
                finally
                {
                    WorldServiceLocator.WorldServer.WORLD_TRANSPORTs_Lock.ExitReadLock();
                }

                if (!transports.ContainsKey(transportGuid) && !WorldServiceLocator.WorldServer.WORLD_GAMEOBJECTs.ContainsKey(transportGuid))
                {
                    result.Details.Add($"Character '{character.Name}' (GUID {guid}): on transport GUID {transportGuid} that does not exist in WORLD_TRANSPORTs or WORLD_GAMEOBJECTs.");
                    result.IssuesFound++;
                }
            }

            // Check bind point map is valid
            if (character.bindpoint_map_id >= 0 && !ValidMapIds.Contains((uint)character.bindpoint_map_id))
            {
                result.Details.Add($"Character '{character.Name}' (GUID {guid}): bind point on invalid map ID {character.bindpoint_map_id}.");
                result.IssuesFound++;
            }

            // Check bind point coordinates are within bounds
            if (character.bindpoint_positionX < MapCoordinateMin || character.bindpoint_positionX > MapCoordinateMax ||
                character.bindpoint_positionY < MapCoordinateMin || character.bindpoint_positionY > MapCoordinateMax)
            {
                result.Details.Add($"Character '{character.Name}' (GUID {guid}): bind point position out of bounds ({character.bindpoint_positionX}, {character.bindpoint_positionY}, {character.bindpoint_positionZ}).");
                result.IssuesFound++;
            }
        }

        if (result.IssuesFound > 0)
        {
            result.Status = VerificationStatus.Warning;
            result.Message = $"Found {result.IssuesFound} map consistency issue(s).";
        }
        else
        {
            result.Message = $"Map consistency checks passed for {characters.Count} character(s).";
        }

        return result;
    }
}
