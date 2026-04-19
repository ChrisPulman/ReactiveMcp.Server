using ModelContextProtocol.Server;
using ReactiveMcp.Core.Abstractions;
using ReactiveMcp.Core.Models;
using ReactiveMcp.Server.Serialization;
using System.ComponentModel;

namespace ReactiveMcp.Server.Tools;

/// <summary>
/// MCP tools for producing reactive application guidance.
/// </summary>
[McpServerToolType]
public sealed class GuidanceTools
{
    /// <summary>
    /// Produces package and pattern recommendations for a reactive scenario.
    /// </summary>
    /// <param name="guidanceService">The guidance service.</param>
    /// <param name="stack">The target stack or platform.</param>
    /// <param name="appKind">The application kind.</param>
    /// <param name="features">Desired features.</param>
    /// <param name="constraints">Constraints.</param>
    /// <param name="existingLibraries">Existing libraries.</param>
    /// <returns>A recommendation payload.</returns>
    [McpServerTool(Name = "reactive_recommend"), Description("Recommend reactive packages, abstractions, and patterns for a requested scenario.")]
    public static string Recommend(
        IReactiveGuidanceService guidanceService,
        [Description("Target stack such as ASP.NET Core, worker service, desktop app, library, or test project.")] string? stack = null,
        [Description("Application kind such as backend service, desktop app, integration layer, or test project.")] string? appKind = null,
        [Description("Comma-separated features such as timers, retry, async enumerable, virtual time tests, or multicast.")] string? features = null,
        [Description("Comma-separated constraints such as performance, determinism, low allocation, or backward compatibility.")] string? constraints = null,
        [Description("Comma-separated existing libraries already in use.")] string? existingLibraries = null)
    {
        ArgumentNullException.ThrowIfNull(guidanceService);

        var request = RecommendationRequest.FromStrings(stack, appKind, features, constraints, existingLibraries);
        return JsonOutput.Serialize(guidanceService.Recommend(request));
    }

    /// <summary>
    /// Reviews a plan for reactive anti-patterns.
    /// </summary>
    /// <param name="guidanceService">The guidance service.</param>
    /// <param name="planText">The plan text.</param>
    /// <param name="stack">Optional stack.</param>
    /// <param name="libraries">Optional libraries.</param>
    /// <returns>A review payload.</returns>
    [McpServerTool(Name = "reactive_review_plan"), Description("Review a reactive implementation plan for likely testing, disposal, subject, and scheduler mistakes.")]
    public static string ReviewPlan(
        IReactiveGuidanceService guidanceService,
        [Description("The plan or code-generation instruction text to review.")] string planText,
        [Description("Optional stack context.")] string? stack = null,
        [Description("Optional comma-separated library list.")] string? libraries = null)
    {
        ArgumentNullException.ThrowIfNull(guidanceService);

        return JsonOutput.Serialize(guidanceService.ReviewPlan(stack, libraries, planText));
    }

    /// <summary>
    /// Reviews a real C# snippet for reactive issues and best-practice replacements.
    /// </summary>
    /// <param name="guidanceService">The guidance service.</param>
    /// <param name="code">The C# snippet to review.</param>
    /// <param name="stack">Optional stack context.</param>
    /// <param name="libraries">Optional libraries.</param>
    /// <param name="reviewFocus">Optional focus such as disposal, schedulers, subjects, or testing.</param>
    /// <returns>A review payload.</returns>
    [McpServerTool(Name = "reactive_review_csharp"), Description("Review a real C# snippet for reactive anti-patterns and return best-practice replacement snippets.")]
    public static string ReviewCSharpSnippet(
        IReactiveGuidanceService guidanceService,
        [Description("The C# snippet to review.")] string code,
        [Description("Optional stack context.")] string? stack = null,
        [Description("Optional comma-separated library list.")] string? libraries = null,
        [Description("Optional review focus such as disposal, schedulers, subjects, testing, or async.")] string? reviewFocus = null)
    {
        ArgumentNullException.ThrowIfNull(guidanceService);

        return JsonOutput.Serialize(guidanceService.ReviewCSharpSnippet(stack, libraries, reviewFocus, code));
    }

