using System;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model.Web;

namespace Vertica.Integration.Model.Startup
{
    internal class StartWebApiHost : StartupAction
    {
        private readonly ILogger _logger;

        public StartWebApiHost(ILogger logger)
        {
            _logger = logger;
        }

        public override bool IsSatisfiedBy(ExecutionContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            return context.Task is WebApiTask && base.IsSatisfiedBy(context);
        }

        protected override string ActionName
        {
            get { return "url"; }
        }

        protected override ArgumentValidator Validator
        {
            get { return new ArgumentValidator("[absolute-url]", Argument.AbsoluteUrl); }
        }

        protected override void DoExecute(ExecutionContext context)
        {
            using (new WebApiHost(context.ActionArguments[0], Console.Out, _logger, context.Task, context.TaskArguments))
            {
                do
                {
                    Console.WriteLine();
                    Console.WriteLine(@"Press ESCAPE to stop web-service..");
                    Console.WriteLine();

//#if DEBUG
//                    System.Threading.Tasks.Task.Factory.StartNew(() =>
//                    {
//                        var assembly = new FileInfo(typeof(StartWebApiHost).Assembly.Location).Directory;

//                        if (assembly != null)
//                        {
//                            string codeRoot = Path.Combine(assembly.FullName, @"..\..\..\");

//                            using (var watcher = new FileSystemWatcher(codeRoot, "build.watch"))
//                            {
//                                watcher.IncludeSubdirectories = true;
//                                watcher.Changed += (o, args) =>
//                                {
//                                    IntPtr windowHandle = Process.GetCurrentProcess().MainWindowHandle;
//                                    bool postMessage = PostMessage(windowHandle, 0x0100, 0x1B, 0);

//                                    Console.WriteLine("POST: {0}", postMessage);
//                                };
//                                string s = "ssssssssss";

//                                watcher.EnableRaisingEvents = true;

//                                Thread.Sleep(Timeout.Infinite);
//                            }
//                        }
//                    });
//#endif

                } while (WaitingForEscape());
            }
        }

        private static bool WaitingForEscape()
        {
            return Console.ReadKey(intercept: true /* don't display */).Key != ConsoleKey.Escape;
        }

//#if DEBUG
//        [DllImport("user32.dll")]
//        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
//#endif
    }
}