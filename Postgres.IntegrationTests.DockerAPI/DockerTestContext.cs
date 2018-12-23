using System.Threading.Tasks;
using Docker.DotNet.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Postgres.IntegrationTests.DockerAPI.Docker;

namespace Postgres.IntegrationTests.DockerAPI
{
    [TestClass]
    public static class DockerTestContext 
    {
        public static PostgresDockerContainer Container {get; private set;}

        [AssemblyInitialize]
        public static async Task AssemblyInitialize(TestContext context)
        {
            Container = new PostgresDockerContainer();
            await Container.Setup();
        }

        [AssemblyCleanup]
        public static async Task AssemblyCleanup()
        {
            using (var config = DockerClientConfigurationFactory.Create())
            using (var client = config.CreateClient())
            {
                await client.Containers.StopContainerAsync(
                    Container.ContainerId, 
                    new ContainerStopParameters());

                await client.Containers.RemoveContainerAsync(
                    Container.ContainerId, 
                    new ContainerRemoveParameters { Force = true });
            }
        }
    }
}