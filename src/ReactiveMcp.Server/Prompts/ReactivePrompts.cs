using ModelContextProtocol.Server;
using ReactiveMcp.Core.Abstractions;
using ReactiveMcp.Core.Models;
using System.ComponentModel;

namespace ReactiveMcp.Server.Prompts;

/// <summary>
/// MCP prompts for reactive application generation and modernization.
/// </summary>
[McpServerPromptType]
public sealed class ReactivePrompts
{
    /// <summary>
    /// Creates a prompt for generating a new reactive application.
    /// </summary>
    /// <param name="guidanceService">The guidance service.</param>
    /// <param name="stack">The target stack.</param>
    /// <param name="appKind">The application kind.</param>
    /// <param name="features">Desired features.</param>
    /// <param name="constraints">Constraints.</param>
    /// <param name="existingLibraries">Existing libraries.</param>
    /// <returns>A prompt string.</returns>
    [McpServerPrompt(Name = "create_reactive_app"), Description("Create a prompt for generating a new reactive .NET application.")]
    public static string CreateReactiveApp(
        IReactiveGuidanceService guidanceService,
        [Description("Target stack such as ASP.NET Core, worker service, or desktop app.")] string? stack = null,
        [Description("Application kind such as backend service, library, or test project.")] string? appKind = null,
        [Description("Comma-separated desired features.")] string? features = null,
        [Description("Comma-separated constraints.")] string? constraints = null,
        [Description("Comma-separated existing libraries.")] string? existingLibraries = null)
    {
        ArgumentNullException.ThrowIfNull(guidanceService);

        var request = RecommendationRequest.FromStrings(stack, appKind, features, constraints, existingLibraries);
        return guidanceService.CreateScaffoldPrompt(request);
    }

    /// <summary>
    /// Creates a prompt for debugging a reactive pipeline.
    /// </summary>
    /// <param name="guidanceService">The guidance service.</param>
    /// <param name="issue">Issue summary.</param>
    /// <param name="symptoms">Observed symptoms.</param>
    /// <param name="currentApproach">Current implementation approach.</param>
    /// <returns>A prompt string.</returns>
    [McpServerPrompt(Name = "debug_reactive_pipeline"), Description("Create a prompt for debugging a reactive pipeline, scheduler issue, or timing bug.")]
    public static string DebugReactivePipeline(
        IReactiveGuidanceService guidanceService,
        [Description("Short description of the issue.")] string? issue = null,
        [Description("Observed symptoms.")] string? symptoms = null,
        [Description("Current implementation approach.")] string? currentApproach = null)
    {
        ArgumentNullException.ThrowIfNull(guidanceService);

        var checklist = guidanceService.CreateDebugChecklist(issue, symptoms, currentApproach);
        return $"Issue: {issue ?? "reactive pipeline issue"}\n\nInvestigation Steps:\n- {string.Join("\n- ", checklist.InvestigationSteps)}\n\nLikely Causes:\n- {string.Join("\n- ", checklist.LikelyCauses)}\n\nValidation Steps:\n- {string.Join("\n- ", checklist.ValidationSteps)}";
    }

    /// <summary>
    /// Creates a prompt for modernizing a reactive codebase.
    /// </summary>
    /// <param name="guidanceService">The guidance service.</param>
    /// <param name="stack">Optional stack.</param>
    /// <param name="projectType">Optional project type.</param>
    /// <param name="currentPackages">Current packages or legacy patterns.</param>
    /// <param name="upgradeGoals">Upgrade goals.</param>
    /// <param name="constraints">Constraints.</param>
    /// <returns>A prompt string.</returns>
    [McpServerPrompt(Name = "modernize_reactive_codebase"), Description("Create a prompt for upgrading and hardening an existing reactive .NET codebase.")]
    public static string ModernizeReactiveCodebase(
        IReactiveGuidanceService guidanceService,
        [Description("Optional stack such as worker service, library, or desktop app.")] string? stack = null,
        [Description("Optional project type.")] string? projectType = null,
        [Description("Comma-separated current packages or legacy patterns.")] string? currentPackages = null,
        [Description("Comma-separated upgrade goals.")] string? upgradeGoals = null,
        [Description("Comma-separated constraints.")] string? constraints = null)
    {
        ArgumentNullException.ThrowIfNull(guidanceService);

        var plan = guidanceService.CreateUpgradePlan(UpgradePlanRequest.FromStrings(stack, projectType, currentPackages, upgradeGoals, constraints));
        return $"Summary: {plan.Summary}\n\nPackage Actions:\n- {string.Join("\n- ", plan.PackageActions)}\n\nCode Actions:\n- {string.Join("\n- ", plan.CodeActions)}\n\nValidation:\n- {string.Join("\n- ", plan.ValidationSteps)}\n\nRisks:\n- {string.Join("\n- ", plan.Risks)}";
    }
}
