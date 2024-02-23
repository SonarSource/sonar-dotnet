namespace Roslyn.Utilities.SonarAnalyzer.Shared;

public static class LoggingFrameworkIdentifier
{
    public static readonly HashSet<string> MicrosoftExtensionsLoggingMethods =
    [
        "Log",
        "LogCritical",
        "LogDebug",
        "LogError",
        "LogInformation",
        "LogTrace",
        "LogWarning"
    ];

    public static readonly HashSet<string> CastleCoreOrCommonCoreLoggingMethods =
    [
        "Debug",
        "DebugFormat",
        "Error",
        "ErrorFormat",
        "Fatal",
        "FatalFormat",
        "Info",
        "InfoFormat",
        "Trace",
        "TraceFormat",
        "Warn",
        "WarnFormat"
    ];

    public static readonly HashSet<string> Log4NetLoggingMethods =
    [
        "Debug",
        "Error",
        "Fatal",
        "Info",
        "Warn"
    ];

    public static readonly HashSet<string> Log4NetLoggingExtensionMethods =
    [
        "DebugExt",
        "ErrorExt",
        "FatalExt",
        "InfoExt",
        "WarnExt"
    ];

    public static readonly HashSet<string> NLogLoggingMethods =
    [
        "Debug",
        "ConditionalDebug",
        "Error",
        "Fatal",
        "Info",
        "Trace",
        "ConditionalTrace",
        "Warn"
    ];

    public static readonly HashSet<string> NLogILoggerBaseLoggingMethods = ["Log"];

    public static bool IsLoggingInvocation(InvocationExpressionSyntax invocation, SemanticModel model) =>
        IsLoggingInvocation(invocation, model, MicrosoftExtensionsLoggingMethods, KnownType.Microsoft_Extensions_Logging_LoggerExtensions, true)
        || IsLoggingInvocation(invocation, model, CastleCoreOrCommonCoreLoggingMethods, KnownType.Castle_Core_Logging_ILogger, false)
        || IsLoggingInvocation(invocation, model, CastleCoreOrCommonCoreLoggingMethods, KnownType.Common_Logging_ILog, false)
        || IsLoggingInvocation(invocation, model, Log4NetLoggingMethods, KnownType.log4net_ILog, false)
        || IsLoggingInvocation(invocation, model, Log4NetLoggingExtensionMethods, KnownType.log4net_Util_ILogExtensions, true)
        || IsLoggingInvocation(invocation, model, NLogLoggingMethods, KnownType.NLog_ILogger, false)
        || IsLoggingInvocation(invocation, model, NLogILoggerBaseLoggingMethods, KnownType.NLog_ILoggerBase, false);

    public static bool IsLoggingInvocation(InvocationExpressionSyntax invocation, SemanticModel model, ICollection<string> methodNames, KnownType containingType, bool isExtensionMethod) =>
        methodNames.Contains(invocation.GetIdentifier().ToString())
        && model.GetSymbolInfo(invocation).Symbol is { } invocationSymbol
        && HasMatchingContainingType(invocationSymbol, containingType, isExtensionMethod);

    private static bool HasMatchingContainingType(ISymbol symbol, KnownType containingType, bool isExtensionMethod) =>
        isExtensionMethod
            ? symbol.ContainingType.Is(containingType)
            : symbol.ContainingType.DerivesOrImplements(containingType);
}
