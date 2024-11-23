using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Shuttle.Recall.EFCore.SqlServer.EventProcessing.Models;

[Index(nameof(Name), IsUnique = true, Name = $"IX_{nameof(Projection)}")]
public class Projection
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(650)]
    public string Name { get; set; } = string.Empty;

    public long SequenceNumber { get; set; }
}