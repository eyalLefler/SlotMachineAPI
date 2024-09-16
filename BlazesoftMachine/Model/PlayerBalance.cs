using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BlazesoftMachine.Model
{
    public class PlayerBalance
    {
        public string Id { get; set; }
        
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Balance { get; set; }
    }
}
