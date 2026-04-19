namespace ReactiveMcp.Core.Models;

/// <summary>
/// Represents a recommendation outcome for a reactive application scenario.
/// </summary>
public sealed record RecommendationResult(
    string Summary,
    IReadOnlyList<string> SelectedManifestIds,
    IReadOnlyList<string> SuggestedPackages,
    IReadOnlyList<string> RecommendedPatterns,
    IReadOnlyList<string> AvoidPatterns,
    IReadOnlyList<string> SetupSteps,
    IReadOnlyList<string> CommonPitfalls,
    IReadOnlyList<string> RelatedLibraries);
