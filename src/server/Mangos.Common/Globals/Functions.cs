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

namespace Mangos.Common.Globals;

public class Functions
{
    private readonly MangosGlobalConstants mangosGlobalConstants;

    public Functions(MangosGlobalConstants mangosGlobalConstants)
    {
        this.mangosGlobalConstants = mangosGlobalConstants;
    }

    public bool GuidIsCreature(ulong guid)
    {
        return GuidHigh2(guid) == mangosGlobalConstants.GUID_UNIT;
    }

    public bool GuidIsPet(ulong guid)
    {
        return GuidHigh2(guid) == mangosGlobalConstants.GUID_PET;
    }

    public bool GuidIsItem(ulong guid)
    {
        return GuidHigh2(guid) == mangosGlobalConstants.GUID_ITEM;
    }

    public bool GuidIsGameObject(ulong guid)
    {
        return GuidHigh2(guid) == mangosGlobalConstants.GUID_GAMEOBJECT;
    }

    public bool GuidIsDnyamicObject(ulong guid)
    {
        return GuidHigh2(guid) == mangosGlobalConstants.GUID_DYNAMICOBJECT;
    }

    public bool GuidIsTransport(ulong guid)
    {
        return GuidHigh2(guid) == mangosGlobalConstants.GUID_TRANSPORT;
    }

    public bool GuidIsMoTransport(ulong guid)
    {
        return GuidHigh2(guid) == mangosGlobalConstants.GUID_MO_TRANSPORT;
    }

    public bool GuidIsCorpse(ulong guid)
    {
        return GuidHigh2(guid) == mangosGlobalConstants.GUID_CORPSE;
    }

    public bool GuidIsPlayer(ulong guid)
    {
        return GuidHigh2(guid) == mangosGlobalConstants.GUID_PLAYER;
    }

    public ulong GuidHigh2(ulong guid)
    {
        return guid & mangosGlobalConstants.GUID_MASK_HIGH;
    }

    public uint GuidHigh(ulong guid)
    {
        return (uint)((guid & mangosGlobalConstants.GUID_MASK_HIGH) >> 32);
    }

    public uint GuidLow(ulong guid)
    {
        return (uint)(guid & mangosGlobalConstants.GUID_MASK_LOW);
    }

    public int GetShapeshiftModel(ShapeshiftForm form, Races race, int model)
    {
        switch (form)
        {
            case ShapeshiftForm.FORM_CAT:
                {
                    if (race == Races.RACE_NIGHT_ELF)
                    {
                        return 892;
                    }

                    if (race == Races.RACE_TAUREN)
                    {
                        return 8571;
                    }

                    break;
                }

            case ShapeshiftForm.FORM_BEAR:
            case ShapeshiftForm.FORM_DIREBEAR:
                {
                    if (race == Races.RACE_NIGHT_ELF)
                    {
                        return 2281;
                    }

                    if (race == Races.RACE_TAUREN)
                    {
                        return 2289;
                    }

                    break;
                }

            case ShapeshiftForm.FORM_MOONKIN:
                {
                    if (race == Races.RACE_NIGHT_ELF)
                    {
                        return 15374;
                    }

                    if (race == Races.RACE_TAUREN)
                    {
                        return 15375;
                    }

                    break;
                }

            case ShapeshiftForm.FORM_TRAVEL:
                {
                    return 632;
                }

            case ShapeshiftForm.FORM_AQUA:
                {
                    return 2428;
                }

            case ShapeshiftForm.FORM_FLIGHT:
                {
                    if (race == Races.RACE_NIGHT_ELF)
                    {
                        return 20857;
                    }

                    if (race == Races.RACE_TAUREN)
                    {
                        return 20872;
                    }

                    break;
                }

            case ShapeshiftForm.FORM_SWIFT:
                {
                    if (race == Races.RACE_NIGHT_ELF)
                    {
                        return 21243;
                    }

                    if (race == Races.RACE_TAUREN)
                    {
                        return 21244;
                    }

                    break;
                }

            case ShapeshiftForm.FORM_GHOUL:
                {
                    return race == Races.RACE_NIGHT_ELF ? 10045 : model;
                }

            case ShapeshiftForm.FORM_CREATUREBEAR:
                {
                    return 902;
                }

            case ShapeshiftForm.FORM_GHOSTWOLF:
                {
                    return 4613;
                }

            case ShapeshiftForm.FORM_SPIRITOFREDEMPTION:
                {
                    return 12824;
                }

            default:
                {
                    return model;
                }
        }

        return default;
    }

    public ManaTypes GetShapeshiftManaType(ShapeshiftForm form, ManaTypes manaType)
    {
        switch (form)
        {
            case ShapeshiftForm.FORM_CAT:
            case ShapeshiftForm.FORM_STEALTH:
                {
                    return ManaTypes.TYPE_ENERGY;
                }

            case ShapeshiftForm.FORM_AQUA:
            case ShapeshiftForm.FORM_TRAVEL:
            case ShapeshiftForm.FORM_MOONKIN:
            case var @case when @case == ShapeshiftForm.FORM_MOONKIN:
            case var case1 when case1 == ShapeshiftForm.FORM_MOONKIN:
            case ShapeshiftForm.FORM_SPIRITOFREDEMPTION:
            case ShapeshiftForm.FORM_FLIGHT:
            case ShapeshiftForm.FORM_SWIFT:
                {
                    return ManaTypes.TYPE_MANA;
                }

            case ShapeshiftForm.FORM_BEAR:
            case ShapeshiftForm.FORM_DIREBEAR:
                {
                    return ManaTypes.TYPE_RAGE;
                }

            default:
                {
                    return manaType;
                }
        }
    }
}
