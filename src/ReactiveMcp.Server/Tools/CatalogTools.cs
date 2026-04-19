using ModelContextProtocol.Server;
using ReactiveMcp.Core.Abstractions;
using ReactiveMcp.Server.Serialization;
using System.ComponentModel;

namespace ReactiveMcp.Server.Tools;

/// <summary>
/// MCP tools for exploring the Reactive .NET knowledge catalog.
/// </summary>
[McpServerToolType]
public sealed class CatalogTools
{
    /// <summary>
    /// Lists all catalog entries.
    /// </summary>
    /// <param name="catalog">The knowledge catalog.</param>
    /// <returns>A JSON payload describing the catalog.</returns>
    [McpServerTool(Name = "reactive_catalog_list"), Description("List all known Reactive .NET ecosystem areas.")]
    public static string ListCatalog(IKnowledgeCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        return JsonOutput.Serialize(new
        {
            Count = catalog.GetAll().Count,
            Items = catalog.GetAll().Select(static manifest => new
            {
                manifest.Id,
                manifest.DisplayName,
                manifest.Category,
                manifest.Summary,
            })
        });
    }

    /// <summary>
    /// Searches the catalog.
    /// </summary>
    /// <param name="catalog">The knowledge catalog.</param>
    /// <param name="query">Optional free-form query.</param>
    /// <param name="category">Optional category filter.</param>
    /// <returns>A JSON search result payload.</returns>
    [McpServerTool(Name = "reactive_catalog_search"), Description("Search the Reactive .NET catalog by keyword or category.")]
    public static string SearchCatalog(
        IKnowledgeCatalog catalog,
        [Description("Optional free-form query such as 'tests virtual time' or 'subjects replay'.")] string? query = null,
        [Description("Optional category filter such as architecture, rx, testing, performance, async, or scheduling.")] string? category = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        return JsonOutput.Serialize(new
        {
            Query = query,
            Category = category,
            Results = catalog.Search(query, category).Select(static manifest => new
            {
                manifest.Id,
                manifest.DisplayName,
                manifest.Category,
                manifest.NuGetPackages,
                manifest.Summary,
            })
        });
    }

    /// <summary>
    /// Gets one manifest by identifier.
    /// </summary>
    /// <param name="catalog">The knowledge catalog.</param>
    /// <param name="id">The manifest identifier.</param>
    /// <returns>A full manifest JSON payload.</returns>
    [McpServerTool(Name = "reactive_catalog_get"), Description("Get detailed guidance for one Reactive .NET ecosystem area by id.")]
    public static string GetManifest(
        IKnowledgeCatalog catalog,
        [Description("The manifest id such as rx-testing, ix-dotnet, rx-subjects, or system-linq-async.")] string id)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var manifest = catalog.GetById(id) ?? throw new InvalidOperationException($"Unknown manifest id '{id}'.");
        return JsonOutput.Serialize(manifest);
    }
}
