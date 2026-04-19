namespace ReactiveMcp.Core.Models;

/// <summary>
/// Represents a modernization or upgrade plan for a reactive codebase.
/// </summary>
public sealed record UpgradePlanResult(
    string Summary,
    IReadOnlyList<string> PackageActions,
    IReadOnlyList<string> CodeActions,
    IReadOnlyList<string> ValidationSteps,
    IReadOnlyList<string> Risks,
    IReadOnlyList<string> RecommendedManifestIds);
