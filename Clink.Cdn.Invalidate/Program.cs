using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Clink.Cdn.Invalidate
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var len = args?.Length;

            if (len == 0)
            {
                Console.WriteLine("please provide a network name and path to invalidate");
            }
            var model = new CommandArgsMapper().Map(args);

            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    var clinkHttpClientBuilder = services.AddHttpClient("clink");
                    if (model.UseProxy)
                    {
                        clinkHttpClientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
                            new HttpClientHandler()
                            {
                                //enter the proxy details here
                                Proxy = new WebProxy("proxy url here")
                                {
                                    Credentials = new NetworkCredential("proxy user name", "proxy password", "domain")
                                },
                                UseProxy = true,
                                PreAuthenticate = true,
                                UseDefaultCredentials = false
                            }
                            );
                    }
                    services.AddSingleton<IInvalidationService, InvalidationService>();
                }).UseConsoleLifetime();

            var host = builder.Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var service = serviceScope.ServiceProvider.GetRequiredService<IInvalidationService>();
                await service.InvalidateService(model);
            }

            await host.RunAsync();

        }
    }
}

