using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vls.Abp.EventBus.Nats;
using Vls.Abp.Examples.Client;
using Vls.Abp.Examples.Hubs;
using Vls.Abp.Examples.Store.Application;
using Vls.Abp.Stan;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Modularity;

namespace Vls.Abp.Examples.Store.WebHost
{
    [DependsOn(
        typeof(StoreAppModule),
        typeof(AbpAspNetCoreMvcModule),
        typeof(AbpAutofacModule),
        typeof(AbpEventBusStanMqModule), 
        typeof(AbpNatsHubsModule),
        typeof(AbpNatsClientModule))]
    public class StoreWebHostModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.Configure<AbpNatsMqOptions>(opt =>
            {
                opt.ConnectionTimeout = 1000;
                opt.Url = "localhost:4222";
            });

            context.Services.Configure<AbpStanMqOptions>(opt =>
            {
                opt.ClientId = "test";
                opt.ClusterId = "test-cluster";
                opt.Url = "localhost:4222";
                opt.ConnectionTimeout = 1000;
            });

            context.Services.Configure<HubServiceOptions>(opt =>
            {
                opt.ServiceUid = "test";
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
