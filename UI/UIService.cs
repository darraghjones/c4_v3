using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace UI
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class UIService : StatelessService
    {
        public UIService(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        return new WebHostBuilder() 
                        //return WebHost.CreateDefaultBuilder()
                                    .UseKestrel(opt =>
                                    {
                                        int port = serviceContext.CodePackageActivationContext.GetEndpoint("ServiceEndpoint").Port;
                                        opt.Listen(IPAddress.IPv6Any, port, listenOptions =>
                                        {
                                            listenOptions.UseHttps(GetCertificateFromStore());
                                        });
                                    })
                                    .ConfigureAppConfiguration(config =>
                                    {
                                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                                    })
                                    .ConfigureServices(
                                        services =>
                                        {
                                            services.AddSingleton<StatelessServiceContext>(serviceContext);
                                            services.AddScoped<IGameManager>(_ =>
                                            {
                                                var gameManager = ServiceProxy.Create<IGameManager>(new Uri("fabric:/c4_v3/GameManager"), new ServicePartitionKey(1));
                                                return gameManager;
                                            });
                                            services.AddSingleton<Func<Guid, IGameActor>>(_ =>
                                            {
                                                return guid =>
                                                {
                                                    var proxy = ActorProxy.Create<IGameActor>(new ActorId(guid));
                                                    return proxy;
                                                };
                                            });
                                        })
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseStartup<Startup>()
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url)
                                    .Build();
                    }))
            };
        }

        private static X509Certificate2 GetCertificateFromStore()
        {
            return new X509Certificate2("cert.pfx");
        }
    }
}
