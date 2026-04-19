using ReactiveMcp.Core.Models;

namespace ReactiveMcp.Core.Abstractions;

/// <summary>
/// Produces recommendations, reviews, debugging guidance, and upgrade plans for reactive .NET applications.
/// </summary>
public interface IReactiveGuidanceService
{
    /// <summary>
    /// Produces a recommendation for a requested reactive application scenario.
    /// </summary>
    /// <param name="request">The recommendation request.</param>
    /// <returns>A recommendation result.</returns>
    RecommendationResult Recommend(RecommendationRequest request);

    /// <summary>
    /// Reviews a reactive plan for likely architecture and testing mistakes.
    /// </summary>
    /// <param name="stack">Optional stack or platform context.</param>
    /// <param name="libraries">Optional library list.</param>
    /// <param name="planText">The plan to review.</param>
    /// <returns>A review result.</returns>
    ReviewResult ReviewPlan(string? stack, string? libraries, string planText);

    /// <summary>
    /// Reviews a real C# snippet with optional focus guidance.
    /// </summary>
    /// <param name="stack">Optional stack or platform context.</param>
    /// <param name="libraries">Optional library list.</param>
    /// <param name="reviewFocus">Optional review focus such as disposal, schedulers, or testing.</param>
    /// <param name="code">The C# snippet to review.</param>
    /// <returns>A review result.</returns>
    ReviewResult ReviewCSharpSnippet(string? stack, string? libraries, string? reviewFocus, string code);

    /// <summary>
    /// Compares two catalog entries side by side.
    /// </summary>
    /// <param name="leftId">The left manifest identifier.</param>
    /// <param name="rightId">The right manifest identifier.</param>
    /// <returns>A comparison result.</returns>
    ComparisonResult Compare(string leftId, string rightId);

    /// <summary>
    /// Creates a debugging checklist for a reactive issue.
    /// </summary>
    /// <param name="issue">Short issue summary.</param>
    /// <param name="symptoms">Observed symptoms.</param>
    /// <param name="currentApproach">Current implementation approach.</param>
    /// <returns>A debugging checklist.</returns>
    DebugChecklistResult CreateDebugChecklist(string? issue, string? symptoms, string? currentApproach);

    /// <summary>
    /// Creates a modernization or upgrade plan.
    /// </summary>
    /// <param name="request">The upgrade request.</param>
    /// <returns>An upgrade plan.</returns>
    UpgradePlanResult CreateUpgradePlan(UpgradePlanRequest request);

    /// <summary>
    /// Creates a reusable prompt for generating reactive code.
    /// </summary>
    /// <param name="request">The recommendation request.</param>
    /// <returns>A prompt string.</returns>
    string CreateScaffoldPrompt(RecommendationRequest request);
}
