using BlazesoftMachine.Model;
using MongoDB.Driver;

namespace BlazesoftMachine.Data
{
    public interface IMongoDbContext
    {        
        Task<PlayerBalance> GetPlayerBalanceByIdAsync(string playerId);
        Task UpdatePlayerBalanceAsync(PlayerBalance player);
        Task UpdateOnePlayerBalanceAsync(FilterDefinition<PlayerBalance> filter, UpdateDefinition<PlayerBalance> update, UpdateOptions updateOptions);
        Task<PlayerBalance> FindOnePlayerBalanceAndUpdateAsync(FilterDefinition<PlayerBalance> filter, UpdateDefinition<PlayerBalance> update, FindOneAndUpdateOptions<PlayerBalance> updateOptions);



        Task CreateSlotMachineConfigurationAsync(SlotConfiguration newConfiguration); 
        Task<SlotConfiguration> GetSlotMachineConfigurationByIdAsync(string slotMachineId);        
        Task UpdateSlotMachineConfigurationAsync(SlotConfiguration slot);        
    }
}