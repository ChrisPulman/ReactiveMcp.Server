using ReactiveMcp.Core.Abstractions;
using ReactiveMcp.Core.Models;
using System.Text;

namespace ReactiveMcp.Core.Services;

/// <summary>
/// Applies lightweight heuristics over the embedded Reactive .NET catalog.
/// </summary>
public sealed class ReactiveGuidanceService(IKnowledgeCatalog catalog) : IReactiveGuidanceService
{
    private const string SubscriptionSnippet = """
private readonly CompositeDisposable _subscriptions = new();

public void Start(IObservable<int> stream)
{
    var subscription = stream
        .ObserveOn(TaskPoolScheduler.Default)
        .Subscribe(
            onNext: x => Console.WriteLine(x),
            onError: ex => LogError(ex));

    _subscriptions.Add(subscription);
}

public void Dispose() => _subscriptions.Dispose();
""";

    private const string TestSchedulerSnippet = """
var scheduler = new TestScheduler();
var observer = scheduler.CreateObserver<int>();

Observable
    .Timer(TimeSpan.FromMilliseconds(500), scheduler)
    .Select(_ => 42)
    .Subscribe(observer);

scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);

ReactiveAssert.AreElementsEqual(
    [ReactiveTest.OnNext(TimeSpan.FromMilliseconds(500).Ticks, 42)],
    observer.Messages);
""";

    private const string SubjectBoundarySnippet = """
private readonly Subject<DomainEvent> _events = new();

public IObservable<DomainEvent> Events => _events.AsObservable();

public void Publish(DomainEvent value)
{
    _events.OnNext(value);
}

// Keep mutable application state in explicit fields or domain models,
// not inside the Subject itself.
""";

    private const string AsyncVoidSnippet = """
public Task SaveAsync()
{
    return _saveRequests
        .Take(1)
        .SelectMany(_ => Observable.FromAsync(ct => repository.SaveAsync(ct)))
        .DefaultIfEmpty()
        .ToTask();
}
""";

    private const string FromEventPatternSnippet = """
var clicks = Observable.FromEventPattern<EventHandler, EventArgs>(
        handler => (_, args) => handler(args),
        handler => button.Click += handler,
        handler => button.Click -= handler)
    .Select(_ => Unit.Default);

var subscription = clicks
    .Throttle(TimeSpan.FromMilliseconds(250))
    .Subscribe(_ => ExecuteAction());
""";

    private const string SchedulerBoundarySnippet = """
var updates = source
    .SubscribeOn(TaskPoolScheduler.Default)
    .Select(Transform)
    .ObserveOn(SynchronizationContext.Current!)
    .Subscribe(UpdateUi, LogError);
""";

    private readonly record struct SourceLocation(
        string? DetectedPattern,
        int? StartLine,
        int? EndLine,
        int? StartColumn,
        int? EndColumn);

    /// <inheritdoc />
    public RecommendationResult Recommend(RecommendationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var manifestIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "reactive-architecture",
            "rx-dotnet-core"
        };

        var signalText = string.Join(' ', [request.Stack ?? string.Empty, request.AppKind ?? string.Empty, .. request.Features, .. request.Constraints, .. request.ExistingLibraries]);
        AddBySignals(signalText, manifestIds);

        if (IsTestScenario(request.AppKind, request.Features, request.ExistingLibraries))
        {
            manifestIds.Add("rx-testing");
        }

        var manifests = manifestIds
            .Select(catalog.GetById)
            .Where(static manifest => manifest is not null)
            .Cast<EcosystemManifest>()
            .ToArray();

