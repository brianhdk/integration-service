using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Slack.Messaging.Handlers;

namespace Vertica.Integration.Slack.Messaging
{
    public class SlackMessageQueue : ISlackMessageQueue
    {
        private readonly ISlackMessageHandlerFactory _factory;
        private readonly ILogger _logger;

        private readonly CancellationToken _token;
        private readonly BlockingCollection<Func<Task>> _queue;

        public SlackMessageQueue(ISlackMessageHandlerFactory factory, IShutdown shutdown, ILogger logger, ISlackConfiguration configuration)
        {
            _factory = factory;
            _logger = logger;

            if (!configuration.Enabled)
                return;

            _token = shutdown.Token;
            _queue = new BlockingCollection<Func<Task>>();

            Consumer = Task.Run(() =>
            {
                while (!_token.IsCancellationRequested)
                {
                    Func<Task> messageInvoker = null;

                    try
                    {
                        messageInvoker = _queue.Take(_token);
                    }
                    catch (OperationCanceledException)
                    {
                    }

                    Task task = messageInvoker?.Invoke();

                    if (task != null)
                    {
                        try
                        {
                            task.Wait(_token);
                        }
                        catch (OperationCanceledException)
                        {
                            // ignore if operation is cancelled.
                        }
                        catch (AggregateException ex)
                        {
                            foreach (Exception exception in ex.Flatten().InnerExceptions)
                                _logger.LogError(exception);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex);
                        }
                    }
                }
            }, _token);
        }

        public void Add<T>(T message) where T : ISlackMessage
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            if (!_token.IsCancellationRequested)
            {
                _queue.Add(() =>
                {
                    IHandleMessages<T> handler = _factory.Get<T>();

                    if (handler == null)
                        throw new InvalidOperationException($"Handler for '{typeof(T).FullName} could not be found.");

                    return handler.Handle(message, _token);

                }, _token);
            }
        }

        public Task Consumer { get; }
    }
}