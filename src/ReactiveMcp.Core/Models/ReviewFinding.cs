namespace ReactiveMcp.Core.Models;

/// <summary>
/// Represents one finding produced while reviewing a reactive plan.
/// </summary>
/// <param name="Severity">The severity level.</param>
/// <param name="Code">The stable finding code.</param>
/// <param name="Message">The diagnostic message.</param>
/// <param name="Recommendation">The remediation guidance.</param>
/// <param name="WhyItMatters">Why the issue is important in reactive code.</param>
/// <param name="DetectedPattern">The approximate pattern detected in the submitted plan or snippet.</param>
/// <param name="BestPracticeSnippet">A best-practice example snippet that shows the preferred approach.</param>
/// <param name="StartLine">Approximate 1-based start line for the detected pattern.</param>
/// <param name="EndLine">Approximate 1-based end line for the detected pattern.</param>
/// <param name="StartColumn">Approximate 1-based start column for the detected pattern.</param>
/// <param name="EndColumn">Approximate 1-based end column for the detected pattern.</param>
public sealed record ReviewFinding(
    string Severity,
    string Code,
    string Message,
    string Recommendation,
    string? WhyItMatters = null,
    string? DetectedPattern = null,
    string? BestPracticeSnippet = null,
    int? StartLine = null,
    int? EndLine = null,
    int? StartColumn = null,
    int? EndColumn = null);
