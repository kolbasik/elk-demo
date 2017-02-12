using System;
using System.Threading;
using log4net;
using log4net.Config;

namespace ElkDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            Console.WriteLine("Press any key to start ...");
            Console.ReadKey(true);

            var cts = new CancellationTokenSource();
            var thread = new Thread(state =>
            {
                var token = (CancellationToken) state;
                var logger = LogManager.GetLogger(typeof(Program));
                try
                {
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();
                        using (CorrelationScope.New())
                        {
                            using (NDC.Push("verbose messages"))
                            {
                                logger.Debug("debug message.");
                                logger.Info("info message.");
                            }
                            using (NDC.Push("notification messages"))
                            {
                                logger.Warn("warn message.");
                                logger.Error("error message.", new NotSupportedException("ELK TEST!"));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Fatal("fatal message.", ex);
                }
            });
            thread.Start(cts.Token);

            Console.WriteLine("Press any key to stop ...");
            Console.ReadKey(true);

            cts.Cancel();
            thread.Join();

            Console.WriteLine("Press any key to exit ...");
            Console.ReadKey(true);
        }
    }

    public static class CorrelationScope
    {
        public static IDisposable New()
        {
            return ThreadContext.Stacks["CorrelationID"].Push(Guid.NewGuid().ToString("N"));
        }
    }
}
