namespace Roslyn.Utilities.SonarAnalyzer.Shared;

public static class LoggingFrameworkMethods
{
    public static readonly HashSet<string> MicrosoftExtensionsLogging =
    [
        "Log",
        "LogCritical",
        "LogDebug",
        "LogError",
        "LogInformation",
        "LogTrace",
        "LogWarning"
    ];

    public static readonly HashSet<string> CastleCoreOrCommonCore =
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

    public static readonly HashSet<string> Log4NetILog =
    [
        "Debug",
        "Error",
        "Fatal",
        "Info",
        "Warn"
    ];

    public static readonly HashSet<string> Log4NetILogExtensions =
    [
        "DebugExt",
        "ErrorExt",
        "FatalExt",
        "InfoExt",
        "WarnExt"
    ];

    public static readonly HashSet<string> Serilog =
    [
        "Debug",
        "Error",
        "Information",
        "Fatal",
        "Warning",
        "Write",
        "Verbose",
    ];

    public static readonly HashSet<string> NLogILogger =
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

    public static readonly HashSet<string> NLogILoggerExtensions =
    [
        "ConditionalTrace",
        "ConditionalDebug",
    ];

    public static readonly HashSet<string> NLogILoggerBase = ["Log"];
}
