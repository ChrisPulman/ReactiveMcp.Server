using ReactiveMcp.Core.Abstractions;
using ReactiveMcp.Core.Models;
using System.Text.Json;

namespace ReactiveMcp.Knowledge.Services;

/// <summary>
/// Loads embedded knowledge manifests from JSON resources.
/// </summary>
public sealed class EmbeddedKnowledgeCatalog : IKnowledgeCatalog
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly Lazy<IReadOnlyList<EcosystemManifest>> manifests = new(LoadManifests);

    /// <inheritdoc />
    public IReadOnlyList<EcosystemManifest> GetAll() => this.manifests.Value;

    /// <inheritdoc />
    public EcosystemManifest? GetById(string id) =>
        GetAll().FirstOrDefault(manifest => string.Equals(manifest.Id, id, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc />
    public IReadOnlyList<EcosystemManifest> Search(string? query = null, string? category = null)
    {
        IEnumerable<EcosystemManifest> results = GetAll();

        if (!string.IsNullOrWhiteSpace(category))
        {
            results = results.Where(manifest => string.Equals(manifest.Category, category, StringComparison.OrdinalIgnoreCase));
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            return results.OrderBy(static manifest => manifest.DisplayName, StringComparer.OrdinalIgnoreCase).ToArray();
        }

        var tokens = query
            .Split([' ', ',', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(static token => token.ToLowerInvariant())
            .ToArray();

        return results
            .Where(manifest => tokens.All(token => BuildSearchText(manifest).Contains(token, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(static manifest => manifest.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<EcosystemManifest> LoadManifests()
    {
        var assembly = typeof(EmbeddedKnowledgeCatalog).Assembly;
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(static name => name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var loaded = new List<EcosystemManifest>(resourceNames.Length);
        foreach (var resourceName in resourceNames)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Unable to load embedded resource '{resourceName}'.");

            var manifest = JsonSerializer.Deserialize<EcosystemManifest>(stream, SerializerOptions)
                ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' could not be deserialized.");

            loaded.Add(manifest);
        }

        return loaded;
    }

    private static string BuildSearchText(EcosystemManifest manifest) =>
        string.Join(
            ' ',
            manifest.Id,
            manifest.DisplayName,
            manifest.Category,
            manifest.Summary,
            string.Join(' ', manifest.NuGetPackages),
            string.Join(' ', manifest.Keywords),
            string.Join(' ', manifest.RelatedLibraries),
            string.Join(' ', manifest.RecommendedPatterns),
            string.Join(' ', manifest.CommonPitfalls));
}
