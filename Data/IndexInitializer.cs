using MongoDB.Bson;
using MongoDB.Driver;

namespace Simplified_Threat_Intelligence_Platform.Data
{
    public static class IndexInitializer
    {
        public static async Task EnsureAsync(IMongoDatabase db)
        {
            var malware = db.GetCollection<BsonDocument>("malware");
            var indicators = db.GetCollection<BsonDocument>("indicators");

            // updatedDate index
            await malware.Indexes.CreateOneAsync(
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Descending("updatedDate")));

            // indicatorIds multikey
            await malware.Indexes.CreateOneAsync(
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending("indicatorIds")));

            // indicators: value unique
            await indicators.Indexes.CreateOneAsync(
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending("value"),
                    new CreateIndexOptions { Unique = true }));

            await indicators.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("valueLower"),
                new CreateIndexOptions { Name = "ix_indicator_valueLower" }
            ));

        }
    }
}
