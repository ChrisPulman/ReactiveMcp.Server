using ReactiveMcp.Server.Tools;
using System.Text.Json;

namespace ReactiveMcp.Tests;

/// <summary>
/// Tests for reactive plan review heuristics.
/// </summary>
public class ReviewPlanTests
{
    /// <summary>
    /// Verifies that sleep-based timing tests are flagged.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Flags_WallClock_Testing()
    {
        IReactiveGuidanceService guidance = new ReactiveGuidanceService(new EmbeddedKnowledgeCatalog());

        var result = guidance.ReviewPlan(
            "worker service",
            "System.Reactive",
            "Use Thread.Sleep(500) after subscribing and then assert the debounce result.");

        await Assert.That(result.Findings.Any(f => f.Code == "RX001")).IsTrue();
    }

    /// <summary>
    /// Verifies that subject-as-state-store plans are flagged.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Flags_Subject_As_State_Store()
    {
        IReactiveGuidanceService guidance = new ReactiveGuidanceService(new EmbeddedKnowledgeCatalog());

        var result = guidance.ReviewPlan(
            "desktop app",
            "System.Reactive",
            "Store all mutable application state in a BehaviorSubject and use it as the central state store.");

        await Assert.That(result.Findings.Any(f => f.Code == "RX003")).IsTrue();
    }

    /// <summary>
    /// Verifies that undisposed subscriptions are flagged.
    /// </summary>
    [Test]
    public async Task ReviewPlan_Flags_Subscribe_Without_Disposal()
    {
        IReactiveGuidanceService guidance = new ReactiveGuidanceService(new EmbeddedKnowledgeCatalog());

        var result = guidance.ReviewPlan(
            "service",
            "System.Reactive",
            "Call stream.Subscribe(x => Handle(x)); and do not mention disposal or ownership.");

        await Assert.That(result.Findings.Any(f => f.Code == "RX002")).IsTrue();
    }

    /// <summary>
    /// Verifies that real C# snippets produce actionable example replacements.
    /// </summary>
    [Test]
    public async Task ReviewPlan_For_CSharp_Snippet_Returns_Best_Practice_Code_Snippets()
    {
        IReactiveGuidanceService guidance = new ReactiveGuidanceService(new EmbeddedKnowledgeCatalog());

        var result = guidance.ReviewPlan(
            "desktop app",
            "System.Reactive",
            """
            public sealed class SampleViewModel
            {
                public void Start(IObservable<int> stream)
                {
                    stream.Subscribe(x => Console.WriteLine(x));
                }
            }
            """);

        var finding = result.Findings.FirstOrDefault(f => f.Code == "RX002");

        await Assert.That(finding).IsNotNull();
        await Assert.That(finding!.BestPracticeSnippet).IsNotNull();
        await Assert.That(finding.BestPracticeSnippet!).Contains("CompositeDisposable");
        await Assert.That(finding.BestPracticeSnippet!).Contains("_subscriptions.Add(subscription)");
        await Assert.That(result.BestPracticeSnippets.Count).IsGreaterThan(0);
    }

    /// <summary>
    /// Verifies that event-handler style code gets a reactive conversion suggestion.
    /// </summary>
    [Test]
    public async Task ReviewPlan_For_Event_Handler_Code_Returns_FromEventPattern_Guidance()
    {
        IReactiveGuidanceService guidance = new ReactiveGuidanceService(new EmbeddedKnowledgeCatalog());

        var result = guidance.ReviewPlan(
            "desktop app",
            "System.Reactive",
            """
            public void Wire(Button button)
            {
                button.Click += OnClick;
            }
            """);

        var finding = result.Findings.FirstOrDefault(f => f.Code == "RX005");

        await Assert.That(finding).IsNotNull();
        await Assert.That(finding!.BestPracticeSnippet).IsNotNull();
        await Assert.That(finding.BestPracticeSnippet!).Contains("Observable.FromEventPattern");
    }

    /// <summary>
    /// Verifies that the dedicated MCP C# review tool returns structured snippet-review output.
    /// </summary>
    [Test]
    public async Task ReviewCSharpSnippetTool_Returns_Structured_Review_With_Best_Practice_Snippets()
    {
        IReactiveGuidanceService guidance = new ReactiveGuidanceService(new EmbeddedKnowledgeCatalog());

        var json = GuidanceTools.ReviewCSharpSnippet(
            guidance,
            code: """
            public sealed class SampleViewModel
            {
                public void Start(IObservable<int> stream)
                {
                    stream.Subscribe(x => Console.WriteLine(x));
                }
            }
            """,
            stack: "desktop app",
            libraries: "System.Reactive",
            reviewFocus: "disposal, schedulers");

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        await Assert.That(root.GetProperty("summary").GetString()).Contains("reactive review item");
        await Assert.That(root.GetProperty("findings").GetArrayLength()).IsGreaterThan(0);
        await Assert.That(root.GetProperty("bestPracticeSnippets").GetArrayLength()).IsGreaterThan(0);
    }

    /// <summary>
    /// Verifies that snippet review findings expose approximate line diagnostics.
    /// </summary>
    [Test]
    public async Task ReviewCSharpSnippetTool_Returns_Approximate_Line_Diagnostics()
    {
        IReactiveGuidanceService guidance = new ReactiveGuidanceService(new EmbeddedKnowledgeCatalog());

        var json = GuidanceTools.ReviewCSharpSnippet(
            guidance,
            code: """
            public sealed class SampleViewModel
            {
                public void Start(IObservable<int> stream)
                {
                    stream.Subscribe(x => Console.WriteLine(x));
                }
            }
            """,
            stack: "desktop app",
            libraries: "System.Reactive",
            reviewFocus: "disposal");

        using var doc = JsonDocument.Parse(json);
        var finding = doc.RootElement
            .GetProperty("findings")
            .EnumerateArray()
            .First(element => element.GetProperty("code").GetString() == "RX002");

        await Assert.That(finding.GetProperty("startLine").GetInt32()).IsEqualTo(5);
        await Assert.That(finding.GetProperty("endLine").GetInt32()).IsEqualTo(5);
        await Assert.That(finding.GetProperty("startColumn").GetInt32()).IsGreaterThan(0);
    }
}
