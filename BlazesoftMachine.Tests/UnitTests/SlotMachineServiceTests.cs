using Moq;
using Xunit;
using BlazesoftMachine.Services;
using BlazesoftMachine.Data;
using BlazesoftMachine.Model;
using System.Threading.Tasks;
using Shouldly;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;

namespace BlazesoftMachine.Tests.UnitTests
{
    public class SlotMachineServiceTests
    {
        private const string COMMON_PLAYER_ID = "testPlayer";

        [Fact]

        public async Task SpinAsync_WhenPlayerNotExist_Exception()
        {
            // Arrange  
            var mockSlotMachineConfiguration = new SlotConfiguration { Id = "slotMachineConfig", MatrixHeight = 3, MatrixWidth = 3 };
            var mockDbContext = new Mock<IMongoDbContext>();
            mockDbContext.Setup(db => db.GetSlotMachineConfigurationByIdAsync(It.IsAny<string>())).ReturnsAsync(mockSlotMachineConfiguration);
            var matrixService = new MatrixService();
            var service = new SlotMachineService(mockDbContext.Object, matrixService);


            // Act            
            var exception = await Record.ExceptionAsync(() => service.SpinAsync("Not Existing Player ID", 10));


            // Assert
            exception.ShouldNotBeNull();
            exception.ShouldBeOfType<Exception>();
            exception.Message.ShouldBe("Player not found");
        }

        [Fact]

        public async Task SpinAsync_WhenslotMachineConfigNotExist_Exception()
        {
            // Arrange 
            var mockPlayer = new PlayerBalance { Id = COMMON_PLAYER_ID, Balance = 100 };
            var mockDbContext = new Mock<IMongoDbContext>();
            mockDbContext.Setup(db => db.GetPlayerBalanceByIdAsync(It.IsAny<string>())).ReturnsAsync(mockPlayer);
            var matrixService = new MatrixService();
            var service = new SlotMachineService(mockDbContext.Object, matrixService);


            // Act            
            var exception = await Record.ExceptionAsync(() => service.SpinAsync(COMMON_PLAYER_ID, 10));


            // Assert
            exception.ShouldNotBeNull();
            exception.ShouldBeOfType<Exception>();
            exception.Message.ShouldBe("Slot machine configuration not found");
        }

        [Fact]
        public async Task SpinAsync_WhenPlayerBalanceNotSufficient_Exception()
        {
            // Arrange            
            var mockPlayer = new PlayerBalance { Id = COMMON_PLAYER_ID, Balance = 8 };
            var mockSlotMachineConfiguration = new SlotConfiguration { Id = "slotMachineConfig", MatrixHeight = 3, MatrixWidth = 3 };
            var mockDbContext = CreateMockDbContext(mockPlayer, mockSlotMachineConfiguration);
            var matrixService = new MatrixService();
            var service = new SlotMachineService(mockDbContext.Object, matrixService);


            // Act
            var exception = await Record.ExceptionAsync(() => service.SpinAsync(COMMON_PLAYER_ID, 10));


            // Assert
            exception.ShouldNotBeNull();
            exception.ShouldBeOfType<Exception>();
            exception.Message.ShouldBe("Insufficient balance");
        }

        [Fact]
        public async Task SpinAsync_WhenMatrixSizeToSmall_Exception()
        {
            // Arrange            
            var mockPlayer = new PlayerBalance { Id = COMMON_PLAYER_ID, Balance = 100 };
            var mockSlotMachineConfiguration = new SlotConfiguration { Id = "slotMachineConfig", MatrixHeight = 0, MatrixWidth = 3 };
            var mockDbContext = CreateMockDbContext(mockPlayer, mockSlotMachineConfiguration);
            var matrixService = new MatrixService();
            var service = new SlotMachineService(mockDbContext.Object, matrixService);


            // Act
            var exception = await Record.ExceptionAsync(() => service.SpinAsync(COMMON_PLAYER_ID, 10));


            // Assert
            exception.ShouldNotBeNull();
            exception.ShouldBeOfType<Exception>();
            exception.Message.ShouldBe("Matrix size set to 0, Reconfigure the matrix size to valid size.");
        }
                              
        [Fact]
        public async Task SpinAsync_CalculatWinAmount()
        {
            // Arrange            
            var expectedArray = new int[][] {
                    [3, 3, 3, 4, 5 ],
                    [2, 3, 2, 3, 3],
                    [1, 2, 3, 3, 3]
                 };
            ArrangeForSpin(expectedArray, 100, out Mock<IMongoDbContext> mockDbContext, out Mock<IMatrixService> matrixService);
            var service = new SlotMachineService(mockDbContext.Object, matrixService.Object);


            // Act
            var result = await service.SpinAsync(COMMON_PLAYER_ID, 10);


            /*
                [0, 0] + [1, 0] + [2, 0](top row across, i.e.sample line 2) = (3 + 3 + 3) * bet +
                [0, 0] + [1, 1] + [2, 2] + [3, 1](diagonally from top row) = (3 + 3 + 3 + 3) * bet +
                [0, 1] + [1, 2] + [2, 1](diagonally from 2nd row) = (2 + 2 + 2) * bet  
            bet = 10  
            Total result should be: 27 * 10 = 270 */

            // Assert
            result.WinAmount.ShouldBe(270);

        }