        return new RecommendationResult(
            $"Recommended {manifests.Length} reactive guidance area(s) for {request.Stack ?? request.AppKind ?? "general"} work.",
            manifests.Select(static manifest => manifest.Id).ToArray(),
            Merge(manifests, static manifest => manifest.NuGetPackages),
            Merge(manifests, static manifest => manifest.RecommendedPatterns),
            Merge(manifests, static manifest => manifest.AvoidPatterns),
            Merge(manifests, static manifest => manifest.SetupSteps),
            Merge(manifests, static manifest => manifest.CommonPitfalls),
            Merge(manifests, static manifest => manifest.RelatedLibraries));
    }

    /// <inheritdoc />
    public ReviewResult ReviewPlan(string? stack, string? libraries, string planText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(planText);
        return ReviewText(stack, libraries, planText, planText);
    }

    private ReviewResult ReviewText(string? stack, string? libraries, string signalText, string locationText)
    {
        var normalizedText = string.Join(' ', stack ?? string.Empty, libraries ?? string.Empty, signalText);
        var combined = normalizedText.ToLowerInvariant();
        var findings = new List<ReviewFinding>();
        var recommendedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "reactive-architecture",
            "rx-dotnet-core"
        };

        AddBySignals(combined, recommendedIds);

        if (combined.Contains("thread.sleep", StringComparison.Ordinal) || combined.Contains("task.delay", StringComparison.Ordinal))
        {
            var location = BuildFindingLocation(locationText, "Thread.Sleep", "Task.Delay");
            findings.Add(new ReviewFinding(
                "warning",
                "RX001",
                "The plan uses wall-clock waiting for reactive tests or timing behavior.",
                "Prefer TestScheduler, virtual time, and deterministic recorded notifications over sleeps and real timers.",
                "Reactive timing behavior becomes flaky and slow when tests rely on wall-clock waiting.",
                location.DetectedPattern,
                TestSchedulerSnippet,
                location.StartLine,
                location.EndLine,
                location.StartColumn,
                location.EndColumn));
            recommendedIds.Add("rx-testing");
        }

        if (combined.Contains(".subscribe(", StringComparison.Ordinal)
            && !combined.Contains("dispose", StringComparison.Ordinal)
            && !combined.Contains("using", StringComparison.Ordinal)
            && !combined.Contains("compositedisposable", StringComparison.Ordinal))
        {
            var location = BuildFindingLocation(locationText, ".Subscribe(");
            findings.Add(new ReviewFinding(
                "warning",
                "RX002",
                "The plan appears to subscribe without describing disposal or lifetime control.",
                "Track IDisposable subscriptions explicitly and ensure they are disposed at the correct boundary.",
                "Undisposed subscriptions create leaks, ghost observers, and hard-to-debug duplicate processing.",
                location.DetectedPattern,
                SubscriptionSnippet,
                location.StartLine,
                location.EndLine,
                location.StartColumn,
                location.EndColumn));
        }

        if ((combined.Contains("subject<", StringComparison.Ordinal) || combined.Contains("behaviorsubject", StringComparison.Ordinal) || combined.Contains("replaysubject", StringComparison.Ordinal))
            && (combined.Contains("state", StringComparison.Ordinal) || combined.Contains("store", StringComparison.Ordinal) || combined.Contains("cache", StringComparison.Ordinal)))
        {
            var location = BuildFindingLocation(locationText, "Subject<", "BehaviorSubject", "ReplaySubject");
            findings.Add(new ReviewFinding(
                "warning",
                "RX003",
                "The plan appears to use a subject as a general-purpose mutable state container.",
                "Use subjects at boundaries or for carefully justified multicast needs; prefer query composition and explicit state ownership elsewhere.",
                "Subjects are delivery mechanisms, not a replacement for explicit domain state ownership.",
                location.DetectedPattern,
                SubjectBoundarySnippet,
                location.StartLine,
                location.EndLine,
                location.StartColumn,
                location.EndColumn));
            recommendedIds.Add("rx-subjects");
        }

        if (combined.Contains("async void", StringComparison.Ordinal))
        {
            var location = BuildFindingLocation(locationText, "async void");
            findings.Add(new ReviewFinding(
                "warning",
                "RX004",
                "The plan contains async void, which is difficult to compose and observe in reactive flows.",
                "Prefer Task-returning methods and explicit conversion into observable or async-enumerable pipelines.",
                "async void hides completion and exception flow from callers and tests.",
                location.DetectedPattern,
                AsyncVoidSnippet,
                location.StartLine,
                location.EndLine,
                location.StartColumn,
                location.EndColumn));
        }

        if ((combined.Contains("event handler", StringComparison.Ordinal)
                || combined.Contains("events", StringComparison.Ordinal)
                || combined.Contains("+= onclick", StringComparison.Ordinal)
                || combined.Contains("click +=", StringComparison.Ordinal))
            && !combined.Contains("fromevent", StringComparison.Ordinal)
            && !combined.Contains("observable", StringComparison.Ordinal))
        {
            var location = BuildFindingLocation(locationText, " += ", "event handler", "Click +=");
            findings.Add(new ReviewFinding(
                "info",
                "RX005",
                "The plan describes event handling without a clear reactive conversion boundary.",
                "Consider FromEventPattern, observable factories, or explicit adapters so operators and schedulers remain available.",
                "Reactive conversion at the boundary keeps composition, throttling, buffering, and scheduling available.",
                location.DetectedPattern,
                FromEventPatternSnippet,
                location.StartLine,
                location.EndLine,
                location.StartColumn,
                location.EndColumn));
        }

        if ((combined.Contains("iasyncenumerable", StringComparison.Ordinal) || combined.Contains("async enumerable", StringComparison.Ordinal))
            && (combined.Contains("push", StringComparison.Ordinal) || combined.Contains("broadcast", StringComparison.Ordinal)))
        {
            var location = BuildFindingLocation(locationText, "IAsyncEnumerable", "broadcast", "push");
            findings.Add(new ReviewFinding(
                "info",
                "RX006",
                "The plan mixes pull-based async iteration with push-based broadcast requirements.",
                "Validate whether IObservable<T> is the better abstraction for fan-out and live event delivery.",
                "IAsyncEnumerable<T> is consumer-driven, while IObservable<T> is producer-driven and better suited to broadcast event streams.",
                location.DetectedPattern,
                SchedulerBoundarySnippet,
                location.StartLine,
                location.EndLine,
                location.StartColumn,
                location.EndColumn));
            recommendedIds.Add("system-linq-async");
            recommendedIds.Add("rx-dotnet-core");
        }

        if ((combined.Contains("observeon", StringComparison.Ordinal) || combined.Contains("subscribeon", StringComparison.Ordinal) || combined.Contains("synchronizationcontext", StringComparison.Ordinal))
            || ((combined.Contains("ui", StringComparison.Ordinal) || combined.Contains("dispatcher", StringComparison.Ordinal)) && combined.Contains("subscribe(", StringComparison.Ordinal)))
        {
            var location = BuildFindingLocation(locationText, "ObserveOn", "SubscribeOn", "SynchronizationContext", "Dispatcher");
            findings.Add(new ReviewFinding(
                "info",
                "RX007",
                "The plan appears to involve scheduler-sensitive code paths that need explicit observation and production boundaries.",
                "Make SubscribeOn and ObserveOn choices explicit so threading behavior is deterministic and reviewable.",
                "Hidden scheduler transitions are a common source of race conditions and UI-thread bugs.",
                location.DetectedPattern,
                SchedulerBoundarySnippet,
                location.StartLine,
                location.EndLine,
                location.StartColumn,
                location.EndColumn));
            recommendedIds.Add("rx-schedulers");
        }

        if (!findings.Any())
        {
            findings.Add(new ReviewFinding(
                "info",
                "RX000",
                "No obvious high-risk reactive anti-patterns were detected in the submitted plan.",
                "Still verify disposal, scheduler boundaries, test determinism, and subject usage during implementation.",
                "Reactive correctness often depends on lifetime, time, and concurrency contracts that are easy to overlook.",
                null,
                null,
                null,
                null,
                null,
                null));
        }

        var snippets = findings
            .Select(static finding => finding.BestPracticeSnippet)
            .Where(static snippet => !string.IsNullOrWhiteSpace(snippet))
            .Distinct(StringComparer.Ordinal)
            .Cast<string>()
            .ToArray();

        return new ReviewResult(
            $"Found {findings.Count} reactive review item(s).",
            findings,
            recommendedIds.ToArray(),
            snippets);
    }

    /// <inheritdoc />
    public ReviewResult ReviewCSharpSnippet(string? stack, string? libraries, string? reviewFocus, string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var signalText = string.Join(
            Environment.NewLine,
            [
                "Review this real C# snippet for reactive correctness and best practices.",
                string.IsNullOrWhiteSpace(reviewFocus) ? string.Empty : $"Review focus: {reviewFocus}",
                code,
            ]);

        return ReviewText(stack, libraries, signalText, code);
    }

    /// <inheritdoc />
    public ComparisonResult Compare(string leftId, string rightId)
    {
        var left = catalog.GetById(leftId) ?? throw new InvalidOperationException($"Unknown manifest id '{leftId}'.");
        var right = catalog.GetById(rightId) ?? throw new InvalidOperationException($"Unknown manifest id '{rightId}'.");

        var tradeoffs = new[]
        {
            $"{left.DisplayName}: {left.Summary}",
            $"{right.DisplayName}: {right.Summary}",
            $"Choose {left.DisplayName} when you want {string.Join(", ", left.RecommendedPatterns.Take(2))}.",
            $"Choose {right.DisplayName} when you want {string.Join(", ", right.RecommendedPatterns.Take(2))}."
        };

        return new ComparisonResult(
            $"Compared {left.DisplayName} with {right.DisplayName}.",
            left.Id,
            right.Id,
            left.NuGetPackages,
            right.NuGetPackages,
            left.RecommendedPatterns,
            right.RecommendedPatterns,
            tradeoffs);
    }

    /// <inheritdoc />
    public DebugChecklistResult CreateDebugChecklist(string? issue, string? symptoms, string? currentApproach)
    {
        var signalText = string.Join(' ', issue ?? string.Empty, symptoms ?? string.Empty, currentApproach ?? string.Empty);
        var manifestIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "rx-dotnet-core",
            "rx-schedulers",
            "rx-testing"
        };

        AddBySignals(signalText, manifestIds);

        var investigationSteps = new List<string>
        {
            "Confirm the source sequence shape: hot vs cold, finite vs infinite, single-subscriber vs multicast.",
            "Locate every Subscribe call and verify who owns each IDisposable.",
            "Check scheduler boundaries: where does production happen, where do observers run, and where does UI/thread-affinity matter?",
            "Replace wall-clock waits with TestScheduler or controlled clocks before debugging timing behavior.",
            "Log or record OnNext, OnError, OnCompleted, and subscription windows so ordering is explicit."
        };

        var likelyCauses = new List<string>
        {
            "Subscription lifetime is shorter or longer than intended.",
            "A scheduler hop or missing ObserveOn/SubscribeOn is hiding the race.",
            "A Subject<T> or ReplaySubject<T> is retaining or replaying more state than expected."
        };

        var validationSteps = new List<string>
        {
            "Create a minimal reproduction with one source and one observer.",
            "Re-run the scenario under virtual time when timers, debounce, throttle, timeout, or delay are involved.",
            "Assert terminal signals and disposal behavior explicitly, not just value counts."
        };

        if (signalText.Contains("subject", StringComparison.OrdinalIgnoreCase))
        {
            likelyCauses.Add("The chosen subject type may not match replay or completion expectations.");
        }

        if (signalText.Contains("async enumerable", StringComparison.OrdinalIgnoreCase) || signalText.Contains("iasyncenumerable", StringComparison.OrdinalIgnoreCase))
        {
            likelyCauses.Add("The issue may come from using a pull-based abstraction where push-based delivery is required, or vice versa.");
        }

        return new DebugChecklistResult(
            $"Created a debugging checklist for: {issue ?? "reactive pipeline issue"}.",
            investigationSteps,
            likelyCauses.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            validationSteps,
            manifestIds.ToArray());
    }

    /// <inheritdoc />
    public UpgradePlanResult CreateUpgradePlan(UpgradePlanRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var current = string.Join(' ', request.CurrentPackages).ToLowerInvariant();
        var goals = string.Join(' ', request.UpgradeGoals).ToLowerInvariant();
        var manifestIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "reactive-architecture",
            "rx-dotnet-core",
            "rx-testing"
        };

        AddBySignals(string.Join(' ', current, goals, request.Stack ?? string.Empty, request.ProjectType ?? string.Empty), manifestIds);

        var packageActions = new List<string>
        {
            "Align all System.Reactive-related packages before changing behavior.",
            "Introduce Microsoft.Reactive.Testing where timing-dependent tests need deterministic control."
        };
        var codeActions = new List<string>
        {
            "Replace implicit event spaghetti with named observable pipelines at the composition boundary.",
            "Track subscription disposal explicitly and move long-lived pipelines behind clear ownership boundaries.",
            "Reduce general-purpose Subject<T> usage to well-justified multicast or adapter seams."
        };
        var validationSteps = new List<string>
        {
            "Build and test after each migration slice rather than rewriting the whole graph at once.",
            "Add scheduler-controlled tests for debounce, throttle, timeout, retry, and buffering semantics.",
            "Document hot/cold expectations and completion semantics for each migrated pipeline."
        };
        var risks = new List<string>
        {
            "Behavior can change subtly when replacing event handlers with composed observable pipelines.",
            "Subject removal can expose hidden ordering assumptions previously masked by mutable shared state."
        };

        if (current.Contains("thread.sleep", StringComparison.Ordinal) || goals.Contains("test", StringComparison.Ordinal))
        {
            codeActions.Add("Convert sleep-based tests to TestScheduler and recorded-message assertions.");
        }

        if (current.Contains("subject", StringComparison.Ordinal) || goals.Contains("subject", StringComparison.Ordinal))
        {
            packageActions.Add("Audit each Subject<T>, BehaviorSubject<T>, ReplaySubject<T>, and AsyncSubject<T> for required semantics before preserving it.");
            manifestIds.Add("rx-subjects");
        }

        if (goals.Contains("async enumerable", StringComparison.Ordinal) || goals.Contains("iasyncenumerable", StringComparison.Ordinal))
        {
            codeActions.Add("Choose explicitly between IObservable<T> push pipelines and IAsyncEnumerable<T> pull pipelines at each API boundary.");
            manifestIds.Add("system-linq-async");
            manifestIds.Add("ix-dotnet");
        }

        return new UpgradePlanResult(
            $"Created an upgrade plan for {request.Stack ?? request.ProjectType ?? "reactive"} modernization.",
            packageActions.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            codeActions.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            validationSteps,
            risks.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            manifestIds.ToArray());
    }

    /// <inheritdoc />
    public string CreateScaffoldPrompt(RecommendationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var recommendation = Recommend(request);
        var builder = new StringBuilder();
        builder.AppendLine("Generate production-quality reactive .NET code using the embedded offline guidance catalog shipped with this MCP server.");
        builder.AppendLine();
        builder.AppendLine($"Stack: {request.Stack ?? "unspecified"}");
        builder.AppendLine($"Application kind: {request.AppKind ?? "unspecified"}");
        builder.AppendLine($"Features: {Format(request.Features)}");
        builder.AppendLine($"Constraints: {Format(request.Constraints)}");
        builder.AppendLine();
        builder.AppendLine("Packages and libraries to consider:");
        foreach (var package in recommendation.SuggestedPackages)
        {
            builder.AppendLine($"- {package}");
        }

        builder.AppendLine();
        builder.AppendLine("Required patterns:");
        foreach (var pattern in recommendation.RecommendedPatterns.Take(12))
        {
            builder.AppendLine($"- {pattern}");
        }

        builder.AppendLine();
        builder.AppendLine("Avoid these mistakes:");
        foreach (var pattern in recommendation.AvoidPatterns.Take(12))
        {
            builder.AppendLine($"- {pattern}");
        }

        builder.AppendLine();
        builder.AppendLine("Verification checklist:");
        builder.AppendLine("- Subscription ownership and disposal are explicit.");
        builder.AppendLine("- Scheduler boundaries are intentional and testable.");
        builder.AppendLine("- Subject usage is justified, minimal, and semantically correct.");
        builder.AppendLine("- Time-based behavior is validated with TestScheduler or equivalent deterministic control.");
        builder.AppendLine("- Push vs pull boundaries are explicit when IObservable<T> and IAsyncEnumerable<T> coexist.");
        return builder.ToString().TrimEnd();
    }

    private static bool IsTestScenario(string? appKind, IReadOnlyList<string> features, IReadOnlyList<string> existingLibraries)
    {
        var combined = string.Join(' ', appKind ?? string.Empty, string.Join(' ', features), string.Join(' ', existingLibraries));
        return combined.Contains("test", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("scheduler", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("virtual time", StringComparison.OrdinalIgnoreCase);
    }

    private static string Format(IReadOnlyList<string> values) => values.Count == 0 ? "none" : string.Join(", ", values);

    private static IReadOnlyList<string> Merge(IEnumerable<EcosystemManifest> manifests, Func<EcosystemManifest, IEnumerable<string>> selector) =>
        manifests.SelectMany(selector).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    private static SourceLocation BuildFindingLocation(string planText, params string[] probes)
    {
        foreach (var probe in probes)
        {
            var index = planText.IndexOf(probe, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                var line = 1;
                var column = 1;
                for (var i = 0; i < index; i++)
                {
                    if (planText[i] == '\n')
                    {
                        line++;
                        column = 1;
                    }
                    else if (planText[i] != '\r')
                    {
                        column++;
                    }
                }

                var endColumn = column + probe.Length - 1;
                var start = Math.Max(0, index - 40);
                var length = Math.Min(planText.Length - start, probe.Length + 80);
                var detectedPattern = planText.Substring(start, length).Trim();
                return new SourceLocation(detectedPattern, line, line, column, endColumn);
            }
        }

        return new SourceLocation(null, null, null, null, null);
    }

    private static void AddBySignals(string signalText, ISet<string> manifestIds)
    {
        var text = signalText.ToLowerInvariant();

        if (text.Contains("operator", StringComparison.Ordinal) || text.Contains("combine", StringComparison.Ordinal) || text.Contains("merge", StringComparison.Ordinal) || text.Contains("zip", StringComparison.Ordinal) || text.Contains("throttle", StringComparison.Ordinal) || text.Contains("buffer", StringComparison.Ordinal))
        {
            manifestIds.Add("rx-operators");
        }

        if (text.Contains("subject", StringComparison.Ordinal) || text.Contains("multicast", StringComparison.Ordinal) || text.Contains("replay", StringComparison.Ordinal) || text.Contains("behaviorsubject", StringComparison.Ordinal) || text.Contains("asyncsubject", StringComparison.Ordinal))
        {
            manifestIds.Add("rx-subjects");
        }

        if (text.Contains("test", StringComparison.Ordinal) || text.Contains("scheduler", StringComparison.Ordinal) || text.Contains("virtual time", StringComparison.Ordinal) || text.Contains("deterministic", StringComparison.Ordinal) || text.Contains("marble", StringComparison.Ordinal))
        {
            manifestIds.Add("rx-testing");
        }

        if (text.Contains("scheduler", StringComparison.Ordinal)
            || text.Contains("observeon", StringComparison.Ordinal)
            || text.Contains("subscribeon", StringComparison.Ordinal)
            || text.Contains("thread", StringComparison.Ordinal)
            || text.Contains("concurrency", StringComparison.Ordinal)
            || text.Contains("timer", StringComparison.Ordinal)
            || text.Contains("timers", StringComparison.Ordinal)
            || text.Contains("interval", StringComparison.Ordinal)
            || text.Contains("delay", StringComparison.Ordinal)
            || text.Contains("timeout", StringComparison.Ordinal)
            || text.Contains("retry", StringComparison.Ordinal)
            || text.Contains("determinism", StringComparison.Ordinal))
        {
            manifestIds.Add("rx-schedulers");
        }

        if (text.Contains("benchmark", StringComparison.Ordinal) || text.Contains("allocation", StringComparison.Ordinal) || text.Contains("throughput", StringComparison.Ordinal) || text.Contains("performance", StringComparison.Ordinal))
        {
            manifestIds.Add("rx-performance");
        }

        if (text.Contains("iasyncenumerable", StringComparison.Ordinal) || text.Contains("async enumerable", StringComparison.Ordinal) || text.Contains("pull", StringComparison.Ordinal))
        {
            manifestIds.Add("system-linq-async");
            manifestIds.Add("ix-dotnet");
        }

        if (text.Contains("ienumerable", StringComparison.Ordinal) || text.Contains("interactive", StringComparison.Ordinal) || text.Contains("ix", StringComparison.Ordinal))
        {
            manifestIds.Add("ix-dotnet");
        }

        if (text.Contains("iasyncobservable", StringComparison.Ordinal) || text.Contains("async observable", StringComparison.Ordinal) || text.Contains("async observer", StringComparison.Ordinal) || text.Contains("asyncrx", StringComparison.Ordinal))
        {
            manifestIds.Add("asyncrx-dotnet");
        }
    }
}
