using BlazesoftMachine.Model.Requests;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace BlazesoftMachine.Tests.IntegrationTests
{
    [Collection("Integration Tests")]
    public class SlotMachinePerformanceTests : IntegrationTestsBase
    {
        private int _performanceTestsRepet = 9000;
        private readonly ITestOutputHelper _output;

        public SlotMachinePerformanceTests(IntegrationTestFixture fixture, ITestOutputHelper output) : base(fixture)
        {
            _output = output;
        }

        [Fact]
        public async Task SetPlayerBalance_ForMultypalPlayers_ShouldReturnValidResponse()
        {

            var random = new Random();

            for (var i = 0; i < _performanceTestsRepet; i++)
            {
                // Arrange
                var playerId = COMMON_PLAYER_ID + random.Next(0, 30);
                var spinRequestData = new SpinRequest { PlayerId = playerId, BetAmount = 13 };
                var requestContent = new StringContent(JsonSerializer.Serialize(spinRequestData), Encoding.UTF8, "application/json");

                // Act
                var response = await Client.PostAsync(API_ENDPOINT_PATH_PLAYER_BALANCE_UPDATE, requestContent);

                // Assert
                response.IsSuccessStatusCode.ShouldBeTrue(); // Ensure it's a good request 
            }                   
        }

        [Fact]
        public async Task Spin_MultypalTimesForSamePlayer_ShouldReturnValidResponse()
        {
            // Arrange            
            var playerID = COMMON_PLAYER_ID + new Random().Next(1154, 5687);

            var updateBalanceRequestData = new UpdateBalanceRequest { PlayerId = playerID, Amount = _performanceTestsRepet + 10 };
            var configSlotMachineMatrixSizeRequestData = new ConfigSlotMachineMatrixSizeRequest { MatrixWidth = 4, MatrixHeight = 3 };
            var spinRequestData = new SpinRequest { PlayerId = playerID, BetAmount = 1 };

            var playerBalanceUpdateRequestContent = new StringContent(JsonSerializer.Serialize(updateBalanceRequestData), Encoding.UTF8, "application/json");
            var configMatrixSizeRequestContent = new StringContent(JsonSerializer.Serialize(configSlotMachineMatrixSizeRequestData), Encoding.UTF8, "application/json");
            var spinRequestContent = new StringContent(JsonSerializer.Serialize(spinRequestData), Encoding.UTF8, "application/json");

            var playerBalanceResponse = await Client.PostAsync(API_ENDPOINT_PATH_PLAYER_BALANCE_UPDATE, playerBalanceUpdateRequestContent);
            var configMatrixSizeResponse = await Client.PostAsync(API_ENDPOINT_PATH_CONFIG_MATRIX_SIZE, configMatrixSizeRequestContent);

            
            for (var i = 0; i < _performanceTestsRepet; i++)
            {
                // Act
                var spinResponse = await Client.PostAsync(API_ENDPOINT_PATH_SPIN, spinRequestContent);

                // Assert
                spinResponse.IsSuccessStatusCode.ShouldBeTrue(); // Ensure it's a good request~                 
            }
        }

        [Fact]
        public async Task Spin_MultypalTimesForSamePlayerParallel_ShouldReturnValidResponse()
        {
            // Arrange            
            var playerID = COMMON_PLAYER_ID + new Random().Next(1154, 5687);

            var updateBalanceRequestData = new UpdateBalanceRequest { PlayerId = playerID, Amount = _performanceTestsRepet + 10 };
            var configSlotMachineMatrixSizeRequestData = new ConfigSlotMachineMatrixSizeRequest { MatrixWidth = 4, MatrixHeight = 3 };
            var spinRequestData = new SpinRequest { PlayerId = playerID, BetAmount = 1 };

            var playerBalanceUpdateRequestContent = new StringContent(JsonSerializer.Serialize(updateBalanceRequestData), Encoding.UTF8, "application/json");
            var configMatrixSizeRequestContent = new StringContent(JsonSerializer.Serialize(configSlotMachineMatrixSizeRequestData), Encoding.UTF8, "application/json");
            

            var playerBalanceResponse = await Client.PostAsync(API_ENDPOINT_PATH_PLAYER_BALANCE_UPDATE, playerBalanceUpdateRequestContent);
            var configMatrixSizeResponse = await Client.PostAsync(API_ENDPOINT_PATH_CONFIG_MATRIX_SIZE, configMatrixSizeRequestContent);


            var tasks = new List<Task<HttpResponseMessage>>();

            // Act: Multiple concurrent requests
            for (int i = 0; i < 100; i++)
            {
                var spinRequestContent = new StringContent(JsonSerializer.Serialize(spinRequestData), Encoding.UTF8, "application/json");
                tasks.Add(Client.PostAsync(API_ENDPOINT_PATH_SPIN, spinRequestContent));
            }
            

            // Await all tasks to complete
            var responses = await Task.WhenAll(tasks);

            // Assert: Ensure all responses were successful
            foreach (var response in responses)            
                response.IsSuccessStatusCode.ShouldBeTrue();               

        }

        [Fact]
        public async Task SpinAndUpdateBalance_MultypalTimesForMultipalPlayerParallel_ShouldReturnValidResponse()
        {
            // Arrange
            int numberOfPlayers = 2;
            int initialBalance = 100;
            decimal betAmount = 10;

            var configSlotMachineMatrixSizeRequestData = new ConfigSlotMachineMatrixSizeRequest { MatrixWidth = 4, MatrixHeight = 7 };
            var configMatrixSizeRequestContent = new StringContent(JsonSerializer.Serialize(configSlotMachineMatrixSizeRequestData), Encoding.UTF8, "application/json");
            var configMatrixSizeResponse = await Client.PostAsync(API_ENDPOINT_PATH_CONFIG_MATRIX_SIZE, configMatrixSizeRequestContent);
                        
            // Create a list to hold tasks for each player
            var playerTasks = new List<Task>();
            var randomForPlayerId = new Random();

            // Act: Simulate multiple players
            for (int i = 0; i < numberOfPlayers; i++)
            {
                string playerId = $"player_{i}_{randomForPlayerId.Next(6144, 8287)}";
                playerTasks.Add(Task.Run(async () =>
                {
                    // Set ititial player's balance
                    var updateBalanceRequestData = new UpdateBalanceRequest { PlayerId = playerId, Amount = initialBalance };
                    var playerBalanceUpdateRequestContent = new StringContent(JsonSerializer.Serialize(updateBalanceRequestData), Encoding.UTF8, "application/json");
                    var addBalanceResponse = await Client.PostAsync(API_ENDPOINT_PATH_PLAYER_BALANCE_UPDATE, playerBalanceUpdateRequestContent);
                    addBalanceResponse.EnsureSuccessStatusCode();

                    var randomForBetAmount = new Random();
                    decimal currentBalance = initialBalance;

                    // Play the game until the balance is depleted
                    while (currentBalance >= 5)
                    {
                        betAmount = randomForBetAmount.Next(1, (int)currentBalance);
                        var spinRequestData = new SpinRequest { PlayerId = playerId, BetAmount = betAmount };
                        var spinRequestContent = new StringContent(JsonSerializer.Serialize(spinRequestData), Encoding.UTF8, "application/json");
                        var spinResponse = await Client.PostAsync(API_ENDPOINT_PATH_SPIN, spinRequestContent);

                        if (spinResponse.IsSuccessStatusCode)
                        {
                            var responseData = await spinResponse.Content.ReadAsStringAsync();                            
                            var spinResult = JsonSerializer.Deserialize<SpinResponse>(responseData);
                            currentBalance = spinResult.PlayerBalance;
                            _output.WriteLine($"ID {playerId}: Bet amount: {betAmount}, WinAmount: {spinResult.WinAmount}, Player balance after bet: {currentBalance}");
                        }
                        else
                        {
                            // Log or handle unexpected response
                            Assert.Fail($"Player {playerId} spin failed: {await spinResponse.Content.ReadAsStringAsync()}");                            
                        }
                    }
                }));
            }

            // Wait for all players to complete
            await Task.WhenAll(playerTasks);

            // Assert or log results if needed
        }
    }
}
