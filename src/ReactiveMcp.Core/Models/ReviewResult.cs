namespace ReactiveMcp.Core.Models;

/// <summary>
/// Represents the result of reviewing a reactive implementation plan.
/// </summary>
/// <param name="Summary">The overall review summary.</param>
/// <param name="Findings">The detailed findings.</param>
/// <param name="RecommendedManifestIds">Manifest ids relevant to the review.</param>
/// <param name="BestPracticeSnippets">Aggregated best-practice snippets referenced by the findings.</param>
public sealed record ReviewResult(
    string Summary,
    IReadOnlyList<ReviewFinding> Findings,
    IReadOnlyList<string> RecommendedManifestIds,
    IReadOnlyList<string> BestPracticeSnippets);
