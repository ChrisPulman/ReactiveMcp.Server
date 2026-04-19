namespace ReactiveMcp.Tests;

/// <summary>
/// Tests for recommendation and comparison behavior.
/// </summary>
public class RecommendationTests
{
    /// <summary>
    /// Verifies that deterministic testing signals recommend testing and scheduler guidance.
    /// </summary>
    [Test]
    public async Task Recommend_Includes_Testing_And_Schedulers_For_Deterministic_Tests()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveGuidanceService guidance = new ReactiveGuidanceService(catalog);

        var result = guidance.Recommend(new RecommendationRequest(
            "worker service",
            "test project",
            ["virtual time tests", "retry", "timers"],
            ["determinism"],
            []));

        await Assert.That(result.SelectedManifestIds).Contains("rx-testing");
        await Assert.That(result.SelectedManifestIds).Contains("rx-schedulers");
        await Assert.That(result.SuggestedPackages).Contains("Microsoft.Reactive.Testing");
    }

    /// <summary>
    /// Verifies that async-enumerable signals recommend Ix and System.Linq.Async guidance.
    /// </summary>
    [Test]
    public async Task Recommend_Includes_Ix_And_SystemLinqAsync_For_AsyncEnumerable_Signals()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveGuidanceService guidance = new ReactiveGuidanceService(catalog);

        var result = guidance.Recommend(new RecommendationRequest(
            "library",
            "integration layer",
            ["IAsyncEnumerable", "pull-based streaming"],
            [],
            []));

        await Assert.That(result.SelectedManifestIds).Contains("ix-dotnet");
        await Assert.That(result.SelectedManifestIds).Contains("system-linq-async");
        await Assert.That(result.SuggestedPackages).Contains("System.Linq.Async");
    }

    /// <summary>
    /// Verifies that compare produces a meaningful side-by-side summary.
    /// </summary>
    [Test]
    public async Task Compare_Returns_A_Summary_For_Two_Manifest_Areas()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();
        IReactiveGuidanceService guidance = new ReactiveGuidanceService(catalog);

        var result = guidance.Compare("rx-testing", "rx-performance");

        await Assert.That(result.Summary).Contains("Compared");
        await Assert.That(result.Tradeoffs.Count).IsGreaterThanOrEqualTo(2);
    }
}
