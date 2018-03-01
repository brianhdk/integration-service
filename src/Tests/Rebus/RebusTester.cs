using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Rebus;
using Vertica.Integration.Tests.Infrastructure.Testing;

namespace Vertica.Integration.Tests.Rebus
{
    [TestFixture]
    [Ignore("Rebus currently does not work as intended due to a double-registration issue.")]
    public class RebusTester
    {
        [Test]
        public void SendMessageFromWorker_InMemory_GetsHandled()
        {
            using (var waitBlock = new WaitBlock())
            {
                var message = new MyMessage(Guid.NewGuid());
                var receivedMessages = new ConcurrentQueue<MyMessage>();

                var waitBlockLocal = new[] { waitBlock };

                using (var context = ApplicationContext.Create(application => application
                    .ConfigureForUnitTest()
                    .WithWaitBlock(waitBlockLocal[0])
                    .UseRebus(rebus => rebus
                        .Bus((bus, kernel) => bus
                            .Routing(routing => routing
                                .TypeBased()
                                    .Map<MyMessage>("inputQueue"))
                            .Transport(transport => transport
                                .UseInMemoryTransport(new InMemNetwork(), "inputQueue")))
                        .Handlers(handlers => handlers
                            .Handler<MyMessageHandler>())
                        .AddToLiteServer())
                    .UseLiteServer(liteServer => liteServer
                        .AddWorker<SendMyMessageWorker>())
                    .Services(services => services
                        .Advanced(advanced => advanced
                            .Register(kernel => message)
                            .Register(kernel => receivedMessages)))))
                {
                    context.Execute(nameof(LiteServerHost));
                }

                Assert.That(receivedMessages.Count, Is.EqualTo(1));

                MyMessage receivedMessage;
                bool canDequeue = receivedMessages.TryDequeue(out receivedMessage);

                Assert.IsTrue(canDequeue);
                Assert.That(receivedMessage, Is.EqualTo(message));
                Assert.IsTrue(receivedMessage.IsHandled);
            }
        }

        public class SendMyMessageWorker : IBackgroundWorker
        {
            private readonly IBus _bus;
            private readonly MyMessage _message;
            
            public SendMyMessageWorker(IBus bus, MyMessage message)
            {
                _bus = bus;
                _message = message;
            }

            public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
            {
                _bus.SendLocal(_message).Wait(token);

                return context.Exit();
            }

            public override string ToString()
            {
                return nameof(SendMyMessageWorker);
            }
        }

        public class MyMessageHandler : IHandleMessages<MyMessage>
        {
            private readonly ConcurrentQueue<MyMessage> _receivedMessages;
            private readonly WaitBlock _waitBlock;

            public MyMessageHandler(WaitBlock waitBlock, ConcurrentQueue<MyMessage> receivedMessages)
            {
                _waitBlock = waitBlock;
                _receivedMessages = receivedMessages;
            }

            public Task Handle(MyMessage message)
            {
                message.IsHandled = true;

                _receivedMessages.Enqueue(message);

                _waitBlock.Release();

                return Task.FromResult(true);
            }
        }

        public class MyMessage : IEquatable<MyMessage>
        {
            public MyMessage(Guid id)
            {
                Id = id;
            }

            public Guid Id { get; }
            public bool IsHandled { get; set; }

            public bool Equals(MyMessage other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Id.Equals(other.Id);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((MyMessage) obj);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }

            public static bool operator ==(MyMessage left, MyMessage right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(MyMessage left, MyMessage right)
            {
                return !Equals(left, right);
            }
        }
    }
}