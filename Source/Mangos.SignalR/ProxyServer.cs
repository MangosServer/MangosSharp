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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;

namespace Mangos.SignalR;

public class ProxyServer<T> : IDisposable where T : Hub
{
    private readonly IHost webhost;

    public ProxyServer(IPAddress address, int port, T hub)
    {
        var hostbuilder = Host.CreateDefaultBuilder();
        hostbuilder.ConfigureWebHost(x => ConfigureWebHost(x, address, port, hub));
        webhost = hostbuilder.Build();
        webhost.Start();
    }

    private void ConfigureWebHost(IWebHostBuilder webHostBuilder, IPAddress address, int port, T hub)
    {
        webHostBuilder.UseKestrel(x => x.Listen(address, port));
        webHostBuilder.ConfigureLogging(x => x.ClearProviders());
        webHostBuilder.ConfigureServices(x => ConfigureServices(x, hub));
        webHostBuilder.Configure(ConfigureApplication);
    }

    private void ConfigureServices(IServiceCollection serviceCollection, T hub)
    {
        serviceCollection.AddSignalR(ConfigureSignalR);
        serviceCollection.AddSingleton(hub);
    }

    private void ConfigureApplication(IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.UseRouting();
        applicationBuilder.UseEndpoints(x => x.MapHub<T>(string.Empty));
    }

    private void ConfigureSignalR(HubOptions hubOptions)
    {
        hubOptions.EnableDetailedErrors = true;
    }

    public void Dispose()
    {
        webhost.Dispose();
    }
}