        [Fact]
        public async Task SpinAsync_PlayerBalanceShouldUpdateAfterWin()
        {
            // Arrange            
            var expectedArray = new int[][] {
                    [3, 3, 3, 4, 5 ],
                    [2, 3, 2, 3, 3],
                    [1, 2, 3, 3, 3]
                 };
            ArrangeForSpin(expectedArray, 100, out Mock<IMongoDbContext> mockDbContext, out Mock<IMatrixService> matrixService);
            var service = new SlotMachineService(mockDbContext.Object, matrixService.Object);


            // Act
            var result = await service.SpinAsync(COMMON_PLAYER_ID, 10);

            /* Bat = 10
             * Total win: 27 * 10 = 270 
             * Player balance initial value = 100
             * Player balance after spi should be: 100 - 10 + 270 = 360
             */

            // Assert
            result.PlayerBalance.ShouldBe(360);
        }


        [Fact]
        public async Task SpinAsync_ReturnArrayShouldBeSameAsInput()
        {
            // Arrange
            var expectedArray = new int[][] {
                    [3, 3, 3, 4, 5 ],
                    [2, 3, 2, 3, 3],
                    [1, 2, 3, 3, 3]
                 };
            ArrangeForSpin(expectedArray, 100, out Mock<IMongoDbContext> mockDbContext, out Mock<IMatrixService> matrixService);
            var service = new SlotMachineService(mockDbContext.Object, matrixService.Object);


            // Act
            var result = await service.SpinAsync(COMMON_PLAYER_ID, 10);


            // Assert
            result.Matrix.ShouldBe(expectedArray);
        }

        [Fact]
        public async Task SpinAsync_CalculatWinAmountWhenNoWin()
        {
            // Arrange            
            var expectedArray = new int[][] {
                    [1, 2, 3, 4, 5 ],
                    [6, 7, 8, 9, 1],
                    [2, 3, 4, 5, 6]
                 };
            ArrangeForSpin(expectedArray, 100, out Mock<IMongoDbContext> mockDbContext, out Mock<IMatrixService> matrixService);
            var service = new SlotMachineService(mockDbContext.Object, matrixService.Object);


            // Act
            var result = await service.SpinAsync(COMMON_PLAYER_ID, 10);


            /*
             * No consecutive numbers - so no win at all
             * bet = 10 
             * Player balance initial value = 100             
             */

            // Assert
            result.WinAmount.ShouldBe(0);
            result.PlayerBalance.ShouldBe(90);
        }

        [Fact]
        public async Task SpinAsync_CalculatWinAmountWhenMatrixTooSmallToWin()
        {
            // Arrange            
            var expectedArray = new int[][] {
                    [1, 1],
                    [1, 1]
                 };
            ArrangeForSpin(expectedArray, 100, out Mock<IMongoDbContext> mockDbContext, out Mock<IMatrixService> matrixService);
            var service = new SlotMachineService(mockDbContext.Object, matrixService.Object);


            // Act
            var result = await service.SpinAsync(COMMON_PLAYER_ID, 10);


            /*
             * No consecutive numbers - so no win at all
             * bet = 10 
             * Player balance initial value = 100             
             */

            // Assert
            result.WinAmount.ShouldBe(0);
            result.PlayerBalance.ShouldBe(90);
        }

        [Fact]
        public async Task SpinAsync_CalculatWinAmount2()
        {
            // Arrange            
            var expectedArray = new int[][] {
                    [3, 3, 3, 3, 3 ],
                    [2, 3, 2, 3, 3],
                    [1, 2, 3, 3, 3]
                 };
            ArrangeForSpin(expectedArray, 100, out Mock<IMongoDbContext> mockDbContext, out Mock<IMatrixService> matrixService);
            var service = new SlotMachineService(mockDbContext.Object, matrixService.Object);


            // Act
            var result = await service.SpinAsync(COMMON_PLAYER_ID, 10);


            /*
                [0, 0] + [1, 0] + [2, 0] + [3, 0] + [4, 0] (top row across, i.e.sample line 2) = (3 + 3 + 3 + 3 + 3) * bet +
                [0, 0] + [1, 1] + [2, 2] + [3, 1] + [4, 0] (diagonally from top row) = (3 + 3 + 3 + 3 + 3) * bet +
                [0, 1] + [1, 2] + [2, 1](diagonally from 2nd row) = (2 + 2 + 2) * bet  
            bet = 10  
            Total result should be: 36 * 10 = 360 */

            // Assert
            result.WinAmount.ShouldBe(360);

        }

