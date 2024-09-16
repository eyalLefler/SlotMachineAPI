using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace BlazesoftMachine.Model.Requests
{
    public class SpinResponse
    {
        [JsonPropertyName("matrix")]
        public int[][] Matrix { get; set; }

        [JsonPropertyName("winAmount")]
        public decimal WinAmount { get; set; }

        [JsonPropertyName("playerBalance")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal PlayerBalance { get; set; }
    }
}
