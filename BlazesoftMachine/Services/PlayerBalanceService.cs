using BlazesoftMachine.Data;
using BlazesoftMachine.Model;

namespace BlazesoftMachine.Services
{
    public class PlayerBalanceService(MongoDbContext dbContext)
    {
        private readonly MongoDbContext _dbContext = dbContext;

        public async Task UpdateBalanceAsync(string playerId, decimal amount)
        {
            var player = await _dbContext.GetPlayerBalanceByIdAsync(playerId);
            if (player != null)//Player exist - just update the player balance and commit the change back to the DB
            {
                player.Balance = amount;
                await _dbContext.UpdatePlayerBalanceAsync(player);
            }
            else
            {
                var newPlayer = new PlayerBalance
                {
                    Id = playerId,
                    Balance = amount
                };
                await _dbContext.CreatePlayerBalanceAsync(newPlayer);
            }
        }
    }
}
