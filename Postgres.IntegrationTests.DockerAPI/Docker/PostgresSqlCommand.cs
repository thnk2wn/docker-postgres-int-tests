using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Docker.DotNet.Models;

namespace Postgres.IntegrationTests.DockerAPI.Docker
{
    public static class PostgresSqlCommand
    {
        public static async Task<long> Execute(string containerId, string filename, string logFilename) 
        {
            Trace.WriteLine($"Initializing docker client");

            using (var config = DockerClientConfigurationFactory.Create())
            using (var client = config.CreateClient())
            {
                var createArgs = new ContainerExecCreateParameters
                {
                    AttachStdin = true,
                    AttachStdout = true,

                    // psql -U postgres -f home/path/script.sql -v ON_ERROR_STOP=1
                    Cmd = new List<string>()
                    { 
                        //"bash",
                        //"/usr/lib/postgresql/11/bin/psql",
                        "touch /home/test.txt",
                        //"-U postgres",
                        //@"-c \'l'"
                        //"-e",
                        //$"-f {filename}",
                        //$"-L {logFilename}",
                        //"-v",
                        //"ON_ERROR_STOP=1"
                    },
                    Tty = false,
                    AttachStderr = true,
                    Detach = false,
                    //Privileged = true
                };

                Trace.WriteLine($"exec create for container {containerId}");
                var created = await client.Containers.ExecCreateContainerAsync(containerId, createArgs);

                Trace.WriteLine($"starting container execution for created id {created.ID}");
                await client.Containers.StartContainerExecAsync(created.ID);

                ContainerExecInspectResponse result;

                do
                {
                    /*
                    psql returns 0 to the shell if it finished normally, 
                    1 if a fatal error of its own occurs (e.g. out of memory, file not found), 
                    2 if the connection to the server went bad and the session was not interactive, 
                    and 3 if an error occurred in a script and the variable ON_ERROR_STOP was set.
                    */
                    result = await client.Containers.InspectContainerExecAsync(created.ID);

                    Trace.WriteLine($"Inspect result => Pid: {result.Pid}, running: {result.Running}, exit code: {result.ExitCode}");

                    var logArgs = new ContainerLogsParameters 
                    {
                        ShowStderr = true,
                        ShowStdout = true
                    };
                    
                    using (var logStream = await client.Containers.GetContainerLogsAsync(containerId, logArgs))
                    using (var reader = new StreamReader(logStream))
                    {
                        var logs = reader.ReadToEnd();
                        Trace.WriteLine(logs);
                    }

                    await Task.Delay(250);
                } while (result.Running);

                return result.ExitCode;
            }
        }
    }
}