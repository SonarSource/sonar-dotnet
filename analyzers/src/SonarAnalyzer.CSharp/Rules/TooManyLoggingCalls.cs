/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using SonarAnalyzer.Common.Walkers;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TooManyLoggingCalls : ParametrizedDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6664";
    private const string MessageFormat = "Reduce the number of {0} logging calls within this code block from {1} to the {2} allowed.";

    private static class CategoryNames
    {
        public const string Debug = "Debug";
        public const string Error = "Error";
        public const string Information = "Information";
        public const string Warning = "Warning";
    }

    private static class DefaultTresholds
    {
        public const int Debug = 4;
        public const int Error = 1;
        public const int Information = 2;
        public const int Warning = 1;
    }

    public static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat, isEnabledByDefault: false);

    private static readonly ImmutableArray<KnownType> LoggerTypes = ImmutableArray.Create(
        KnownType.Castle_Core_Logging_ILogger,
        KnownType.log4net_ILog,
        KnownType.log4net_Util_ILogExtensions,
        KnownType.Microsoft_Extensions_Logging_ILogger,
        KnownType.Microsoft_Extensions_Logging_LoggerExtensions,
        KnownType.NLog_ILogger,
        KnownType.NLog_ILoggerBase,
        KnownType.NLog_ILoggerExtensions,
        KnownType.Serilog_ILogger,
        KnownType.Serilog_Log);

    private static readonly ImmutableArray<LoggingCategory> LoggingCategories = ImmutableArray.Create<LoggingCategory>(
        new(CategoryNames.Debug, ImmutableHashSet.Create("ConditionalDebug", "ConditionalTrace", "Debug", "DebugFormat", "LogDebug", "LogTrace", "Trace", "TraceFormat", "Verbose")),
        new(CategoryNames.Error, ImmutableHashSet.Create("Error", "ErrorFormat", "Fatal", "FatalFormat", "LogCritical", "LogError")),
        new(CategoryNames.Information, ImmutableHashSet.Create("Info", "InfoFormat", "Information", "LogInformation")),
        new(CategoryNames.Warning, ImmutableHashSet.Create("LogWarning", "Warn", "WarnFormat", "Warning")));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    [RuleParameter("debugThreshold", PropertyType.Integer, "The maximum number of DEBUG, TRACE and VERBOSE statements allowed in the same code block.", DefaultTresholds.Debug)]
    public int DebugThreshold { get; set; } = DefaultTresholds.Debug;

    [RuleParameter("errorThreshold", PropertyType.Integer, "The maximum number of ERROR and FATAL statements allowed in the same code block.", DefaultTresholds.Error)]
    public int ErrorThreshold { get; set; } = DefaultTresholds.Error;

    [RuleParameter("informationThreshold", PropertyType.Integer, "The maximum number of INFORMATION statements allowed in the same code block.", DefaultTresholds.Information)]
    public int InformationThreshold { get; set; } = DefaultTresholds.Information;

    [RuleParameter("warningThreshold", PropertyType.Integer, "The maximum number of WARNING statements allowed in the same code block.", DefaultTresholds.Warning)]
    public int WarningThreshold { get; set; } = DefaultTresholds.Warning;

    protected override void Initialize(SonarParametrizedAnalysisContext context) =>
        context.RegisterCompilationStartAction(cc =>
        {
            if (cc.Compilation.ReferencesAny(KnownAssembly.CastleCore, KnownAssembly.Log4Net, KnownAssembly.MicrosoftExtensionsLoggingAbstractions, KnownAssembly.Serilog, KnownAssembly.NLog))
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
            foreach (var loggingCategory in logCallCollector.LoggingInvocations)
            {
                var treshold = Treshold(loggingCategory.Key);
                var invocations = loggingCategory.Value;
                if (invocations.Count > treshold)
                {
                    var primaryLocation = invocations[0].GetLocation();
                    var secondaryLocations = invocations.Skip(1).Select(x => x.GetLocation());
                    context.ReportIssue(Diagnostic.Create(Rule, primaryLocation, secondaryLocations, loggingCategory.Key, invocations.Count, treshold));
                }
            }
        }
    }

    private int Treshold(string category)
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
        private readonly SemanticModel semanticModel;
        private SyntaxNode currentBlock;

        public Dictionary<string, List<InvocationExpressionSyntax>> LoggingInvocations { get; } = [];

        public LoggingCallCollector(SemanticModel semanticModel, SyntaxNode currentBlock)
        {
            this.semanticModel = semanticModel;
            this.currentBlock = currentBlock;
        }

        public override void VisitBlock(BlockSyntax node)
        {
            if (currentBlock == node) // Don't visit nested blocks, that will be done by additional LoggingCallCollector instances.
            {
                currentBlock = node;
                base.VisitBlock(node);
            }
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (IsLoggerMethod(node.GetName())
                && semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol methodSymbol
                && methodSymbol.ContainingType.DerivesFromAny(LoggerTypes)
                && LoggingCategoryName(node, methodSymbol) is { } loggingCategory)
            {
                if (LoggingInvocations.TryGetValue(loggingCategory, out var invocationList))
                {
                    invocationList.Add(node);
                }
                else
                {
                    LoggingInvocations.Add(loggingCategory, [node]);
                }
            }
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            // Skip syntax node to avoid FPs
        }

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            // Skip syntax node to avoid FPs
        }

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            // Skip syntax node to avoid FPs
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
}
