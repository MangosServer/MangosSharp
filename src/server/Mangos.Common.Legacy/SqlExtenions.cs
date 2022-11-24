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

using System;
using System.Data;

namespace Mangos.Common.Legacy;

public static class SqlExtenions
{
    public static T As<T>(this DataRow row, int column)
    {
        return row == null || row[column] == null ? throw new Exception("Null data row.") : (T)Convert.ChangeType(row[column], typeof(T));
    }

    public static T As<T>(this DataRow row, string field)
    {
        return row == null || row[field] == null ? throw new Exception("Null data row.") : (T)Convert.ChangeType(row[field], typeof(T));
    }

    /// <typeparam name="T1">Cast1</typeparam>
    /// <typeparam name="T2">Cast2</typeparam>
    public static T2 As<T1, T2>(this DataRow row, string field)
    {
        if (row == null || row[field] == null)
        {
            throw new Exception("Null data row.");
        }

        T1 t1 = (T1)Convert.ChangeType(row[field], typeof(T1));
        return (T2)Convert.ChangeType(t1, typeof(T2));
    }
}
