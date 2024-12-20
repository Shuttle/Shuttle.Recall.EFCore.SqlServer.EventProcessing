﻿using Microsoft.EntityFrameworkCore;
using Shuttle.Core.Contract;
using Shuttle.Extensions.EFCore;
using Shuttle.Recall.EFCore.SqlServer.Storage;

namespace Shuttle.Recall.EFCore.SqlServer.EventProcessing;

public class PrimitiveEventQuery : IPrimitiveEventQuery
{
    private readonly IDbContextService _dbContextService;
    private readonly IEventTypeRepository _eventTypeRepository;

    public PrimitiveEventQuery(IDbContextService dbContextService, IEventTypeRepository eventTypeRepository)
    {
        _dbContextService = Guard.AgainstNull(dbContextService);
        _eventTypeRepository = Guard.AgainstNull(eventTypeRepository);
    }

    public async Task<IEnumerable<Storage.Models.PrimitiveEvent>> SearchAsync(Storage.Models.PrimitiveEvent.Specification specification)
    {
        var dbContext = _dbContextService.Get<StorageDbContext>();

        var eventTypeIds = new List<Guid>();

        foreach (var eventType in specification.EventTypes)
        {
            eventTypeIds.Add(await _eventTypeRepository.GetIdAsync(Guard.AgainstNullOrEmptyString(eventType.FullName)));
        }

        var queryable = dbContext.PrimitiveEvents.Include(item => item.EventType).AsQueryable();

        if (eventTypeIds.Count > 0)
        {
            queryable = queryable.Where(item => eventTypeIds.Contains(item.EventTypeId));
        }

        if (specification.SequenceNumberStart > 0)
        {
            queryable = queryable.Where(item => item.SequenceNumber >= specification.SequenceNumberStart);
        }

        if (specification.MaximumRows > 0)
        {
            queryable = queryable.Take(specification.MaximumRows);
        }
        
        return await queryable.ToListAsync();
    }
}