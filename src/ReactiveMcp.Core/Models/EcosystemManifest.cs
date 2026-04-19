namespace ReactiveMcp.Core.Models;

/// <summary>
/// Represents one area of the Reactive .NET ecosystem knowledge base.
/// </summary>
public sealed record EcosystemManifest(
    string Id,
    string DisplayName,
    string RepositoryUrl,
    string Category,
    string Summary,
    IReadOnlyList<string> SupportedTargets,
    IReadOnlyList<string> NuGetPackages,
    IReadOnlyList<string> RecommendedPatterns,
    IReadOnlyList<string> AvoidPatterns,
    IReadOnlyList<string> SetupSteps,
    IReadOnlyList<string> CommonPitfalls,
    IReadOnlyList<string> RelatedLibraries,
    IReadOnlyList<string> Keywords,
    IReadOnlyList<SourceReference> Sources,
    HarvestMetadata Harvest);
