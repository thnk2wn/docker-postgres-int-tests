using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Postgres.IntegrationTests.DockerAPI.Docker
{
    public class PostgresDockerContainer
    {
        private const string ContainerName = "PostgresIntTests";
        private const string ImageName = "postgres";
        private const string ImageTag = "latest";
        private const string Port = "5432";

        public string ContainerId { get; private set; }

        public async Task Setup()
        {
            using (var config = DockerClientConfigurationFactory.Create())
            using (var client = config.CreateClient())
            {
                var containers = await client.Containers.ListContainersAsync(new ContainersListParameters() { All = true });
                var container = containers.FirstOrDefault(c => c.Names.Contains("/" + ContainerName));
                
                if (container == null)
                {
                    await CreateImage(client);
                    container = await CreateContainer(client);
                }

                this.ContainerId = container.ID;

                if (container.State != "running")
                {
                    var started = await client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
                    if (!started)
                    {
                        Assert.Fail("Cannot start the docker container");
                    }
                }

                await WaitForContainerReady();
            }
        }

        private static async Task<ContainerListResponse> CreateContainer(IDockerClient client) 
        {
            var hostConfig = CreateHostConfig();

            var volumeMount = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/");

            var parameters = new CreateContainerParameters
            {
                Hostname = "localhost",
                Image = ImageName + ":" + ImageTag,
                Name = ContainerName,
                Tty = false,
                HostConfig = hostConfig,

                Volumes = new Dictionary<string, EmptyStruct> 
                {
                    // TODO: fix hardcode
                    { $"{volumeMount}:/home", new EmptyStruct() }
                },

                ExposedPorts = new Dictionary<string, EmptyStruct> 
                {
                    { Port, new EmptyStruct() }
                }, 
            };

            var response = await client.Containers.CreateContainerAsync(parameters);

            var containers = await client.Containers.ListContainersAsync(new ContainersListParameters { All = true });
            var container = containers.First(c => c.ID == response.ID);
            return container;
        }

        private static async Task CreateImage(DockerClient client)
        {
            // https://github.com/docker/for-win/issues/611 - timeout exceeded waiting for headers. Set docker network dns to fixed
            await client.Images.CreateImageAsync(
                new ImagesCreateParameters
                {
                    FromImage = ImageName,
                    Tag = ImageTag
                }, new AuthConfig() { }, new Progress<JSONMessage>());
        }

        private static HostConfig CreateHostConfig()
        {
            return new HostConfig()
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {
                        $"{Port}/tcp",
                        new List<PortBinding>
                        {
                            new PortBinding { HostIP = "127.0.0.1", HostPort = Port }
                        }
                    },
                }
            };
        }

        private async Task WaitForContainerReady() 
        {
            // TODO: Consider Polly
            const int maxTries = 3;
            var tries = 0;

            for (int i = 0; i < maxTries; i++)
            {
                var code = await PostgresReadyCommand.Execute(ContainerId);

                if (code == 0) break;

                await Task.Delay(250);

                if (++tries == maxTries) 
                {                    
                    Assert.Fail("Cannot determine if postgres is ready");
                }
            }         
        }
    }
}