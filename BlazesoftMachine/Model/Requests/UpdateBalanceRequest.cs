using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BlazesoftMachine.Model.Requests
{
    public class UpdateBalanceRequest
    {
        public string PlayerId { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Amount { get; set; }
    }
}
