namespace ReactiveMcp.Core.Models;

/// <summary>
/// Describes when and how a catalog entry was curated.
/// </summary>
/// <param name="HarvestedAtUtc">The curation timestamp.</param>
/// <param name="SourceSummary">Short summary of the source basis.</param>
/// <param name="Notes">Optional notes.</param>
public sealed record HarvestMetadata(DateTimeOffset HarvestedAtUtc, string SourceSummary, string? Notes);
