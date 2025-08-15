using MongoDB.Bson;
using Simplified_Threat_Intelligence_Platform.Dtos;

namespace Simplified_Threat_Intelligence_Platform.Repositories
{
    public interface IIndicatorRepository
    {
        Task<List<string>> EnsureIndicatorsAsync(IEnumerable<IndicatorInputDto> indicators);
        Task<List<BsonDocument>> FindByValuesAsync(IEnumerable<string> values);
    }
}