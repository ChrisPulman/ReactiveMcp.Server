using ReactiveMcp.Server.Prompts;

namespace ReactiveMcp.Tests;

/// <summary>
/// Tests for prompt generation.
/// </summary>
public class PromptTests
{
    /// <summary>
    /// Verifies that the create prompt includes deterministic testing guidance when requested.
    /// </summary>
    [Test]
    public async Task CreateReactiveApp_Includes_TestScheduler_Guidance()
    {
        IReactiveGuidanceService guidance = new ReactiveGuidanceService(new EmbeddedKnowledgeCatalog());

        var prompt = ReactivePrompts.CreateReactiveApp(
            guidance,
            stack: "worker service",
            appKind: "test project",
            features: "virtual time tests, retry",
            constraints: "determinism");

        await Assert.That(prompt).Contains("TestScheduler");
        await Assert.That(prompt).Contains("Scheduler boundaries are intentional");
    }
}
