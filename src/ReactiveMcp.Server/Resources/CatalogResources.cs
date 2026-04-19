using ModelContextProtocol.Server;
using ReactiveMcp.Core.Abstractions;
using ReactiveMcp.Server.Serialization;
using System.ComponentModel;

namespace ReactiveMcp.Server.Resources;

/// <summary>
/// Read-only MCP resources for the reactive catalog.
/// </summary>
[McpServerResourceType]
public sealed class CatalogResources
{
    /// <summary>
    /// Gets a catalog overview resource.
    /// </summary>
    /// <param name="catalog">The knowledge catalog.</param>
    /// <returns>A serialized catalog overview.</returns>
    [McpServerResource(UriTemplate = "reactive://catalog", Name = "Reactive Catalog", MimeType = "application/json")]
    [Description("Read-only overview of the Reactive .NET catalog.")]
    public static string GetCatalog(IKnowledgeCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        return JsonOutput.Serialize(new
        {
            Count = catalog.GetAll().Count,
            Categories = catalog.GetAll().Select(static manifest => manifest.Category).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(static value => value, StringComparer.OrdinalIgnoreCase),
            Items = catalog.GetAll().Select(static manifest => new
            {
                manifest.Id,
                manifest.DisplayName,
                manifest.Category,
            })
        });
    }

    /// <summary>
    /// Gets a single manifest resource.
    /// </summary>
    /// <param name="catalog">The knowledge catalog.</param>
    /// <param name="id">The manifest identifier.</param>
    /// <returns>A serialized manifest.</returns>
    [McpServerResource(UriTemplate = "reactive://ecosystem/{id}", Name = "Reactive Ecosystem Manifest", MimeType = "application/json")]
    [Description("Read-only manifest for a specific Reactive .NET ecosystem area.")]
    public static string GetManifest(IKnowledgeCatalog catalog, string id)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var manifest = catalog.GetById(id) ?? throw new InvalidOperationException($"Unknown manifest id '{id}'.");
        return JsonOutput.Serialize(manifest);
    }

    /// <summary>
    /// Gets a resource tailored for debugging best practices.
    /// </summary>
    /// <param name="catalog">The knowledge catalog.</param>
    /// <returns>A serialized debugging guidance payload.</returns>
    [McpServerResource(UriTemplate = "reactive://best-practices/debugging", Name = "Reactive Debugging Guidance", MimeType = "application/json")]
    [Description("Read-only debugging best practices focused on schedulers, virtual time, subjects, and performance trade-offs.")]
    public static string GetDebuggingGuidance(IKnowledgeCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var ids = new[]
        {
            "rx-dotnet-core",
            "rx-schedulers",
            "rx-testing",
            "rx-subjects",
            "rx-performance"
        };

        var manifests = ids
            .Select(catalog.GetById)
            .Where(static manifest => manifest is not null)
            .ToArray();

        return JsonOutput.Serialize(new
        {
            Purpose = "Reactive debugging guidance for timing, disposal, subject semantics, and performance-sensitive pipelines.",
            Manifests = manifests,
        });
    }
}
