using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;
using Shuttle.Core.Threading;
using Shuttle.Extensions.EFCore;
using Shuttle.Recall.EFCore.SqlServer.Storage;

namespace Shuttle.Recall.EFCore.SqlServer.EventProcessing;

public class ProjectionService : IProjectionService
{
    private readonly IDbContextFactory<EventProcessingDbContext> _eventProcessingDbContextFactory;
    private readonly IDbContextService _dbContextService;
    private readonly IDbContextFactory<StorageDbContext> _storageSbContextFactory;
    private readonly IEventProcessorConfiguration _eventProcessorConfiguration;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IPrimitiveEventQuery _primitiveEventQuery;
    private readonly IProjectionQuery _projectionQuery;
    private readonly Queue<Projection> _projections = new();

    public ProjectionService(IDbContextService dbContextService, IDbContextFactory<StorageDbContext> storageDbContextFactory, IDbContextFactory<EventProcessingDbContext> eventProcessingDbContextFactory, IProjectionQuery projectionQuery, IPrimitiveEventQuery primitiveEventQuery, IEventProcessorConfiguration eventProcessorConfiguration)
    {
        _dbContextService = Guard.AgainstNull(dbContextService);
        _storageSbContextFactory = Guard.AgainstNull(storageDbContextFactory);
        _eventProcessingDbContextFactory = Guard.AgainstNull(eventProcessingDbContextFactory);
        _projectionQuery = Guard.AgainstNull(projectionQuery);
        _primitiveEventQuery = Guard.AgainstNull(primitiveEventQuery);
        _eventProcessorConfiguration = Guard.AgainstNull(eventProcessorConfiguration);
    }

    public async Task<ProjectionEvent?> GetProjectionEventAsync()
    {
        Projection? projection = null;

        await _lock.WaitAsync();

        try
        {
            if (_projections.Count == 0)
            {
                return null;
            }

            projection = _projections.Dequeue();
        }
        finally
        {
            _lock.Release();
        }

        try
        {
            var projectionConfiguration = _eventProcessorConfiguration.GetProjection(projection.Name);

            var specification = new Storage.Models.PrimitiveEvent.Specification()
                .AddEventTypes(projectionConfiguration.EventTypes)
                .WithSequenceNumberStart(projection.SequenceNumber + 1)
                .WithMaximumRows(1);

            var primitiveEventModels = await _primitiveEventQuery.SearchAsync(specification);

            if (!primitiveEventModels.Any())
            {
                await GetJournal();

                primitiveEventModels = await _primitiveEventQuery.SearchAsync(specification);
            }

            if (!primitiveEventModels.Any())
            {
                return null;
            }

            var primitiveEventModel = primitiveEventModels.FirstOrDefault();

            return primitiveEventModel == null ? null : new ProjectionEvent(projection, new PrimitiveEvent()
            {
                Id = primitiveEventModel.Id,
                Version = primitiveEventModel.Version,
                EventId = primitiveEventModel.EventId,
                EventType = primitiveEventModel.EventType.TypeName,
                SequenceNumber = primitiveEventModel.SequenceNumber,
                CorrelationId = primitiveEventModel.CorrelationId,
                DateRegistered = primitiveEventModel.DateRegistered,
                EventEnvelope = primitiveEventModel.EventEnvelope
            });
        }
        finally
        {
            _projections.Enqueue(projection);
        }
    }

    public async Task SetSequenceNumberAsync(string projectionName, long sequenceNumber)
    {
        await _projectionQuery.SetSequenceNumberAsync(projectionName, sequenceNumber);

        await _dbContextService.Get<EventProcessingDbContext>().SaveChangesAsync();
    }

    private async Task GetJournal()
    {
        await Task.CompletedTask;
    }

    public async Task StartupAsync(IProcessorThreadPool processorThreadPool)
    {
        Guard.AgainstNull(processorThreadPool);

        await using var dbContext = await _eventProcessingDbContextFactory.CreateDbContextAsync();
        using (_dbContextService.Add(dbContext))
        {
            foreach (var projectionConfiguration in _eventProcessorConfiguration.Projections)
            {
                _projections.Enqueue(new(projectionConfiguration.Name, await _projectionQuery.GetSequenceNumberAsync(projectionConfiguration.Name)));
            }

            await dbContext.SaveChangesAsync();

            //  assign thread id to each existing journal entry
        }

        await Task.CompletedTask;
    }
}