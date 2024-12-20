﻿using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall.EFCore.SqlServer.EventProcessing;

public class EventProcessingStartupObserver : IPipelineObserver<OnAfterConfigureThreadPools>
{
    private readonly IProjectionService _projectionService;

    public EventProcessingStartupObserver(IProjectionService projectionService)
    {
        _projectionService = Guard.AgainstNull(projectionService);
    }

    public async Task ExecuteAsync(IPipelineContext<OnAfterConfigureThreadPools> pipelineContext)
    {
        if (_projectionService is not ProjectionService service)
        {
            throw new InvalidOperationException(string.Format(Resources.ProjectionServiceTypeException, typeof(ProjectionService).FullName, _projectionService.GetType().FullName));
        }

        await service.StartupAsync(Guard.AgainstNull(Guard.AgainstNull(pipelineContext).Pipeline.State.Get<IProcessorThreadPool>("EventProcessorThreadPool")));
    }
}