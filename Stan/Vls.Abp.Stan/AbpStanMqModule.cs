using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace Vls.Abp.Stan
{
    public class AbpStanMqModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            Configure<AbpStanMqModule>(configuration.GetSection("Stan"));
        }

        public override void OnApplicationShutdown(ApplicationShutdownContext context)
        {
            context.ServiceProvider
                .GetRequiredService<IStanConnectionPool>()
                .Dispose();
        }
    }
}
