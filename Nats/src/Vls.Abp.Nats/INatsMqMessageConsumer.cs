using System.Threading.Tasks;
using System;
using NATS.Client;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ExceptionHandling;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Volo.Abp.Threading;

namespace Vls.Abp.Nats
{
    public interface INatsMqMessageConsumer
    {
        Task BindAsync(string routingKey);

        Task UnbindAsync(string routingKey);

        void OnMessageReceived(Func<MsgHandlerEventArgs, Task> callback);
    }

    public class NatsMqMessageConsumer : INatsMqMessageConsumer, ITransientDependency, IDisposable
    {
        private readonly ILogger<NatsMqMessageConsumer> _logger;
        private readonly AbpTimer _timer;

        protected INatsMqConnectionManager ConnectionManager { get; }
        protected IExceptionNotifier ExceptionNotifier { get; }

        protected IConnection Connection { get; }

        protected ConcurrentBag<Func<MsgHandlerEventArgs, Task>> Callbacks { get; }

        public NatsMqMessageConsumer(ILogger<NatsMqMessageConsumer> logger, INatsMqConnectionManager connectionManager, AbpTimer timer, IExceptionNotifier exceptionNotifier)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ConnectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            ExceptionNotifier = exceptionNotifier ?? throw new ArgumentNullException(nameof(exceptionNotifier));

            Callbacks = new ConcurrentBag<Func<MsgHandlerEventArgs, Task>>();

            _timer = timer;
            _timer.Period = 5000;
            _timer.Elapsed += TimerElapsed;
            _timer.RunOnStart = true;
        }

        public void Initialize()
        {
            _timer.Start();
        }

        private void TimerElapsed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public Task BindAsync(string routingKey)
        {
            throw new NotImplementedException();
        }

        public Task UnbindAsync(string routingKey)
        {
            throw new NotImplementedException();
        }

        public void OnMessageReceived(Func<MsgHandlerEventArgs, Task> callback)
        {

        }

        protected virtual async Task HandleIncomingMessage(MsgHandlerEventArgs msgHandlerEventArgs)
        {
            try
            {
                foreach (var callback in Callbacks)
                {
                    await callback(msgHandlerEventArgs);
                }

                channel.BasicAck(basicDeliverEventArgs.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                await ExceptionNotifier.NotifyAsync(ex);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
