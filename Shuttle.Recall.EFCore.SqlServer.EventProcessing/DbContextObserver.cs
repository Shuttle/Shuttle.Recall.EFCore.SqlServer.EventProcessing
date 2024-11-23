using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;
using Shuttle.Extensions.EFCore;
using Shuttle.Recall.EFCore.SqlServer.Storage;

namespace Shuttle.Recall.EFCore.SqlServer.EventProcessing;

public class DbContextObserver :
    IPipelineObserver<OnStageStarting>,
    IPipelineObserver<OnStageCompleted>,
    IPipelineObserver<OnPipelineException>,
    IPipelineObserver<OnAbortPipeline>
{
    private readonly IDbContextFactory<StorageDbContext> _storageDbContextFactory;
    private readonly IDbContextService _dbContextService;
    private readonly IDbContextFactory<EventProcessingDbContext> _eventProcessingDbContextFactory;

    private const string DisposableStateKey = "Shuttle.Recall.EFCore.SqlServer.EventProcessing.DbContextObserver:Disposable";

    public DbContextObserver(IDbContextService dbContextService, IDbContextFactory<StorageDbContext> storageDbContextFactory, IDbContextFactory<EventProcessingDbContext> eventProcessingDbContextFactory)
    {
        _dbContextService = Guard.AgainstNull(dbContextService);
        _storageDbContextFactory = Guard.AgainstNull(storageDbContextFactory);
        _eventProcessingDbContextFactory = Guard.AgainstNull(eventProcessingDbContextFactory);
    }

    public async Task ExecuteAsync(IPipelineContext<OnAbortPipeline> pipelineContext)
    {
        await DisposeDbContextAsync(Guard.AgainstNull(pipelineContext));
    }

    public async Task ExecuteAsync(IPipelineContext<OnPipelineException> pipelineContext)
    {
        await DisposeDbContextAsync(Guard.AgainstNull(pipelineContext));
    }

    public async Task ExecuteAsync(IPipelineContext<OnStageCompleted> pipelineContext)
    {
        await DisposeDbContextAsync(Guard.AgainstNull(pipelineContext));
    }

    public async Task ExecuteAsync(IPipelineContext<OnStageStarting> pipelineContext)
    {
        switch (Guard.AgainstNull(pipelineContext).Pipeline.StageName.ToUpperInvariant())
        {
            case "EVENTPROCESSING.READ":
            {
                if (!_dbContextService.Contains<StorageDbContext>())
                {
                    pipelineContext.Pipeline.State.Add(DisposableStateKey, _dbContextService.Add(await _storageDbContextFactory.CreateDbContextAsync()));
                }

                break;
            }
            case "EVENTPROCESSING.HANDLE":
            {
                if (!_dbContextService.Contains<EventProcessingDbContext>())
                {
                    pipelineContext.Pipeline.State.Add(DisposableStateKey, _dbContextService.Add(await _eventProcessingDbContextFactory.CreateDbContextAsync()));
                }


                break;
            }
        }

        await Task.CompletedTask;
    }

    private async Task DisposeDbContextAsync(IPipelineContext pipelineContext)
    {
        var disposable = Guard.AgainstNull(pipelineContext).Pipeline.State.Get(DisposableStateKey);

        if (disposable != null)
        {
            await disposable.TryDisposeAsync();
        }

        pipelineContext.Pipeline.State.Remove(DisposableStateKey);
    }
}