using BlazesoftMachine.Model;

namespace BlazesoftMachine.Services
{
    public class MatrixService : IMatrixService
    {
        public int[][] GenerateRandomMatrix(int height, int width)
        {
            var random = new Random();
            var matrix = new int[height][];
            for (int row = 0; row < height; row++)
            {
                matrix[row] = new int[width];
                for (int col = 0; col < width; col++)
                    matrix[row][col] = random.Next(0, 9); // Random integer between 0-9                
            }
            return matrix;
        }
    }
}
