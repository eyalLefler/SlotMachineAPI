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
using Microsoft.VisualStudio.TestPlatform.Utilities;

namespace BlazesoftMachine.Tests.IntegrationTests
{
    public abstract class IntegrationTestsBase
    {
        protected const string API_ENDPOINT_PATH_PLAYER_BALANCE_UPDATE = "/api/PlayerBalance/update";
        protected const string API_ENDPOINT_PATH_SPIN = "/api/SlotMachine/spin";
        protected const string API_ENDPOINT_PATH_CONFIG_MATRIX_SIZE = "/api/SlotMachine/ConfigSlotMachineMatrixSize";
        protected const string COMMON_PLAYER_ID = "testPlayer";
        protected HttpClient Client;

        public IntegrationTestsBase(IntegrationTestFixture fixture)
        {
            Client = fixture.Client;
        }

        protected async Task<string> GetErrorMessageFromResponse(HttpResponseMessage response)
        {
            try
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);
                JsonElement root = doc.RootElement;
                return root.GetProperty("message").GetString();
            }
            catch
            {
                return "Error parsing the response";
            }

        }
    }
}
