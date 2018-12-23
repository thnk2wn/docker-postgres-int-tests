using System.Collections.Generic;
using System.Threading.Tasks;
using Docker.DotNet.Models;

namespace Postgres.IntegrationTests.DockerAPI.Docker
{
    public static class PostgresReadyCommand
    {
        public static async Task<long> Execute(string containerId)
        {
            // container may be up but postgres might not quite be fully started yet
            using (var config = DockerClientConfigurationFactory.Create())
            using (var client = config.CreateClient())
            {
                var createArgs = new ContainerExecCreateParameters
                {
                    AttachStdin = true,
                    AttachStdout = true,
                    Cmd = new List<string>()
                    {
                        "pg_isready",
                        // "-h blah" // to test failures
                    },
                    Tty = true,
                    AttachStderr = true,
                    Detach = true
                };

                var created = await client.Containers.ExecCreateContainerAsync(containerId, createArgs);

                await client.Containers.StartContainerExecAsync(created.ID);

                // pg_isready returns 0 to the shell if the server is accepting connections normally, 
                // 1 if the server is rejecting connections (for example during startup), 
                // 2 if there was no response to the connection attempt, 
                // and 3 if no attempt was made (for example due to invalid parameters).
                var result = await client.Containers.InspectContainerExecAsync(created.ID);

                return result.ExitCode;
            }
        }
    }
}