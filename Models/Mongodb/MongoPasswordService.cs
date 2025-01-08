using MongoDB.Driver;

namespace BikeVille.Models.Mongodb
{
    public class MongoPasswordService
    {
        private readonly IMongoCollection<UserCredentials> _passwordCollection;

        public MongoPasswordService(IMongoDatabase database)
        {
            _passwordCollection = database.GetCollection<UserCredentials>("BikeVille");
        }

        public UserCredentials GetUserByEmail(string emailAddress)
        {
            var filter = Builders<UserCredentials>.Filter.Eq(u => u.EmailAddress, emailAddress);
            return _passwordCollection.Find(filter).FirstOrDefault();
        }

        public bool isAdmin(string emailAddress)
        {
            var filter = Builders<UserCredentials>.Filter.And(
            Builders<UserCredentials>.Filter.Eq(u => u.EmailAddress, emailAddress),
            Builders<UserCredentials>.Filter.Eq(u => u.Role, "Admin")
        );

            var user = _passwordCollection.Find(filter).FirstOrDefault();

            return user != null;
        }

    }
}
