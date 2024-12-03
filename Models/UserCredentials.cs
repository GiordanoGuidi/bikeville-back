using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BikeVille.Models
{
    public class UserCredentials
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int CustomerID { get; set; }
        public string EmailAddress { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
    }
}
