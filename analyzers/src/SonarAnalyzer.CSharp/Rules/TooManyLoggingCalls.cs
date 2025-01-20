/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TooManyLoggingCalls : ParametrizedDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6664";
    private const string MessageFormat = "Reduce the number of {0} logging calls within this code block from {1} to the {2} allowed.";

    public static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat, isEnabledByDefault: false);

    private static readonly KnownAssembly[] SupportedLoggingLibraries =
    [
        KnownAssembly.MicrosoftExtensionsLoggingAbstractions,
        KnownAssembly.NLog,
        KnownAssembly.Serilog,
        KnownAssembly.Log4Net,
        KnownAssembly.CastleCore
    ];

    private static readonly ImmutableArray<KnownType> LoggerTypes = ImmutableArray.Create(
        KnownType.Microsoft_Extensions_Logging_ILogger,
        KnownType.Microsoft_Extensions_Logging_LoggerExtensions,
        KnownType.NLog_ILogger,
        KnownType.NLog_ILoggerBase,
        KnownType.NLog_ILoggerExtensions,
        KnownType.Serilog_ILogger,
        KnownType.Serilog_Log,
        KnownType.log4net_ILog,
        KnownType.log4net_Util_ILogExtensions,
        KnownType.Castle_Core_Logging_ILogger);

    private static readonly ImmutableArray<LoggingCategory> LoggingCategories = ImmutableArray.Create<LoggingCategory>(
        new(CategoryNames.Debug, ImmutableHashSet.Create("ConditionalDebug", "ConditionalTrace", "Debug", "DebugFormat", "LogDebug", "LogTrace", "Trace", "TraceFormat", "Verbose")),
        new(CategoryNames.Error, ImmutableHashSet.Create("Error", "ErrorFormat", "Fatal", "FatalFormat", "LogCritical", "LogError")),
        new(CategoryNames.Information, ImmutableHashSet.Create("Info", "InfoFormat", "Information", "LogInformation")),
        new(CategoryNames.Warning, ImmutableHashSet.Create("LogWarning", "Warn", "WarnFormat", "Warning")));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    [RuleParameter("debugThreshold", PropertyType.Integer, "The maximum number of DEBUG, TRACE and VERBOSE statements allowed in the same code block.", DefaultThresholds.Debug)]
    public int DebugThreshold { get; set; } = DefaultThresholds.Debug;

    [RuleParameter("errorThreshold", PropertyType.Integer, "The maximum number of ERROR and FATAL statements allowed in the same code block.", DefaultThresholds.Error)]
    public int ErrorThreshold { get; set; } = DefaultThresholds.Error;

    [RuleParameter("informationThreshold", PropertyType.Integer, "The maximum number of INFORMATION statements allowed in the same code block.", DefaultThresholds.Information)]
    public int InformationThreshold { get; set; } = DefaultThresholds.Information;

    [RuleParameter("warningThreshold", PropertyType.Integer, "The maximum number of WARNING statements allowed in the same code block.", DefaultThresholds.Warning)]
    public int WarningThreshold { get; set; } = DefaultThresholds.Warning;

    protected override void Initialize(SonarParametrizedAnalysisContext context) =>
        context.RegisterCompilationStartAction(cc =>
        {
            if (cc.Compilation.ReferencesAny(SupportedLoggingLibraries))
            {
                cc.RegisterNodeAction(Process, SyntaxKind.Block, SyntaxKind.CompilationUnit);
            }
        });

    private void Process(SonarSyntaxNodeReportingContext context)
    {
        var node = context.Node;
        if (IsBlockNodeSupported(node))
        {
            var logCallCollector = new LoggingCallCollector(context.SemanticModel, node);
            logCallCollector.Visit(node);
            foreach (var group in logCallCollector.GroupedLoggingInvocations)
            {
                var threshold = Threshold(group.Key);
                var invocations = group.Value;
                if (invocations.Count > threshold)
                {
                    var primaryLocation = invocations[0].GetLocation();
                    string[] messageParams = [group.Key, invocations.Count.ToString(), threshold.ToString()];
                    var secondaryLocations = invocations.Skip(1).ToSecondaryLocations(MessageFormat, messageParams);
                    context.ReportIssue(Rule, primaryLocation, secondaryLocations, messageParams);
                }
            }
        }
    }

    private int Threshold(string category)
    {
        var threshold = category switch
        {
            CategoryNames.Debug => DebugThreshold,
            CategoryNames.Error => ErrorThreshold,
            CategoryNames.Warning => WarningThreshold,
            _ => InformationThreshold
        };
        return Math.Max(threshold, 0);
    }

    private static bool IsBlockNodeSupported(SyntaxNode node) =>
        node.Kind() == SyntaxKind.Block
        || (node.Kind() == SyntaxKind.CompilationUnit && node.ChildNodes().OfType<GlobalStatementSyntax>().Any());  // for top-level statements

    private sealed class LoggingCallCollector : SafeCSharpSyntaxWalker
    {
        private readonly SemanticModel model;
        private readonly SyntaxNode currentBlock;

        public Dictionary<string, List<InvocationExpressionSyntax>> GroupedLoggingInvocations { get; } = [];

        public LoggingCallCollector(SemanticModel model, SyntaxNode currentBlock)
        {
            this.model = model;
            this.currentBlock = currentBlock;
        }

        public override void VisitBlock(BlockSyntax node)
        {
            if (currentBlock == node) // Don't visit nested blocks, that will be done by additional LoggingCallCollector instances.
            {
                base.VisitBlock(node);
            }
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (IsLoggerMethod(node.GetName())
                && model.GetSymbolInfo(node).Symbol is IMethodSymbol methodSymbol
                && methodSymbol.ContainingType.DerivesOrImplementsAny(LoggerTypes)
                && LoggingCategoryName(node, methodSymbol) is { } loggingCategory)
            {
                if (GroupedLoggingInvocations.TryGetValue(loggingCategory, out var invocationList))
                {
                    invocationList.Add(node);
                }
                else
                {
                    GroupedLoggingInvocations.Add(loggingCategory, [node]);
                }
            }
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            // Skip syntax node to avoid FPs
        }

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            // Skip syntax node to avoid FPs when the lambda body is a single instruction rather than a block
        }

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            // Skip syntax node to avoid FPs when the lambda body is a single instruction rather than a block
        }

        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            // Skip syntax node to avoid FPs
        }

        private static bool IsLoggerMethod(string methodName) =>
            methodName is "Log" or "Write"
            || LoggingCategories.Any(x => x.LoggingMethods.Contains(methodName));

        private static string LoggingCategoryName(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol) =>
            LoggingCategories.FirstOrDefault(x => x.LoggingMethods.Contains(invocation.GetName()))?.CategoryName
                ?? CategoryNameForLogLevel(invocation, methodSymbol);

        private static string CategoryNameForLogLevel(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol) =>
                LogLevelArgumentName(invocation, methodSymbol) is { } logLevel
                    ? logLevel switch
                    {
                        "Debug" or "Trace" or "Verbose" => CategoryNames.Debug,
                        "Critical" or "Error" or "Fatal" => CategoryNames.Error,
                        "Information" => CategoryNames.Information,
                        "Warning" => CategoryNames.Warning,
                        _ => null
                    }
                    : null;

        private static string LogLevelArgumentName(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol)
        {
            var lookup = new CSharpMethodParameterLookup(invocation.ArgumentList, methodSymbol);
            return lookup.TryGetSyntax("logLevel", out var arguments) || lookup.TryGetSyntax("level", out arguments)
                ? arguments[0].GetName()
                : null;
        }
    }

    private sealed record LoggingCategory(string CategoryName, ImmutableHashSet<string> LoggingMethods);

    private static class DefaultThresholds
    {
        public const int Debug = 4;
        public const int Information = 2;
        public const int Warning = 1;
        public const int Error = 1;
    }

    private static class CategoryNames
    {
        public const string Debug = "Debug";
        public const string Information = "Information";
        public const string Warning = "Warning";
        public const string Error = "Error";
    }
}