    /// <summary>
    /// Compares two reactive ecosystem areas.
    /// </summary>
    /// <param name="guidanceService">The guidance service.</param>
    /// <param name="leftId">The left manifest id.</param>
    /// <param name="rightId">The right manifest id.</param>
    /// <returns>A comparison payload.</returns>
    [McpServerTool(Name = "reactive_compare"), Description("Compare two reactive ecosystem areas side by side.")]
    public static string Compare(
        IReactiveGuidanceService guidanceService,
        [Description("The left manifest id.")] string leftId,
        [Description("The right manifest id.")] string rightId)
    {
        ArgumentNullException.ThrowIfNull(guidanceService);

        return JsonOutput.Serialize(guidanceService.Compare(leftId, rightId));
    }

    /// <summary>
    /// Produces a debugging checklist for a reactive issue.
    /// </summary>
    /// <param name="guidanceService">The guidance service.</param>
    /// <param name="issue">Short issue summary.</param>
    /// <param name="symptoms">Observed symptoms.</param>
    /// <param name="currentApproach">Current implementation details.</param>
    /// <returns>A debugging checklist payload.</returns>
    [McpServerTool(Name = "reactive_debug_checklist"), Description("Create a structured debugging checklist for a reactive pipeline, scheduler, or timing issue.")]
    public static string DebugChecklist(
        IReactiveGuidanceService guidanceService,
        [Description("Short description of the issue.")] string? issue = null,
        [Description("Observed symptoms or failures.")] string? symptoms = null,
        [Description("Current implementation approach or suspicious code patterns.")] string? currentApproach = null)
    {
        ArgumentNullException.ThrowIfNull(guidanceService);

        return JsonOutput.Serialize(guidanceService.CreateDebugChecklist(issue, symptoms, currentApproach));
    }

    /// <summary>
    /// Produces a modernization plan for a reactive codebase.
    /// </summary>
    /// <param name="guidanceService">The guidance service.</param>
    /// <param name="stack">Optional stack.</param>
    /// <param name="projectType">Optional project type.</param>
    /// <param name="currentPackages">Current packages or patterns.</param>
    /// <param name="upgradeGoals">Upgrade goals.</param>
    /// <param name="constraints">Constraints.</param>
    /// <returns>An upgrade plan payload.</returns>
    [McpServerTool(Name = "reactive_upgrade_plan"), Description("Create a modernization plan for an existing reactive codebase.")]
    public static string UpgradePlan(
        IReactiveGuidanceService guidanceService,
        [Description("Optional stack such as worker service, desktop app, or shared library.")] string? stack = null,
        [Description("Optional project type.")] string? projectType = null,
        [Description("Comma-separated current packages or patterns such as System.Reactive, Subject<T>, Thread.Sleep tests.")] string? currentPackages = null,
        [Description("Comma-separated upgrade goals such as deterministic tests, fewer subjects, or async enumerable integration.")] string? upgradeGoals = null,
        [Description("Comma-separated constraints.")] string? constraints = null)
    {
        ArgumentNullException.ThrowIfNull(guidanceService);

        var request = UpgradePlanRequest.FromStrings(stack, projectType, currentPackages, upgradeGoals, constraints);
        return JsonOutput.Serialize(guidanceService.CreateUpgradePlan(request));
    }

    /// <summary>
    /// Creates a reusable prompt for another AI coding agent.
    /// </summary>
    /// <param name="guidanceService">The guidance service.</param>
    /// <param name="stack">Target stack.</param>
    /// <param name="appKind">Application kind.</param>
    /// <param name="features">Desired features.</param>
    /// <param name="constraints">Constraints.</param>
    /// <param name="existingLibraries">Existing libraries.</param>
    /// <returns>A scaffold prompt string.</returns>
    [McpServerTool(Name = "reactive_scaffold_prompt"), Description("Create a high-quality prompt for generating reactive .NET code that follows current ecosystem guidance.")]
    public static string CreateScaffoldPrompt(
        IReactiveGuidanceService guidanceService,
        [Description("Target stack such as ASP.NET Core, worker service, desktop app, library, or test project.")] string? stack = null,
        [Description("Application kind such as backend service, integration layer, or test project.")] string? appKind = null,
        [Description("Comma-separated desired features.")] string? features = null,
        [Description("Comma-separated constraints.")] string? constraints = null,
        [Description("Comma-separated existing libraries.")] string? existingLibraries = null)
    {
        ArgumentNullException.ThrowIfNull(guidanceService);

        var request = RecommendationRequest.FromStrings(stack, appKind, features, constraints, existingLibraries);
        return guidanceService.CreateScaffoldPrompt(request);
    }
}
