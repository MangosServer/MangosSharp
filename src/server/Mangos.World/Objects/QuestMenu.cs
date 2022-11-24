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

using System.Collections;

namespace Mangos.World.Objects;

public class QuestMenu
{
    public ArrayList IDs;

    public ArrayList Names;

    public ArrayList Icons;

    public ArrayList Levels;

    public QuestMenu()
    {
        IDs = new ArrayList();
        Names = new ArrayList();
        Icons = new ArrayList();
        Levels = new ArrayList();
    }

    public void AddMenu(string QuestName, short ID, short Level, byte Icon = 0)
    {
        Names.Add(QuestName);
        IDs.Add(ID);
        Icons.Add(Icon);
        Levels.Add(Level);
    }
}
