namespace ReactiveMcp.Tests;

/// <summary>
/// Tests for manifest loading and lookup behavior.
/// </summary>
public class CatalogLoadingTests
{
    /// <summary>
    /// Verifies that the embedded catalog loads all expected knowledge areas.
    /// </summary>
    [Test]
    public async Task EmbeddedCatalog_Loads_All_Requested_Manifest_Areas()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        await Assert.That(catalog.GetAll().Count).IsGreaterThanOrEqualTo(9);
        await Assert.That(catalog.GetById("rx-dotnet-core")).IsNotNull();
        await Assert.That(catalog.GetById("rx-testing")).IsNotNull();
        await Assert.That(catalog.GetById("system-linq-async")).IsNotNull();
    }

    /// <summary>
    /// Verifies that search can locate testing guidance by keyword.
    /// </summary>
    [Test]
    public async Task Search_Finds_Virtual_Time_Testing_Guidance()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var results = catalog.Search("virtual time testing");
        await Assert.That(results.Any(result => result.Id == "rx-testing")).IsTrue();
    }

    /// <summary>
    /// Verifies that id lookups are case-insensitive.
    /// </summary>
    [Test]
    public async Task GetById_Is_Case_Insensitive()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var lower = catalog.GetById("rx-dotnet-core");
        var mixed = catalog.GetById("Rx-DotNet-Core");

        await Assert.That(lower).IsNotNull();
        await Assert.That(mixed).IsNotNull();
        await Assert.That(lower!.Id).IsEqualTo(mixed!.Id);
    }

    /// <summary>
    /// Verifies that the embedded catalog is offline-first and does not require DeepWiki links.
    /// </summary>
    [Test]
    public async Task EmbeddedCatalog_Uses_Offline_First_Source_Metadata()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var manifest = catalog.GetById("rx-dotnet-core");

        await Assert.That(manifest).IsNotNull();
        await Assert.That(manifest!.Sources.All(static source => !source.Url.Contains("deepwiki.com", StringComparison.OrdinalIgnoreCase))).IsTrue();
        await Assert.That(manifest.Harvest.SourceSummary.Contains("offline", StringComparison.OrdinalIgnoreCase)).IsTrue();
    }
}
