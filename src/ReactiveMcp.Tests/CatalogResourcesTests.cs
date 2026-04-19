using ReactiveMcp.Server.Resources;
using System.Text.Json;

namespace ReactiveMcp.Tests;

/// <summary>
/// Tests for the catalog resources.
/// </summary>
public class CatalogResourcesTests
{
    /// <summary>
    /// Verifies that the debug guidance resource returns the expected purpose and manifests.
    /// </summary>
    [Test]
    public async Task DebuggingResource_Returns_Expected_Manifest_Set()
    {
        IKnowledgeCatalog catalog = new EmbeddedKnowledgeCatalog();

        var json = CatalogResources.GetDebuggingGuidance(catalog);
        using var doc = JsonDocument.Parse(json);

        await Assert.That(doc.RootElement.GetProperty("purpose").GetString()).Contains("Reactive debugging guidance");
        await Assert.That(doc.RootElement.GetProperty("manifests").GetArrayLength()).IsGreaterThanOrEqualTo(4);
    }
}
