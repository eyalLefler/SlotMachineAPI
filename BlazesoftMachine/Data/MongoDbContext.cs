using BlazesoftMachine.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Numerics;



namespace BlazesoftMachine.Data
{
    public class MongoDbContext
    {
        private const string PLAYER_BALANCE_COLLECTION_NAME = "PlayerBalance";
        private const string SLOT_CONFIGURATION_COLLECTION_NAME = "SlotConfiguration";
        private const string CONNETION_STRIN_ENVIRONMENT_VARIABL_NAME = "MONGO_CONNECTION_STRING";
        private const string DATABASE_NAME = "SlotMachineDB";

        private readonly IMongoDatabase _database;

        private IMongoCollection<PlayerBalance> Players => _database.GetCollection<PlayerBalance>(PLAYER_BALANCE_COLLECTION_NAME);
        private IMongoCollection<SlotConfiguration> Configs => _database.GetCollection<SlotConfiguration>(SLOT_CONFIGURATION_COLLECTION_NAME);


        public MongoDbContext()
        {
            var connectionString = Environment.GetEnvironmentVariable(CONNETION_STRIN_ENVIRONMENT_VARIABL_NAME) ?? "mongodb://root:example@localhost:27017";
            
            // Connection time out should not be 30 seconds 
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5); //5 seconds timeout

            var client = new MongoClient(settings);
            _database = client.GetDatabase(DATABASE_NAME);

            TestDatabaseConnection();
        }

        public async Task<SlotConfiguration> GetSlotMachineConfigurationByIdAsync(string slotMachineId)
        {
            var filter = Builders<SlotConfiguration>.Filter.Eq(p => p.Id, slotMachineId);
            return await Configs.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<PlayerBalance> GetPlayerBalanceByIdAsync(string playerId)
        {
            var filter = Builders<PlayerBalance>.Filter.Eq(p => p.Id, playerId);
            return await Players.Find(filter).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Update the player balance
        /// </summary>        
        public async Task UpdatePlayerBalanceAsync(PlayerBalance player)
        {
            var filter = Builders<PlayerBalance>.Filter.Eq(p => p.Id, player.Id);
            await Players.ReplaceOneAsync(filter, player);
        }

        /// <summary>
        /// Create new player balance
        /// </summary>
        public async Task CreatePlayerBalanceAsync(PlayerBalance player)
        {
            await Players.InsertOneAsync(player);
        }

        /// <summary>
        /// Update the Slot Machine Matrix Size configuration
        /// </summary>  
        internal async Task UpdateSlotMachineMatrixSize(SlotConfiguration slot)
        {
            var filter = Builders<SlotConfiguration>.Filter.Eq(p => p.Id, slot.Id);
            await Configs.ReplaceOneAsync(filter, slot);
        }

        /// <summary>
        /// Create new Slot Machine Matrix Size configuration
        /// </summary>
        internal async Task CreateSlotMachineMatrixSizeAsync(SlotConfiguration newConfiguration)
        {
            await Configs.InsertOneAsync(newConfiguration);
        }


        private void TestDatabaseConnection()
        {
            try
            {
                _database.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait();                
            }
            catch (Exception ex)
            {
                var error = $"Unable to connect to the MongoDB server, Make sure the MongoDB Docker image is up and runing: {ex.Message}";
                Console.WriteLine(error);
                throw new Exception(error, ex);
            }   
        }
    }
}
