namespace ReactiveMcp.Core.Models;

/// <summary>
/// Represents a source URL used to curate a catalog entry.
/// </summary>
/// <param name="Title">The source title.</param>
/// <param name="Url">The source URL.</param>
/// <param name="Notes">Optional notes about why the source matters.</param>
public sealed record SourceReference(string Title, string Url, string? Notes);
