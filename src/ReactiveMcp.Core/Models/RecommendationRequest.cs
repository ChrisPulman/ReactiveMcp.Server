namespace ReactiveMcp.Core.Models;

/// <summary>
/// Describes a reactive application scenario to recommend guidance for.
/// </summary>
public sealed record RecommendationRequest(
    string? Stack,
    string? AppKind,
    IReadOnlyList<string> Features,
    IReadOnlyList<string> Constraints,
    IReadOnlyList<string> ExistingLibraries)
{
    /// <summary>
    /// Creates a request from comma-separated string values.
    /// </summary>
    /// <param name="stack">Optional stack or platform.</param>
    /// <param name="appKind">Optional application kind.</param>
    /// <param name="features">Optional comma-separated features.</param>
    /// <param name="constraints">Optional comma-separated constraints.</param>
    /// <param name="existingLibraries">Optional comma-separated library names.</param>
    /// <returns>A parsed request.</returns>
    public static RecommendationRequest FromStrings(
        string? stack,
        string? appKind,
        string? features,
        string? constraints,
        string? existingLibraries) =>
        new(
            stack,
            appKind,
            Split(features),
            Split(constraints),
            Split(existingLibraries));

    private static IReadOnlyList<string> Split(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split([',', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
