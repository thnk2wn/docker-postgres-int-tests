using Microsoft.Extensions.Logging;

namespace Postgres.IntegrationTests.DockerScript.Diagnostics
{
    public static class TestLoggerFactory 
    {
        private static LoggerFactory loggerFactory;

        static TestLoggerFactory() 
        {
            loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole(LogLevel.Debug);
        }

        public static ILogger<T> Create<T>()
        {
            // don't use MS logger factory here, use our own for immediate flush
            var logger = new TestLogger<T>();
            return logger;
        }

        public static void Shutdown()
        {
            loggerFactory?.Dispose();
        }
    }
}