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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mangos.World.AI;

namespace Mangos.World.Scripts.Creatures;

public static class ScriptHarness
{
    private static readonly Dictionary<string, Type> RegisteredScripts = new(StringComparer.OrdinalIgnoreCase);

    public static void Main()
    {
        DiscoverScripts();
    }

    public static void DiscoverScripts()
    {
        RegisteredScripts.Clear();

        var baseType = typeof(WS_Creatures_AI.BossAI);
        var scriptTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t));

        foreach (var type in scriptTypes)
        {
            RegisteredScripts[type.Name] = type;
        }
    }

    public static IReadOnlyDictionary<string, Type> GetRegisteredScripts() => RegisteredScripts;

    public static Type? GetScript(string name)
    {
        return RegisteredScripts.GetValueOrDefault(name);
    }

    public static int ScriptCount => RegisteredScripts.Count;
}
