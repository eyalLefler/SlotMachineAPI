using BlazesoftMachine.Model;
using MongoDB.Driver;



namespace BlazesoftMachine.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<PlayerBalance> Players => _database.GetCollection<PlayerBalance>("PlayerBalance");
        public IMongoCollection<SlotConfiguration> Configs => _database.GetCollection<SlotConfiguration>("SlotConfiguration");
    }
}
