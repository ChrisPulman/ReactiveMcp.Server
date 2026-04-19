namespace ReactiveMcp.Core.Models;

/// <summary>
/// Represents a side-by-side comparison of two reactive ecosystem areas.
/// </summary>
public sealed record ComparisonResult(
    string Summary,
    string LeftId,
    string RightId,
    IReadOnlyList<string> LeftPackages,
    IReadOnlyList<string> RightPackages,
    IReadOnlyList<string> LeftPatterns,
    IReadOnlyList<string> RightPatterns,
    IReadOnlyList<string> Tradeoffs);
