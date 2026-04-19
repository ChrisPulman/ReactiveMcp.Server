using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ReactiveMcp.Core.Abstractions;
using ReactiveMcp.Core.Services;
using ReactiveMcp.Knowledge.Services;
using ReactiveMcp.Server.Prompts;
using ReactiveMcp.Server.Resources;
using ReactiveMcp.Server.Tools;
using System.Reflection;

namespace ReactiveMcp.Server;

/// <summary>
/// Entry point for the Reactive MCP server host.
/// </summary>
public static class Program
{
    private static readonly string[] SuppressedMetadataKeys =
    [
        "Title",
        "Description",
        "WebsiteUrl",
        "Icons"
    ];

    /// <summary>
    /// Builds the host used by the MCP server.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>The configured host.</returns>
    public static IHost CreateHost(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        ConfigureLogging(builder);

        builder.Services.AddSingleton<IKnowledgeCatalog, EmbeddedKnowledgeCatalog>();
        builder.Services.AddSingleton<IReactiveGuidanceService, ReactiveGuidanceService>();

        builder.Services
            .AddMcpServer(options => options.ServerInfo = BuildServerInfo())
            .WithStdioServerTransport()
            .WithTools<CatalogTools>()
            .WithTools<GuidanceTools>()
            .WithResources<CatalogResources>()
            .WithPrompts<ReactivePrompts>();

        return builder.Build();
    }

    /// <summary>
    /// Builds the metadata advertised to MCP clients.
    /// </summary>
    /// <returns>The server metadata.</returns>
    public static Implementation BuildServerInfo()
    {
        var assembly = typeof(Program).Assembly;
        var serverInfo = new Implementation
        {
            Name = "reactive-mcp-server",
            Version = assembly
                .GetCustomAttributes<AssemblyInformationalVersionAttribute>()
                .FirstOrDefault()?.InformationalVersion
                ?? assembly.GetName().Version?.ToString()
                ?? "0.0.0"
        };

        if (!ShouldAdvertiseRichClientMetadata())
        {
            return serverInfo;
        }

        serverInfo.Title = "Reactive MCP Server";
        serverInfo.Description = "Reactive .NET architecture, debugging, testing, and modernization guidance curated from dotnet/reactive.";
        serverInfo.WebsiteUrl = "https://github.com/ChrisPulman/ReactiveMcp.Server";
        return serverInfo;
    }

    /// <summary>
    /// Returns a value indicating whether rich optional metadata should be advertised.
    /// </summary>
    /// <returns><see langword="false"/> because compatibility mode is always enforced.</returns>
    internal static bool ShouldAdvertiseRichClientMetadata() => false;

    /// <summary>
    /// Gets the names of optional client metadata fields omitted in compatibility mode.
    /// </summary>
    /// <returns>The omitted field names.</returns>
    public static IReadOnlyList<string> GetSuppressedClientMetadataKeys() => SuppressedMetadataKeys;

    /// <summary>
    /// Starts the server process.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Main(string[] args) => await CreateHost(args).RunAsync();

    private static void ConfigureLogging(HostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
    }
}
