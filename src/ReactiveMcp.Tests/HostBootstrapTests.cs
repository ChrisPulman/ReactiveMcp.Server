namespace ReactiveMcp.Tests;

/// <summary>
/// Tests for server host bootstrap and metadata.
/// </summary>
public class HostBootstrapTests
{
    /// <summary>
    /// Verifies that the host resolves the core guidance services.
    /// </summary>
    [Test]
    public async Task CreateHost_Registers_Guidance_Services()
    {
        using var host = Program.CreateHost([]);

        var catalog = host.Services.GetRequiredService<IKnowledgeCatalog>();
        var guidance = host.Services.GetRequiredService<IReactiveGuidanceService>();

        await Assert.That(catalog.GetAll().Count).IsGreaterThanOrEqualTo(9);
        await Assert.That(guidance).IsNotNull();
    }

    /// <summary>
    /// Verifies that safe metadata mode omits optional rich fields.
    /// </summary>
    [Test]
    public async Task BuildServerInfo_DefaultMode_Suppresses_Rich_Metadata()
    {
        var serverInfo = Program.BuildServerInfo();

        await Assert.That(serverInfo.Name).IsEqualTo("reactive-mcp-server");
        await Assert.That(serverInfo.Title).IsNull();
        await Assert.That(serverInfo.Description).IsNull();
        await Assert.That(serverInfo.WebsiteUrl).IsNull();
    }
}
