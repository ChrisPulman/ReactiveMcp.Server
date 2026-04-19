namespace ReactiveMcp.Core.Models;

/// <summary>
/// Represents a structured debugging checklist for a reactive issue.
/// </summary>
public sealed record DebugChecklistResult(
    string Summary,
    IReadOnlyList<string> InvestigationSteps,
    IReadOnlyList<string> LikelyCauses,
    IReadOnlyList<string> ValidationSteps,
    IReadOnlyList<string> RecommendedManifestIds);
