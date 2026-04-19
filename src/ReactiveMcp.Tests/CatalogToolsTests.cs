using ReactiveMcp.Server.Tools;
using System.Text.Json;

namespace ReactiveMcp.Tests;

/// <summary>
/// Tests for the catalog MCP tools.
/// </summary>
public class CatalogToolsTests
{
    /// <summary>
    /// Verifies that the catalog list tool returns count and items.
    /// </summary>
    [Test]
    public async Task ListCatalog_Returns_Valid_Json_With_Count_And_Items()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var json = CatalogTools.ListCatalog(catalog);
        using var doc = JsonDocument.Parse(json);

        await Assert.That(doc.RootElement.GetProperty("count").GetInt32()).IsGreaterThanOrEqualTo(9);
        await Assert.That(doc.RootElement.GetProperty("items").GetArrayLength()).IsGreaterThanOrEqualTo(9);
    }

    /// <summary>
    /// Verifies that search returns matching results.
    /// </summary>
    [Test]
    public async Task SearchCatalog_With_Query_Returns_Relevant_Results()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var json = CatalogTools.SearchCatalog(catalog, query: "subject replay");
        using var doc = JsonDocument.Parse(json);
        var results = doc.RootElement.GetProperty("results").EnumerateArray().ToList();

        await Assert.That(results.Count).IsGreaterThan(0);
        await Assert.That(results.Any(item => item.GetProperty("id").GetString() == "rx-subjects")).IsTrue();
    }

    /// <summary>
    /// Verifies that getting a manifest returns the expected id.
    /// </summary>
    [Test]
    public async Task GetManifest_Returns_Full_Manifest_For_Known_Id()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var json = CatalogTools.GetManifest(catalog, "rx-testing");
        using var doc = JsonDocument.Parse(json);

        await Assert.That(doc.RootElement.GetProperty("id").GetString()).IsEqualTo("rx-testing");
        await Assert.That(doc.RootElement.TryGetProperty("recommendedPatterns", out _)).IsTrue();
    }
}
