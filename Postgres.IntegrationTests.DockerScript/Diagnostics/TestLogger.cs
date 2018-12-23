using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Postgres.IntegrationTests.DockerScript.Diagnostics
{
    class TestLogger<T> : ILogger<T>, IDisposable
    {
        private readonly Action<string> output = Console.WriteLine;

        private void LogIt(string message) 
        {
            Trace.WriteLine(message);
        }

        public void Dispose()
        {
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter) 
        {
            LogIt(formatter(state, exception));
        } 

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state) => this;
    }
}