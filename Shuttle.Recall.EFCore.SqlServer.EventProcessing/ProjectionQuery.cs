using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;
using Shuttle.Extensions.EFCore;

namespace Shuttle.Recall.EFCore.SqlServer.EventProcessing;

public class ProjectionQuery : IProjectionQuery
{
    private readonly IDbContextService _dbContextService;

    public async Task SetSequenceNumberAsync(string projectionName, long sequenceNumber)
    {
        var model = await _dbContextService.Get<EventProcessingDbContext>().Projections.FirstOrDefaultAsync(item => item.Name == projectionName);

        if (model == null)
        {
            throw new InvalidOperationException(string.Format(Resources.ProjectionNotFoundException, projectionName));
        }

        model.SequenceNumber = sequenceNumber;
    }

    public ProjectionQuery(IDbContextService dbContextService)
    {
        _dbContextService = Guard.AgainstNull(dbContextService);
    }

    public async ValueTask<long> GetSequenceNumberAsync(string projectionName)
    {
        var model = await _dbContextService.Get<EventProcessingDbContext>().Projections.FirstOrDefaultAsync(item => item.Name == projectionName);

        if (model == null)
        {
            model = new()
            {
                Id = Guid.NewGuid(),
                Name = projectionName,
                SequenceNumber = 0
            };

            _dbContextService.Get<EventProcessingDbContext>().Projections.Add(model);
        }

        return model.SequenceNumber;
    }
}