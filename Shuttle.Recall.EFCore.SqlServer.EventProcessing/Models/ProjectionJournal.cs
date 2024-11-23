using Microsoft.EntityFrameworkCore;

namespace Shuttle.Recall.EFCore.SqlServer.EventProcessing.Models;

[PrimaryKey(nameof(ProjectionId), nameof(SequenceNumber))]
public class ProjectionJournal
{
    public Guid ProjectionId { get; set; }
    public long SequenceNumber { get; set; }
    public Guid CorrelationId { get; set; }
    public int ManagedThreadId { get; set; }
}