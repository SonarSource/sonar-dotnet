/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using static Roslyn.Utilities.SonarAnalyzer.Shared.LoggingFrameworkMethods;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LoggingArgumentsShouldBePassedCorrectly : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6668";
    private const string MessageFormat = "Logging arguments should be passed to the correct parameter.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly ImmutableArray<KnownType> MicrosoftLoggingExtensionsInvalidTypes =
        ImmutableArray.Create(KnownType.System_Exception, KnownType.Microsoft_Extensions_Logging_LogLevel, KnownType.Microsoft_Extensions_Logging_EventId);
    private static readonly ImmutableArray<KnownType> CastleCoreInvalidTypes = ImmutableArray.Create(KnownType.System_Exception);
    private static readonly ImmutableArray<KnownType> NLogAndSerilogInvalidTypes = ImmutableArray.Create(KnownType.System_Exception, KnownType.Serilog_Events_LogEventLevel, KnownType.NLog_LogLevel);
    private static readonly HashSet<string> LoggingMethodNames = MicrosoftExtensionsLogging
        .Concat(NLogLoggingMethods)
        .Concat(Serilog)
        .Concat(CastleCoreOrCommonCore)
        .ToHashSet();

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var invocation = (InvocationExpressionSyntax)c.Node;
                if (!LoggingMethodNames.Contains(invocation.GetName())
                    || c.Model.GetSymbolInfo(invocation).Symbol is not IMethodSymbol invocationSymbol)
                {
                    return;
                }
                if (invocationSymbol.HasContainingType(KnownType.Microsoft_Extensions_Logging_LoggerExtensions, false))
                {
                    CheckInvalidParams(invocation, invocationSymbol, c, Filter(invocationSymbol, MicrosoftLoggingExtensionsInvalidTypes));
                }
                else if (invocationSymbol.HasContainingType(KnownType.Castle_Core_Logging_ILogger, true))
                {
                    CheckInvalidParams(invocation, invocationSymbol, c, Filter(invocationSymbol, CastleCoreInvalidTypes));
                }
                else if (invocationSymbol.HasContainingType(KnownType.Serilog_ILogger, true)
                         || invocationSymbol.HasContainingType(KnownType.Serilog_Log, false)
                         || invocationSymbol.HasContainingType(KnownType.NLog_ILoggerBase, true)
                         || invocationSymbol.HasContainingType(KnownType.NLog_ILoggerExtensions, false))
                {
                    var knownTypes = Filter(invocationSymbol, NLogAndSerilogInvalidTypes);
                    CheckInvalidParams(invocation, invocationSymbol, c, knownTypes);
                    CheckInvalidTypeParams(invocation, invocationSymbol, c, knownTypes);
                }
            },
            SyntaxKind.InvocationExpression);

    private static void CheckInvalidParams(InvocationExpressionSyntax invocation, IMethodSymbol invocationSymbol, SonarSyntaxNodeReportingContext c, ImmutableArray<KnownType> knownTypes)
    {
        var paramsParameter = invocationSymbol.Parameters.FirstOrDefault(x => x.IsParams);
        if (paramsParameter is null || knownTypes.IsEmpty)
        {
            return;
        }

        var paramsIndex = invocationSymbol.Parameters.IndexOf(paramsParameter);
        var invalidArguments = invocation.ArgumentList.Arguments
            .Where(x => x.GetArgumentIndex() >= paramsIndex && IsInvalidArgument(x, c.Model, knownTypes))
            .ToSecondaryLocations()
            .ToArray();
        if (invalidArguments.Length > 0)
        {
            c.ReportIssue(Rule, invocation.Expression, invalidArguments);
        }
    }

    private static void CheckInvalidTypeParams(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol, SonarSyntaxNodeReportingContext c, ImmutableArray<KnownType> knownTypes)
    {
        if (!knownTypes.IsEmpty && !IsNLogIgnoredOverload(methodSymbol) && methodSymbol.TypeArguments.Any(x => x.DerivesFromAny(knownTypes)))
        {
            var typeParameterNames = methodSymbol.TypeParameters.Select(x => x.MetadataName).ToArray();
            var positions = methodSymbol.ConstructedFrom.Parameters.Where(x => typeParameterNames.Contains(x.Type.MetadataName)).Select(x => methodSymbol.ConstructedFrom.Parameters.IndexOf(x));
            var invalidArguments = InvalidArguments(invocation, c.Model, positions, knownTypes).ToSecondaryLocations();
            c.ReportIssue(Rule, invocation.Expression, invalidArguments);
        }
    }

    private static bool IsNLogIgnoredOverload(IMethodSymbol methodSymbol) =>
        // These overloads are ignored since they will try to convert the T value to an exception.
        MatchesParams(methodSymbol, KnownType.System_Exception)
        || MatchesParams(methodSymbol, KnownType.System_IFormatProvider, KnownType.System_Exception)
        || MatchesParams(methodSymbol, KnownType.NLog_ILogger, KnownType.System_Exception)
        || MatchesParams(methodSymbol, KnownType.NLog_ILogger, KnownType.System_IFormatProvider, KnownType.System_Exception)
        || MatchesParams(methodSymbol, KnownType.NLog_LogLevel, KnownType.System_Exception)
        || MatchesParams(methodSymbol, KnownType.NLog_LogLevel, KnownType.System_IFormatProvider, KnownType.System_Exception);

    private static bool MatchesParams(IMethodSymbol methodSymbol, params KnownType[] knownTypes) =>
        methodSymbol.Parameters.Length == knownTypes.Length
        && !methodSymbol.Parameters.Where((x, index) => !x.Type.DerivesFrom(knownTypes[index])).Any();

    private static IEnumerable<ArgumentSyntax> InvalidArguments(InvocationExpressionSyntax invocation, SemanticModel model, IEnumerable<int> positionsToCheck, ImmutableArray<KnownType> knownTypes) =>
        positionsToCheck
            .Select(x => invocation.ArgumentList.Arguments[x])
            .Where(x => IsInvalidArgument(x, model, knownTypes));

    private static bool IsInvalidArgument(ArgumentSyntax argumentSyntax, SemanticModel model, ImmutableArray<KnownType> knownTypes) =>
        model.GetTypeInfo(argumentSyntax.Expression).Type?.DerivesFromAny(knownTypes) is true;

    // This method filters out the types that the method accepts strongly:
    // logger.Debug(exception, "template", exception)
    //              ^^^^^^^^^ valid
    //                                     ^^^^^^^^^ do not raise
    private static ImmutableArray<KnownType> Filter(IMethodSymbol methodSymbol, ImmutableArray<KnownType> knownTypes) =>
        knownTypes.Where(knownType => !methodSymbol.ConstructedFrom.Parameters.Any(x => x.Type.DerivesFrom(knownType))).ToImmutableArray();
}
