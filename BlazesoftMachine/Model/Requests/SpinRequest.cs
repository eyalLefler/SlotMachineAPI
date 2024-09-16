using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BlazesoftMachine.Model.Requests
{
    public class SpinRequest
    {
        public string PlayerId { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal BetAmount { get; set; }
    }
}
