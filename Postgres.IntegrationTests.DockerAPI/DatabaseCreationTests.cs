using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Postgres.IntegrationTests.DockerAPI.Docker;

namespace Postgres.IntegrationTests.DockerAPI
{
    [TestClass]
    public class DatabaseCreationTests
    {
        [TestMethod]
        public async Task ExecuteSqlFile_WithDbCreateScript_Returns0ExitCode()
        {
            // TODO: execute create db scripts and verify schema and data.

            // TODO: change hardcoded values
            const string sqlFile = "Script.sql";
            const string logFile = "log.txt";

            var exitCode = await PostgresSqlCommand.Execute( 
                DockerTestContext.Container.ContainerId, 
                sqlFile, 
                logFile);

            exitCode.Should().Be(0, "psql 0 exit code indicates success");
            
            return;
        }        
    }
}
