using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Tests.Infrastructure.Testing
{
    public class WaitForWaitBlockToFinish : IWaitForShutdownRequest
    {
        private readonly ConsoleWriterQueue _consoleWriterQueue;
        private readonly WaitBlock _waitBlock;

        public WaitForWaitBlockToFinish(ConsoleWriterQueue consoleWriterQueue, WaitBlock waitBlock)
        {
            _consoleWriterQueue = consoleWriterQueue;
            _waitBlock = waitBlock;
        }

        public void Wait()
        {
            _consoleWriterQueue.Enqueue("Shutdown.Waiting");

            _waitBlock.Wait();

            _consoleWriterQueue.Enqueue("Shutdown.Waited");
        }
    }
}