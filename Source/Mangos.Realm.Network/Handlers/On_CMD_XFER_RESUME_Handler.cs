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

using Mangos.Realm.Network.Readers;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mangos.Realm.Network.Handlers;

public class On_CMD_XFER_RESUME_Handler : IPacketHandler
{
    private readonly CMD_XFER_RESUME_Reader CMD_XFER_RESUME_Reader;

    public On_CMD_XFER_RESUME_Handler(CMD_XFER_RESUME_Reader CMD_XFER_RESUME_Reader)
    {
        this.CMD_XFER_RESUME_Reader = CMD_XFER_RESUME_Reader;
    }

    public async Task HandleAsync(ChannelReader<byte> reader, ChannelWriter<byte> writer, Client clientModel)
    {
        await CMD_XFER_RESUME_Reader.ReadAsync(reader);
    }
}
