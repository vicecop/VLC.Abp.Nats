using Microsoft.Extensions.Options;
using NATS.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;

namespace Vls.Abp.Nats.Client
{
    public sealed class NatsProxyInterceptor<TService> : AbpInterceptor, ITransientDependency
    {
        private readonly NatsProxyOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly NatsMqConnectionManager _connectionManager;

        private readonly INatsSerializer _serializer;

        private readonly static MethodInfo _genericInterceptAsyncMethod;

        static NatsProxyInterceptor()
        {
            _genericInterceptAsyncMethod = typeof(NatsProxyInterceptor<TService>)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(m => m.Name == nameof(MakeRequestAndGetResultAsync) && m.IsGenericMethodDefinition);
        }

        public NatsProxyInterceptor(IOptions<NatsProxyOptions> options, IServiceProvider serviceProvider, NatsMqConnectionManager connectionManager, INatsSerializer serializer)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public override async Task InterceptAsync(IAbpMethodInvocation invocation)
        {
            if (invocation.Method.ReturnType.GenericTypeArguments.IsNullOrEmpty())
            {
                await MakeRequestAsync(invocation);
            }
            else
            {
                var result = (Task)_genericInterceptAsyncMethod
                    .MakeGenericMethod(invocation.Method.ReturnType.GenericTypeArguments[0])
                    .Invoke(this, new object[] { invocation });

                invocation.ReturnValue = await GetResultAsync(
                    result,
                    invocation.Method.ReturnType.GetGenericArguments()[0]
                );
            }
        }

        private async Task<T> MakeRequestAndGetResultAsync<T>(IAbpMethodInvocation invocation)
        {
            var response = await MakeRequestAsync(invocation);
            return _serializer.Deserialize<T>(response.Data);
        }

        private async Task<Msg> MakeRequestAsync(IAbpMethodInvocation invocation)
        {
            var connection = _connectionManager.Connection;

            if (invocation.Method.Name == "Dispose")
            {
                connection.Close();
                return null;
            }

            var argBytes = _serializer.Serialize(invocation.Arguments);
            var subject = $"{typeof(TService).Name}.{invocation.Method.Name}";

            var response = await connection.RequestAsync(subject, argBytes, 1);

            return response;
        }

        private async Task<object> GetResultAsync(Task task, Type resultType)
        {
            await task;
            return typeof(Task<>)
                .MakeGenericType(resultType)
                .GetProperty(nameof(Task<object>.Result), BindingFlags.Instance | BindingFlags.Public)
                .GetValue(task);
        }
    }
}
