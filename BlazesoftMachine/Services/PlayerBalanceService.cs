using BlazesoftMachine.Data;
using BlazesoftMachine.Model;
using MongoDB.Driver;

namespace BlazesoftMachine.Services
{
    public class PlayerBalanceService(IMongoDbContext dbContext)
    {
        private readonly IMongoDbContext _dbContext = dbContext;

        public async Task UpdateBalanceAsync(string playerId, decimal amount)
        {
            var filter = Builders<PlayerBalance>.Filter.Eq(p => p.Id, playerId);
            var update = Builders<PlayerBalance>.Update.Set(p => p.Balance, amount);

            // Creating the player balance if not exist
            var updateOptions = new UpdateOptions { IsUpsert = true };

            //Read and update if exist or create new if not exist in atomic operation
            await _dbContext.UpdateOnePlayerBalanceAsync(filter, update, updateOptions);
        }
    }
}
