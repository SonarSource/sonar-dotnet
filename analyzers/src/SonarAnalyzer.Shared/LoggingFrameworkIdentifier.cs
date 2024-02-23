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
}
