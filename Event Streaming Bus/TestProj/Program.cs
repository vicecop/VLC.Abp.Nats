using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Vls.Abp.EventStreamingBus;

namespace TestProj
{
    class Program
    {
        interface IPipeElementRepository
        {
            Task<PipeElement> GetAsync();
        }

        class PipesApp
        {
            private readonly IPipeElementRepository _elementRepository;

            public PipesApp(IPipeElementRepository elementRepository)
            {
                _elementRepository = elementRepository;
            }

            public async Task Connect()
            {
                var input = await _elementRepository.GetAsync();
                var output = await _elementRepository.GetAsync();

                input.Connect(output);
            }

            public async Task Disconnect()
            {
                var pipe = await _elementRepository.GetAsync();
                _pipelineBus.Unsubscribe(pipe.Id);
            }
        }

        class MixerConnectedEvent
        {
            public Mixer Mixer { get; set; }
            public IEnumerable<PipeElement> Inputs { get; set; }
        }

        class MixerConnectedEventHandler
        {
            private readonly IPipelineBus _pipelineBus;

            public MixerConnectedEventHandler(IPipelineBus pipelineBus)
            {
                _pipelineBus = pipelineBus;
            }

            public async Task Handle(MixerConnectedEvent @event)
            {
                var mixer = @event.Mixer;

                var inputChannels = @event.Inputs.Select(i => _pipelineBus.GetChannel(i.Id));

                var combined = Observable.CombineLatest(inputChannels, (src) => mixer.Process(src));
                var subscription = Observable.Interval(TimeSpan.FromMilliseconds(100))
                    .WithLatestFrom(combined, (clock, mix) => mix)
                    .DistinctUntilChanged()
                    .Subscribe(async datagram =>
                    {
                        await _pipelineBus.PublishAsync(mixer.Id, datagram);
                    });
            }
        }

        enum PipeElementType
        {
            Source,
            Mixer,
            Filter,
            Consumer
        }

        abstract class PipeElement
        {
            public Guid Id { get; set; }
            public List<Guid> Inputs { get; set; }
            public PipeElementType ElementType { get; set; }

            public PipeElement()
            {

            }

            internal PipeElement(PipeElementType elementType)
            {
                ElementType = elementType;
            }

            public static PipeElement CreateNew(Guid id, PipeElementType elementType) => 
                elementType switch
                {
                    PipeElementType.Source => new Source(),
                    PipeElementType.Mixer => new Mixer(),
                    PipeElementType.Filter => throw new NotImplementedException(),
                    PipeElementType.Consumer => throw new NotImplementedException(),
                    _ => throw new NotImplementedException()
                };

            public abstract void Connect(PipeElement pipeElement);
            public abstract void Disconnect(PipeElement pipeElement);

            public abstract PipelineDatagram Process(IEnumerable<PipelineDatagram> datagrams);
        }

        sealed class Source : PipeElement
        {
            public override void Connect(PipeElement pipeElement)
            {
                throw new NotImplementedException();
            }

            public override void Disconnect(PipeElement pipeElement)
            {
                throw new NotImplementedException();
            }

            public override PipelineDatagram Process(IEnumerable<PipelineDatagram> datagrams)
            {
                throw new NotImplementedException();
            }
        }

        sealed class Mixer : PipeElement
        {
            internal Mixer()
                : base(PipeElementType.Mixer)
            {
                
            }

            public override void Connect(PipeElement pipeElement)
            {
                throw new NotImplementedException();
            }

            public override void Disconnect(PipeElement pipeElement)
            {
                throw new NotImplementedException();
            }

            public override PipelineDatagram Process(IEnumerable<PipelineDatagram> datagrams)
            {
                return PipelineDatagram.Combine(datagrams);
            }
        }

        static void Main(string[] args)
        {
            var device1 = Observable.Interval(TimeSpan.FromMilliseconds(1000)).StartWith(0);
            var device2 = Observable.Interval(TimeSpan.FromMilliseconds(3000)).StartWith(0);
            var device3 = Observable.Interval(TimeSpan.FromMilliseconds(5000)).StartWith(0);

            var mixer = Observable.CombineLatest(device1, device2, device3, (dev1, dev2, dev3) => (dev1, dev2, dev3));

            Observable.Interval(TimeSpan.FromMilliseconds(100))
                .WithLatestFrom(mixer, (clock, mix) => mix)
                .DistinctUntilChanged()
                .Subscribe(values => 
                {
                    Console.WriteLine(values);
                });

            Console.ReadLine();

            var reader = new Reader();

            IPipelineBus bus = null;

            bus.Subscribe(reader.Id);
        }
    }
}
