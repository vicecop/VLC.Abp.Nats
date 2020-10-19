using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vls.Abp.EventBus.Nats;
using Vls.Abp.Nats.Hubs;
using Vls.Abp.Nats.TestApplication;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Modularity;

namespace Vls.Abp.Nats.TestApp
{
    [DependsOn(
        typeof(TestApplicationModule),
        typeof(AbpAspNetCoreMvcModule),
        typeof(AbpAutofacModule),
        typeof(AbpEventBusNatsMqModule), 
        typeof(AbpNatsHubsModule))]
    public class TestAppModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.Configure<AbpNatsMqOptions>(opt =>
            {
                opt.ConnectionTimeout = 1000;
                opt.Url = "localhost:4222";
            });
        }

        public override void OnApplicationInitialization(
            ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            var env = context.GetEnvironment();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseConfiguredEndpoints();

            var eventBus = context.ServiceProvider.GetRequiredService<IDistributedEventBus>();
            eventBus.PublishAsync(new TestEventEto() { Value = "Test" });
        }
    }
}
