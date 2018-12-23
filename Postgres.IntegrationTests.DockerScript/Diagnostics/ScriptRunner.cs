using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Postgres.IntegrationTests.DockerScript.Diagnostics
{
    public class ScriptRunner
    {
        public static void Run(string script, TimeSpan? waitTimeout = null)
        {
            var logger = TestLoggerFactory.Create<ScriptRunner>();
            logger.LogInformation($"Running {script}...");

            var timeout = waitTimeout ?? TimeSpan.FromMinutes(2);

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName =  "cmd",
                    Arguments =  $"/c {script}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };
            
            process.OutputDataReceived += (sender, data) => 
            {
                logger.LogInformation(data.Data);
            };

            process.ErrorDataReceived += (sender, data) => 
            {
                logger.LogError(data.Data);
            };

            logger.LogInformation("starting process");
            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit(Convert.ToInt32(timeout.TotalMilliseconds));

            if (!process.HasExited)
            {
                process.Kill();
            }

            logger.LogInformation($"Exit code was: {process.ExitCode}");
            process.Close();

            try 
            {
                if (process.ExitCode != 0) 
                {
                    throw new Exception($"Received failure exit code {process.ExitCode}");
                }
            }
            catch (Exception ex) 
            {
                logger.LogError(ex, "Error accessing exit code");
            }
        }
    }
}