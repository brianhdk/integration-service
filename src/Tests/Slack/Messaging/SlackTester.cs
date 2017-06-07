using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Slack;
using Vertica.Integration.Slack.Messaging;
using Vertica.Integration.Slack.Messaging.Handlers;
using Vertica.Integration.Slack.Messaging.Messages;
using Vertica.Integration.Tests.Infrastructure;

namespace Vertica.Integration.Tests.Slack.Messaging
{
    [TestFixture]
    public class SlackTester
    {
        [Test]
        public void AttachToConsoleWriter_Verify_Interaction()
        {
            var messageQueue = Substitute.For<ISlackMessageQueue>();

            using (var context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register(kernel => messageQueue)
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            .Set("Slack.Enabled", "true"))))
                .UseSlack(slack => slack.AttachToConsoleWriter())))
            {
                var writer = context.Resolve<IConsoleWriter>();

                writer.WriteLine("Some message");

                messageQueue.Received(1)
                    .Add(Arg.Is<SlackPostMessageInChannel>(x => x.Text == "Some message"));
            }
        }

        [Test]
        public void SlackMessage_EndToEnd_Test()
        {
            using (var resetEvent = new ManualResetEvent(false))
            {
                var factory = new MessageFactory(resetEvent);
                var shutdown = new ShutdownHandler(resetEvent, TimeSpan.FromSeconds(5));

                using (var context = ApplicationContext.Create(application => application
                    .ConfigureForUnitTest()
                    .Services(services => services
                        .Advanced(advanced => advanced
                            .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                                .Set("Slack.Enabled", "true"))
                            .Register<IWaitForShutdownRequest>(kernel => shutdown)
                            .Register(kernel => factory)))
                    .UseLiteServer(liteServer => liteServer
                        .AddWorker<AddMessageToQueueAndExit>())
                    .UseSlack(slack => slack
                        .MessageHandlers(messageHandlers => messageHandlers
                            .Add<MessageHandler>())
                        .AddToLiteServer())))
                {
                    context.Execute(nameof(LiteServerHost));
                }

                Assert.That(factory.Message.HandledCount, Is.EqualTo(1));
            }
        }

        public class ShutdownHandler : IWaitForShutdownRequest
        {
            private readonly ManualResetEvent _resetEvent;
            private readonly TimeSpan _maxWaitTime;

            public ShutdownHandler(ManualResetEvent resetEvent, TimeSpan maxWaitTime)
            {
                _resetEvent = resetEvent;
                _maxWaitTime = maxWaitTime;
            }

            public void Wait()
            {
                _resetEvent.WaitOne(_maxWaitTime);
            }
        }

        internal class MessageFactory
        {
            public MessageFactory(ManualResetEvent resetEvent)
            {
                Message = new Message(resetEvent);
            }

            public Message Message { get; }
        }

        internal class AddMessageToQueueAndExit : IBackgroundWorker
        {
            private readonly ISlackMessageQueue _queue;
            private readonly MessageFactory _factory;
            private readonly TextWriter _writer;

            public AddMessageToQueueAndExit(ISlackMessageQueue queue, MessageFactory factory, TextWriter writer)
            {
                _queue = queue;
                _factory = factory;
                _writer = writer;
            }

            public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
            {
                _writer.WriteLine("Adding message");
                _queue.Add(_factory.Message);

                return context.Exit();
            }
        }

        internal class MessageHandler : IHandleMessages<Message>
        {
            private readonly TextWriter _writer;

            public MessageHandler(TextWriter writer)
            {
                _writer = writer;
            }

            public Task Handle(Message message, CancellationToken token)
            {
                _writer.WriteLine("Handling message");

                message.MarkAsHandled();

                return Task.FromResult(true);
            }
        }

        internal class Message : ISlackMessage
        {
            private readonly ManualResetEvent _resetEvent;

            private int _handledCount;

            public Message(ManualResetEvent resetEvent)
            {
                if (resetEvent == null) throw new ArgumentNullException(nameof(resetEvent));

                _resetEvent = resetEvent;
            }

            public int HandledCount => _handledCount;

            public void MarkAsHandled()
            {
                Interlocked.Increment(ref _handledCount);

                _resetEvent.Set();
            }
        }
    }
}