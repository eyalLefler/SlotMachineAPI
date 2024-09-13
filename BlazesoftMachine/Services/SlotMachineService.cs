using BlazesoftMachine.Data;
using BlazesoftMachine.Model;
using BlazesoftMachine.Model.Requests;
using MongoDB.Driver;

namespace BlazesoftMachine.Services
{
    public class SlotMachineService
    {
        private readonly MongoDbContext _dbContext;

        public SlotMachineService(MongoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SpinResponse> SpinAsync(string playerId, decimal betAmount)
        {
            var player = await _dbContext.Players.Find(p => p.Id == playerId).FirstOrDefaultAsync();
            if (player == null) throw new Exception("Player not found");

            // Check if bet is greater than balance
            if (player.Balance < betAmount) throw new Exception("Insufficient balance");

            // Deduct bet from player balance
            player.Balance -= betAmount;

            // Fetch slot machine configuration
            var config = await _dbContext.Configs.Find(c => c.Id == "slotMachineConfig").FirstOrDefaultAsync();
            if (config == null) throw new Exception("Slot machine configuration not found");

            // Generate random result matrix
            var resultMatrix = GenerateRandomMatrix(config.MatrixWidth, config.MatrixHeight);

            // Calculate win based on result matrix and predefined win lines
            var winAmount = CalculateWin(resultMatrix, betAmount, config);

            // Add win to player balance
            player.Balance += winAmount;

            // Update player's balance in the database
            await _dbContext.Players.ReplaceOneAsync(p => p.Id == playerId, player);

            // Return the spin result
            return new SpinResponse
            {
                Matrix = resultMatrix,
                WinAmount = winAmount,
                PlayerBalance = player.Balance
            };
        }

        public async Task UpdateBalanceAsync(string playerId, decimal amount)
        {
            var player = await _dbContext.Players.Find(p => p.Id == playerId).FirstOrDefaultAsync();
            if (player == null)            
                throw new Exception("Player not found");            
                        
            player.Balance += amount;

            // Commit the update to the database
            await _dbContext.Players.ReplaceOneAsync(p => p.Id == playerId, player);
        }

        private int[,] GenerateRandomMatrix(int width, int height)
        {
            var random = new Random();
            var matrix = new int[height, width];
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)                
                    matrix[row, col] = random.Next(0, 10); // Random integer between 0-9                
            }
            return matrix;
        }

        private decimal CalculateWin(int[,] matrix, decimal betAmount, SlotConfiguration config)
        {
            decimal totalWin = 0;

            // Implement straight row calculation
            for (int row = 0; row < matrix.GetLength(0); row++)
            {
                totalWin += CalculateLineWin(matrix, row, betAmount);
            }

            // Implement diagonal line calculation here (zig-zag win logic)

            return totalWin;
        }

        private decimal CalculateLineWin(int[,] matrix, int row, decimal betAmount)
        {
            int consecutive = 1;
            decimal win = 0;

            for (int col = 1; col < matrix.GetLength(1); col++)
            {
                if (matrix[row, col] == matrix[row, col - 1])
                {
                    consecutive++;
                }
                else
                {
                    if (consecutive >= 3)
                    {
                        win += consecutive * matrix[row, col - 1] * betAmount;
                    }
                    consecutive = 1;
                }
            }

            // Check the last series
            if (consecutive >= 3)            
                win += consecutive * matrix[row, matrix.GetLength(1) - 1] * betAmount;            

            return win;
        }
    }
}
