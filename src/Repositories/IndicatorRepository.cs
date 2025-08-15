using MongoDB.Bson;
using MongoDB.Driver;
using Simplified_Threat_Intelligence_Platform.Dtos;

namespace Simplified_Threat_Intelligence_Platform.Repositories
{
    public class IndicatorRepository : IIndicatorRepository
    {
        private readonly IMongoCollection<BsonDocument> _col;
        public IndicatorRepository(IMongoDatabase db) => _col = db.GetCollection<BsonDocument>("indicators");

        public async Task<List<string>> EnsureIndicatorsAsync(IEnumerable<IndicatorInputDto> indicators)
        {
            var ids = new List<string>();

            var cleaned = indicators
                .Select(i => new {
                    Type = (i.Type ?? "").Trim().ToLowerInvariant(),
                    Value = (i.Value ?? "").Trim(),
                    ValueLower = (i.Value ?? "").Trim().ToLowerInvariant(),
                    i.CreatedDate,
                    i.UpdatedDate,
                    i.ExpirationDate
                })
                .Where(i => !string.IsNullOrWhiteSpace(i.Value));

            foreach (var i in cleaned)
            {
                var filter = Builders<BsonDocument>.Filter.Eq("value", i.Value);
                var existing = await _col.Find(filter).FirstOrDefaultAsync();

                if (existing is null)
                {
                    var doc = new BsonDocument {
                { "value", i.Value },
                { "valueLower", i.ValueLower },       
                { "type", i.Type },
                { "createdDate", i.CreatedDate },
                { "updatedDate", i.UpdatedDate },
                { "expirationDate", i.ExpirationDate.HasValue ? (BsonValue)i.ExpirationDate.Value : BsonNull.Value }
            };

                    await _col.InsertOneAsync(doc);
                    ids.Add(doc["_id"].ToString());
                }
                else
                {
                    var update = Builders<BsonDocument>.Update
                        .Set("updatedDate", i.UpdatedDate)
                        .Set("valueLower", i.ValueLower);      

                    if (i.ExpirationDate.HasValue)
                        update = update.Set("expirationDate", i.ExpirationDate.Value);

                    var res = await _col.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<BsonDocument>
                    {
                        ReturnDocument = ReturnDocument.After
                    });

                    ids.Add(res!["_id"].ToString());
                }
            }
            return ids;
        }

        public Task<List<BsonDocument>> FindByValuesAsync(IEnumerable<string> values)
        {
            var input = values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim().ToLowerInvariant())
                .ToList();

            var filter = Builders<BsonDocument>.Filter.In("valueLower", input);
            return _col.Find(filter).ToListAsync();
        }


    }
}
