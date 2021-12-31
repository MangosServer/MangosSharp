//
// Copyright (C) 2013-2022 getMaNGOS <https://getmangos.eu>
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

using Mangos.World.AI;
using Mangos.World.DataStores;
using Mangos.World.Objects;
using Microsoft.VisualBasic.CompilerServices;
using System.Collections.Generic;

namespace Mangos.World.Scripts.Creatures;

public class CreatureAI_The_Plains_Vision : WS_Creatures_AI.TBaseAI
{
    protected WS_Creatures.CreatureObject aiCreature;
    private int CurrentWaypoint;
    private int NextWaypoint;
    private readonly List<WS_DBCDatabase.CreatureMovePoint> Waypoints = new();

    public CreatureAI_The_Plains_Vision(ref WS_Creatures.CreatureObject Creature)
    {
        aiCreature = Creature;
        InitWaypoints();
    }

    private void InitWaypoints()
    {
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint((float)-2239.839d, (float)-404.8294d, (float)-9.424251d, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint((float)-2224.553d, (float)-419.0978d, (float)-9.319928d, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-2201.178f, -440.8505f, -5.636199f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-2182.829f, -453.5462f, -5.741747f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-2163.393f, -461.4627f, -7.541375f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-2130.101f, -453.9658f, -9.343233f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-2104.559f, -426.2767f, -6.42904f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-2098.924f, -418.6772f, -6.739819f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-2081.393f, -394.2358f, -10.18329f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-2053.604f, -356.7091f, -6.137362f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-2036.3f, -325.1877f, -8.734427f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-2004.85f, -252.2856f, -10.78323f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-1967.54f, -186.5961f, -10.83361f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-1923.677f, -122.1353f, -11.85162f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-1839.147f, -37.15145f, -12.28224f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-1799.021f, -15.25407f, -10.3242f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-1781.322f, 17.70257f, -4.69648f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-1754.998f, 70.02794f, 0.8294563f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-1726.214f, 108.2988f, -6.750209f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-1695.726f, 137.4528f, 0.02649199f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-1672.852f, 159.3268f, -2.089812f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-1643.49f, 187.4037f, 2.815289f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-1609.052f, 220.3357f, 0.2568858f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-1571.285f, 254.8306f, 0.743489f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-1547.945f, 281.0505f, 22.61983f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-1535.038f, 325.911f, 57.57213f, 0, 0, 0, 100));
        Waypoints.Add(new WS_DBCDatabase.CreatureMovePoint(-1526.743f, 332.8374f, 63.22335f, 0, 0, 0, 100));
    }

    public override void DoThink()
    {
        NextWaypoint -= 1000;
        if (NextWaypoint > 0)
        {
            return;
        }
        // The guide has finished
        if (Conversions.ToBoolean(aiCreature.Life.Current) || CurrentWaypoint >= Waypoints.Count)
        {
            aiCreature.Destroy();
            return;
        }

        NextWaypoint = aiCreature.MoveTo(Waypoints[CurrentWaypoint].x, Waypoints[CurrentWaypoint].y, Waypoints[CurrentWaypoint].z) + Waypoints[CurrentWaypoint].waittime;
        CurrentWaypoint += 1;
    }
}
