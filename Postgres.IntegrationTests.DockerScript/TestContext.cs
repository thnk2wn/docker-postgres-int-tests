using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Postgres.IntegrationTests.DockerScript.Diagnostics;

namespace Postgres.IntegrationTests.DockerScript
{
    public class TestContext
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            var testDir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
            var rootDir = new DirectoryInfo(Path.Combine(testDir.FullName, @"..\..\..\"));

            var filename = Path.Combine(testDir.FullName, @"test-startup.bat");
            var command = $"{filename} {rootDir.FullName.Replace("\\", "/")}";
            ScriptRunner.Run(command);
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            var filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\test-stop.bat");
            ScriptRunner.Run(filename);
            TestLoggerFactory.Shutdown();
        }
    }
}