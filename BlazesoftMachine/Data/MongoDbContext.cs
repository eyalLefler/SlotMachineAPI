using BlazesoftMachine.Model;
using MongoDB.Driver;



namespace BlazesoftMachine.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext()
        {
            var connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING") ?? "mongodb://localhost:27017";
            var databaseName = "SlotMachineDB";

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<PlayerBalance> Players => _database.GetCollection<PlayerBalance>("PlayerBalance");
        public IMongoCollection<SlotConfiguration> Configs => _database.GetCollection<SlotConfiguration>("SlotConfiguration");
    }
}
