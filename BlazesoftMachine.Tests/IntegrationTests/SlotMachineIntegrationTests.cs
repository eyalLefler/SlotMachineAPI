using Shouldly;
using System.Text;
using System.Text.Json;
using BlazesoftMachine.Model.Requests;
using BlazesoftMachine.Tests.IntegrationTests;

[Collection("Integration Tests")]
public class SlotMachineIntegrationTests : IntegrationTestsBase
{
    public SlotMachineIntegrationTests(IntegrationTestFixture fixture) : base(fixture){}

    [Fact]
    public async Task SetPlayerBalance_ShouldReturnValidResponse()
    {
        // Arrange
        var spinBalanceRequestData = new SpinRequest { PlayerId = COMMON_PLAYER_ID, BetAmount = 13 };
        var requestContent = new StringContent(JsonSerializer.Serialize(spinBalanceRequestData), Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync(API_ENDPOINT_PATH_PLAYER_BALANCE_UPDATE, requestContent);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue(); // Ensure it's a good request        
    }

    [Fact]
    public async Task SpinAsync_NotPlayerBalanceInDb_ShouldReturnPlayerNotFoundResponse()
    {
        // Arrange
        var randomPlayerId = COMMON_PLAYER_ID + new Random().Next(8795, 15000);
        var updateBalanceRequestData = new UpdateBalanceRequest { PlayerId = randomPlayerId, Amount = 10 };
        var requestContent = new StringContent(JsonSerializer.Serialize(updateBalanceRequestData), Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync(API_ENDPOINT_PATH_SPIN, requestContent);

        // Assert
        response.IsSuccessStatusCode.ShouldBeFalse(); // Ensure it's a bad request
        var errorMessage = await GetErrorMessageFromResponse(response);
        errorMessage.ShouldBe("Player not found");        
    }

    
    [Fact]
    public async Task SpinAsync_ShouldReturnValidResponse()
    {
        // Arrange
        var updateBalanceRequestData = new UpdateBalanceRequest { PlayerId = COMMON_PLAYER_ID, Amount = 100 };
        var configSlotMachineMatrixSizeRequestData = new ConfigSlotMachineMatrixSizeRequest { MatrixWidth = 12, MatrixHeight = 12 };
        var spinBalanceRequestData = new SpinRequest { PlayerId = COMMON_PLAYER_ID, BetAmount = 13 };        

        var playerBalanceUpdateRequestContent = new StringContent(JsonSerializer.Serialize(updateBalanceRequestData), Encoding.UTF8, "application/json");
        var configMatrixSizeRequestContent = new StringContent(JsonSerializer.Serialize(configSlotMachineMatrixSizeRequestData), Encoding.UTF8, "application/json");
        var spinRequestContent = new StringContent(JsonSerializer.Serialize(spinBalanceRequestData), Encoding.UTF8, "application/json");           


        // Act
        var playerBalanceResponse = await Client.PostAsync(API_ENDPOINT_PATH_PLAYER_BALANCE_UPDATE, playerBalanceUpdateRequestContent);
        var configMatrixSizeResponse = await Client.PostAsync(API_ENDPOINT_PATH_CONFIG_MATRIX_SIZE, configMatrixSizeRequestContent);
        var spinResponse = await Client.PostAsync(API_ENDPOINT_PATH_SPIN, spinRequestContent);

        // Assert
        playerBalanceResponse.IsSuccessStatusCode.ShouldBeTrue();
        configMatrixSizeResponse.IsSuccessStatusCode.ShouldBeTrue();
        spinResponse.IsSuccessStatusCode.ShouldBeTrue(await GetErrorMessageFromResponse(spinResponse));
    }

    [Fact]
    public async Task SpinAsync_InsufficientPlayerBalance_ShouldReturnInsufficientBalanceResponse()
    {
        // Arrange
        var updateBalanceRequestData = new UpdateBalanceRequest{PlayerId = COMMON_PLAYER_ID, Amount = 200};        
        var configSlotMachineMatrixSizeRequestData = new ConfigSlotMachineMatrixSizeRequest { MatrixWidth=7, MatrixHeight=7 };
        var spinBalanceRequestData = new SpinRequest { PlayerId = COMMON_PLAYER_ID, BetAmount = 201 };


        var playerBalanceUpdateRequestContent = new StringContent(JsonSerializer.Serialize(updateBalanceRequestData), Encoding.UTF8, "application/json");                
        var configMatrixSizeRequestContent = new StringContent(JsonSerializer.Serialize(configSlotMachineMatrixSizeRequestData), Encoding.UTF8, "application/json");
        var spinRequestContent = new StringContent(JsonSerializer.Serialize(spinBalanceRequestData), Encoding.UTF8, "application/json");


        // Act
        var playerBalanceResponse = await Client.PostAsync(API_ENDPOINT_PATH_PLAYER_BALANCE_UPDATE, playerBalanceUpdateRequestContent);
        var configMatrixSizeResponse = await Client.PostAsync(API_ENDPOINT_PATH_CONFIG_MATRIX_SIZE, configMatrixSizeRequestContent);
        var spinResponse = await Client.PostAsync(API_ENDPOINT_PATH_SPIN, spinRequestContent);

        // Assert
        playerBalanceResponse.IsSuccessStatusCode.ShouldBeTrue();
        configMatrixSizeResponse.IsSuccessStatusCode.ShouldBeTrue();
        spinResponse.IsSuccessStatusCode.ShouldBeFalse();
        var errorMessage = await GetErrorMessageFromResponse(spinResponse);
        errorMessage.ShouldBe("Insufficient balance");
    }    
}
