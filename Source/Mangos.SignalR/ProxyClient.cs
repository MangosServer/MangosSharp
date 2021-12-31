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

using Microsoft.AspNetCore.SignalR.Client;
using System.Reflection;

namespace Mangos.SignalR;

public class ProxyClient : DispatchProxy
{
    private HubConnection hubConnection;

    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        if (targetMethod.ReturnType.Name == "Void")
        {
            hubConnection.InvokeCoreAsync(targetMethod.Name, args).Wait();
            return null;
        }

        return hubConnection.InvokeCoreAsync(targetMethod.Name, targetMethod.ReturnType, args).Result;
    }

    public static T Create<T>(string url)
    {
        HubConnectionBuilder hubConnectionBuilder = new();
        hubConnectionBuilder.WithUrl(url);
        var hubConnection = hubConnectionBuilder.Build();
        hubConnection.StartAsync().Wait();
        var proxy = Create<T, ProxyClient>();
        (proxy as ProxyClient).hubConnection = hubConnection;
        return proxy;
    }
}
