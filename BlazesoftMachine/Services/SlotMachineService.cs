using BlazesoftMachine.Data;
using BlazesoftMachine.Model;
using BlazesoftMachine.Model.Requests;


namespace BlazesoftMachine.Services
{
    public class SlotMachineService(MongoDbContext dbContext)
    {
        private const string SLOT_CONFIG_ID = "slotMachineConfig"; //Currently there is only one slot machine, but when we have more then one, we should change the way we use the slot configuration ID
        private readonly MongoDbContext _dbContext = dbContext;

        public async Task ConfigSlotMachineMatrixSize(int newMatrixHeight, int newMatrixWidth)
        {
            var config = await _dbContext.GetSlotMachineConfigurationByIdAsync(SLOT_CONFIG_ID);
            if (config != null)//Config already exist - just update and commit the change back to the DB
            {
                config.MatrixWidth += newMatrixWidth;
                config.MatrixHeight += newMatrixHeight;
                await _dbContext.UpdateSlotMachineMatrixSize(config);
            }
            else
            {
                var newConfiguration = new SlotConfiguration
                {
                    Id = SLOT_CONFIG_ID,
                    MatrixWidth = newMatrixWidth,
                    MatrixHeight = newMatrixHeight
                };
                await _dbContext.CreateSlotMachineMatrixSizeAsync(newConfiguration);
            }
        }

        public async Task<SpinResponse> SpinAsync(string playerId, decimal betAmount)
        {
            var player = await _dbContext.GetPlayerBalanceByIdAsync(playerId);
            if (player == null) throw new Exception("Player not found");

            // Check if bet is greater than balance
            if (player.Balance < betAmount) throw new Exception("Insufficient balance");

            // Deduct bet from player balance
            player.Balance -= betAmount;

            // Fetch slot machine configuration
            var config = await _dbContext.GetSlotMachineConfigurationByIdAsync(SLOT_CONFIG_ID);
            if (config == null) throw new Exception("Slot machine configuration not found");

            if (config.MatrixHeight <= 0 || config.MatrixWidth <= 0)
                throw new Exception("Matrix size set to 0, Reconfigure the matrix size to valid size.");

            // Generate random result matrix
            var resultMatrix = GenerateRandomMatrix(config.MatrixHeight, config.MatrixWidth);

            // Calculate win based on result matrix and predefined win lines
            var winAmount = CalculateWin(resultMatrix, betAmount, config);

            // Add win to player balance
            player.Balance += winAmount;

            // Update player's balance in the database
            await _dbContext.UpdatePlayerBalanceAsync(player);

            // Return the spin result
            return new SpinResponse
            {
                Matrix = resultMatrix,
                WinAmount = winAmount,
                PlayerBalance = player.Balance
            };
        }


        private int[][] GenerateRandomMatrix(int height, int width)
        {            
            var random = new Random();
            var matrix = new int[height][];
            for (int row = 0; row < height; row++)
            {
                matrix[row] = new int[width];
                for (int col = 0; col < width; col++)
                    matrix[row][col] = random.Next(0, 10); // Random integer between 0-9                
            }
            return matrix;
        }

        private decimal CalculateWin(int[][] matrix, decimal betAmount, SlotConfiguration config)
        {
            decimal totalWin = 0;
                                  
            totalWin += CalculateLineWin(matrix, betAmount);           
            totalWin += CalculateDiagonalWin(matrix, betAmount);

            return totalWin;
        }

        private decimal CalculateLineWin(int[][] matrix, decimal betAmount)
        {
            decimal win = 0;
            int numberOfRows = matrix.Length;
            int numberOfColumns = matrix[0].Length;

            if (numberOfColumns < 3) return 0; //When there are no more then 2 columns, there can't be a line win


            for (int row = 0; row < numberOfRows; row++)
            {
                int consecutive = 1;
                
                for (int col = 1; col < numberOfColumns; col++)
                {
                    if (matrix[row][col] == matrix[row][col - 1])
                    {
                        consecutive++;
                    }
                    else
                    {
                        if (consecutive >= 3)                        
                            win += consecutive * matrix[row][col - 1] * betAmount;                        
                        consecutive = 1;
                        //There will be no more then 1 consecutive in a row (It must start at column zero)
                        break;
                    }
                }

                // Should not forget to add the wins If the entire row is the same number:
                if (consecutive >= 3)
                    win += consecutive * matrix[row][matrix.GetLength(1) - 1] * betAmount;
            }
            return win;
        }

        private decimal CalculateDiagonalWin(int[][] matrix, decimal betAmount)
        {
            decimal totalDiagonalWin = 0;
            int numberOfRows = matrix.Length;
            int numberOfColumns = matrix[0].Length;

            if (numberOfRows <= 1) return 0; //No diagonal wins - just 1 row
            if (numberOfRows <= 2 && numberOfColumns <= 2) return 0; //No diagonal wins in a 2 X 2 (or less) Matrix

            // Check diagonals starting from each row
            for (int startRow = 0; startRow < numberOfRows; startRow++)
            {
                int consecutive = 1;
                int row = startRow;
                int col = 0;
                bool goingDown = true; 

                // Loop through columns
                while (col < numberOfColumns - 1)
                {
                    int nextRow;

                    if (goingDown) // Downward diagonal
                    
                        nextRow = row + 1;
                    
                    else // Upward diagonal
                    
                        nextRow = row - 1;
                    

                    // Check if the next position is within bounds and equal cells
                    if (nextRow >= 0 && nextRow < numberOfRows && matrix[row][col] == matrix[nextRow][col + 1])
                    {
                        consecutive++;
                        row = nextRow;
                        
                        //Change direction if needed:
                        if(row == 0) goingDown = true;
                        if(row == numberOfRows - 1) goingDown = false;
                    }
                    else
                    {
                        if (consecutive >= 3)
                        {
                            totalDiagonalWin += consecutive * matrix[row][col] * betAmount;
                        }
                        consecutive = 1;
                        //There will be no more then 1 consecutive Diagonal win started at the same row (It must start at column zero)
                        break;
                    }
                    col++;
                }

                // Check the last series
                if (consecutive >= 3)
                {
                    totalDiagonalWin += consecutive * matrix[row][col] * betAmount;
                }
            }

            return totalDiagonalWin;
        }
    }
}
