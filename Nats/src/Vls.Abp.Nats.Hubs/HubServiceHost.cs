using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Vls.Abp.Nats.Hubs
{
    public class HubServiceHost : BackgroundService
    {
        private readonly IEnumerable<HubService> _services;

        public HubServiceHost(IEnumerable<HubService> services)
        {
            _services = services;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var service in _services)
                service.Start();

            stoppingToken.Register(() =>
            {
                foreach (var service in _services)
                    service.Stop();
            });

            return Task.CompletedTask;
        }
    }
}
