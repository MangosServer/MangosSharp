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

using System.Threading;
using System.Threading.Tasks;

namespace Mangos.Cluster.Admin.Commands;

/// <summary>
/// Local executor for an <see cref="AdminCommand"/> that targets this
/// cluster (TargetRealmId == this cluster's realm id, or 0).
///
/// The actual implementation lives in Mangos.Cluster (talks to the
/// supervisor); this interface is in the Admin project so the federation
/// transport can dispatch without referencing cluster internals.
/// </summary>
public interface IAdminCommandHandler
{
    Task<AdminCommandReply> ExecuteAsync(AdminCommand command, CancellationToken ct = default);
}
