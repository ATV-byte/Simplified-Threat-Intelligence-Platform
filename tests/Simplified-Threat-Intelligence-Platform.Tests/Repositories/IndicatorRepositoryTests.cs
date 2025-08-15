using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Simplified_Threat_Intelligence_Platform.Dtos;
using Simplified_Threat_Intelligence_Platform.Repositories;
using Xunit;

namespace Simplified_Threat_Intelligence_Platform.Tests.Repositories;

public class IndicatorRepositoryTests
{
    [Fact]
    public async Task EnsureIndicatorsAsync_NoValidIndicators_PerformsNoDatabaseOperations()
    {
        var collectionMock = new Mock<IMongoCollection<BsonDocument>>();
        var dbMock = new Mock<IMongoDatabase>();
        dbMock.Setup(d => d.GetCollection<BsonDocument>("indicators", null))
              .Returns(collectionMock.Object);

        var repo = new IndicatorRepository(dbMock.Object);

        var input = new[]
        {
            new IndicatorInputDto { Type = "ip", Value = "" },
            new IndicatorInputDto { Type = "domain", Value = "  " }
        };

        var ids = await repo.EnsureIndicatorsAsync(input);

        Assert.Empty(ids);
        collectionMock.Verify(c => c.InsertOneAsync(It.IsAny<BsonDocument>(), null, default), Times.Never);
        collectionMock.Verify(c => c.FindOneAndUpdateAsync(
                It.IsAny<FilterDefinition<BsonDocument>>(),
                It.IsAny<UpdateDefinition<BsonDocument>>(),
                It.IsAny<FindOneAndUpdateOptions<BsonDocument>>(),
                default), Times.Never);
    }
}