using Docker.DotNet;
using System;

namespace Postgres.IntegrationTests.DockerAPI.Docker
{
    public static class DockerClientConfigurationFactory
    {
        public static DockerClientConfiguration Create()
        {
            var config = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine"));
            return config;
        }
    }
}