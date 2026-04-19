namespace ReactiveMcp.Core.Models;

/// <summary>
/// Describes an upgrade or modernization scenario for a reactive codebase.
/// </summary>
public sealed record UpgradePlanRequest(
    string? Stack,
    string? ProjectType,
    IReadOnlyList<string> CurrentPackages,
    IReadOnlyList<string> UpgradeGoals,
    IReadOnlyList<string> Constraints)
{
    /// <summary>
    /// Creates a request from comma-separated values.
    /// </summary>
    /// <param name="stack">Optional stack.</param>
    /// <param name="projectType">Optional project type.</param>
    /// <param name="currentPackages">Current packages.</param>
    /// <param name="upgradeGoals">Upgrade goals.</param>
    /// <param name="constraints">Constraints.</param>
    /// <returns>A parsed request.</returns>
    public static UpgradePlanRequest FromStrings(
        string? stack,
        string? projectType,
        string? currentPackages,
        string? upgradeGoals,
        string? constraints) =>
        new(
            stack,
            projectType,
            Split(currentPackages),
            Split(upgradeGoals),
            Split(constraints));

    private static IReadOnlyList<string> Split(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split([',', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