        [Fact]
        public async Task SpinAsync_CalculatWinAmount3()
        {
            // Arrange            
            var expectedArray = new int[][] {
                    [3, 3, 3],
                    [3, 3, 3],
                    [3, 3, 3]
                 };
            ArrangeForSpin(expectedArray, 100, out Mock<IMongoDbContext> mockDbContext, out Mock<IMatrixService> matrixService);
            var service = new SlotMachineService(mockDbContext.Object, matrixService.Object);


            // Act
            var result = await service.SpinAsync(COMMON_PLAYER_ID, 10);


            /*
                [0, 0] + [1, 0] + [2, 0] = (3 + 3 + 3) * bet + Line
                [0, 1] + [1, 1] + [2, 1] = (3 + 3 + 3) * bet + Line
                [0, 2] + [1, 2] + [2, 2] = (3 + 3 + 3) * bet + Line
                [0, 0] + [1, 1] + [2, 2] = (3 + 3 + 3) * bet + Diagonal
                [0, 1] + [1, 2] + [2, 1] = (3 + 3 + 3) * bet + Diagonal
            bet = 10  
            Total result should be: 45 * 10 = 450 */

            // Assert
            result.WinAmount.ShouldBe(450);

        }

        private static void ArrangeForSpin(int[][] expectedArray, decimal playerInitialBalance, out Mock<IMongoDbContext> mockDbContext, out Mock<IMatrixService> matrixService)
        {
            var matrixHeight = expectedArray == null ? 0 : expectedArray.Length;
            var matrixWidth = expectedArray == null || expectedArray.Length <= 0 ? 0 : expectedArray[0].Length;
            var mockPlayer = new PlayerBalance { Id = COMMON_PLAYER_ID, Balance = playerInitialBalance };
            var mockSlotMachineConfiguration = new SlotConfiguration { Id = "slotMachineConfig", MatrixHeight = matrixHeight, MatrixWidth = matrixWidth };

            mockDbContext = CreateMockDbContext(mockPlayer, mockSlotMachineConfiguration);

            matrixService = new Mock<IMatrixService>();
            matrixService.Setup(mat => mat.GenerateRandomMatrix(It.IsAny<int>(), It.IsAny<int>())).
                Returns(expectedArray);
        }

        private static Mock<IMongoDbContext> CreateMockDbContext(PlayerBalance playerToReturn, SlotConfiguration slotConfigurationToReturn)
        {
            var mockDbContext = new Mock<IMongoDbContext>();
            mockDbContext.Setup(db => db.GetPlayerBalanceByIdAsync(It.IsAny<string>())).ReturnsAsync(playerToReturn);
            mockDbContext.Setup(db => db.GetSlotMachineConfigurationByIdAsync(It.IsAny<string>())).ReturnsAsync(slotConfigurationToReturn);
           


            //This logic will make sure to update the player balance if he have the needed amount.
            mockDbContext.Setup(db => db.FindOnePlayerBalanceAndUpdateAsync(
                    It.IsAny<FilterDefinition<PlayerBalance>>(),
                    It.IsAny<UpdateDefinition<PlayerBalance>>(),
                    It.IsAny<FindOneAndUpdateOptions<PlayerBalance>>()))
                .ReturnsAsync((FilterDefinition<PlayerBalance> filter,
                               UpdateDefinition<PlayerBalance> update,
                               FindOneAndUpdateOptions<PlayerBalance> options) =>
            {

                var playerBalance = new PlayerBalance
                {
                    Id = playerToReturn.Id,
                    Balance = playerToReturn.Balance
                };

                decimal betAmount = 0;
                var filterBson = filter.Render(BsonSerializer.SerializerRegistry.GetSerializer<PlayerBalance>(),
                                               BsonSerializer.SerializerRegistry);

                if (filterBson.TryGetValue("Balance", out var balanceCondition) && balanceCondition is BsonDocument balanceDoc)
                {
                    if (balanceDoc.TryGetValue("$gte", out var gteValue))
                    {
                        betAmount = gteValue.ToDecimal();
                    }
                }

                // Extract the update details
                var updateBsonValue = update.Render(
                    BsonSerializer.SerializerRegistry.GetSerializer<PlayerBalance>(),
                    BsonSerializer.SerializerRegistry);

                if (updateBsonValue is BsonDocument updateBson)
                    // Assume we're using `$inc` operator to change the balance
                    if (updateBson.TryGetValue("$inc", out var incElement) && incElement is BsonDocument incDoc)
                    {
                        if (incDoc.TryGetValue("Balance", out var incrementValueBson))
                        {
                            var incrementValue = incrementValueBson.ToDecimal();

                            // Calculate the new balance to check if it's sufficient
                            var newBalance = playerBalance.Balance + incrementValue;

                            // Check if the current balance is sufficient for the bet before updating
                            if (playerBalance.Balance < betAmount || newBalance < 0)
                            {
                                // If insufficient funds or the resulting balance would be negative, return null
                                return null;
                            }

                            // Apply the increment to the player's balance
                            playerBalance.Balance = newBalance;
                        }
                    }

                // Return the modified player balance
                return playerBalance;
            });
            return mockDbContext;
        }
    }
}
