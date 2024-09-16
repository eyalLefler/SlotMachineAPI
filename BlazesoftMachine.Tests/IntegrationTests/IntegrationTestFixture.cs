using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MongoDB.Driver;
using Xunit;
using BlazesoftMachine.Services;
using BlazesoftMachine.Data;
using BlazesoftMachine.Model;
using System.Threading.Tasks;
using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Text;
using Amazon.Runtime.Internal;
using System.Text.Json;
using Amazon.Runtime;
using BlazesoftMachine.Model.Requests;

namespace BlazesoftMachine.Tests.IntegrationTests
{
    public class IntegrationTestFixture : IAsyncLifetime
    {
        private readonly IContainer _mongoContainer;
        private readonly WebApplicationFactory<Program> _factory;

        private const int MONGO_CONTAINER_PORT = 27017;

        public HttpClient Client;


        public IntegrationTestFixture()
        {           
            // Set up the MongoDB container
            _mongoContainer = new ContainerBuilder()
                .WithImage("mongo:latest")
                .WithPortBinding(MONGO_CONTAINER_PORT, MONGO_CONTAINER_PORT)
                .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", "root")
                .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", "example")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(MONGO_CONTAINER_PORT))
                .Build();

            // Set up the WebApplicationFactory for the API
            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => { });
        }

        public async Task InitializeAsync()
        {
            await _mongoContainer.StartAsync();

            // Update connection string to use the MongoDB container
            Environment.SetEnvironmentVariable("MONGO_CONNECTION_STRING", $"mongodb://root:example@localhost:{MONGO_CONTAINER_PORT}");

            // Create HttpClient to make requests to the API
            Client = _factory.CreateClient();
        }

        public async Task DisposeAsync()
        {
            Client.Dispose();
            await _mongoContainer.StopAsync();
            await _mongoContainer.DisposeAsync();
        }
    }
}
