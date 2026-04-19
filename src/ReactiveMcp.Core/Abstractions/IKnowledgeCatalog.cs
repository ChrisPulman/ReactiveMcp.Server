using ReactiveMcp.Core.Models;

namespace ReactiveMcp.Core.Abstractions;

/// <summary>
/// Provides access to the embedded Reactive .NET knowledge catalog.
/// </summary>
public interface IKnowledgeCatalog
{
    /// <summary>
    /// Gets all manifests in the catalog.
    /// </summary>
    /// <returns>All known manifests.</returns>
    IReadOnlyList<EcosystemManifest> GetAll();

    /// <summary>
    /// Gets a manifest by its stable identifier.
    /// </summary>
    /// <param name="id">The stable identifier.</param>
    /// <returns>The matching manifest if present; otherwise <see langword="null"/>.</returns>
    EcosystemManifest? GetById(string id);

    /// <summary>
    /// Searches the catalog by free-form query and optional category.
    /// </summary>
    /// <param name="query">Optional free-form query.</param>
    /// <param name="category">Optional category filter.</param>
    /// <returns>The matching manifests.</returns>
    IReadOnlyList<EcosystemManifest> Search(string? query = null, string? category = null);
}
