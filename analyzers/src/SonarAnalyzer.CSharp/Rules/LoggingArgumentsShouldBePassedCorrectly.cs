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

using static Roslyn.Utilities.SonarAnalyzer.Shared.LoggingFrameworkMethods;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LoggingArgumentsShouldBePassedCorrectly : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6668";
    private const string MessageFormat = "Logging arguments should be passed to the correct parameter.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var invocation = (InvocationExpressionSyntax)c.Node;
                if (!HasLoggingMethodName(invocation)
                    || c.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol invocationSymbol)
                {
                    return;
                }
                if (invocationSymbol.HasContainingType(KnownType.Microsoft_Extensions_Logging_LoggerExtensions, false))
                {
                    var invalidTypes = ImmutableArray.Create(KnownType.System_Exception, KnownType.Microsoft_Extensions_Logging_LogLevel, KnownType.Microsoft_Extensions_Logging_EventId);
                    CheckInvalidParams(invocation, invocationSymbol, c, invalidTypes);
                }
                else if (invocationSymbol.HasContainingType(KnownType.Castle_Core_Logging_ILogger, true))
                {
                    CheckInvalidParams(invocation, invocationSymbol, c, ImmutableArray.Create(KnownType.System_Exception));
                }
                else if (invocationSymbol.HasContainingType(KnownType.Serilog_ILogger, true)
                         || invocationSymbol.HasContainingType(KnownType.Serilog_Log, false)
                         || invocationSymbol.HasContainingType(KnownType.NLog_ILoggerBase, true)
                         || invocationSymbol.HasContainingType(KnownType.NLog_ILoggerExtensions, false))
                {
                    var invalidTypes = ImmutableArray.Create(KnownType.System_Exception, KnownType.Serilog_Events_LogEventLevel, KnownType.NLog_LogLevel);
                    CheckInvalidParams(invocation, invocationSymbol, c, invalidTypes);
                    CheckInvalidTypeParams(invocation, invocationSymbol, c, invalidTypes);
                }
            },
            SyntaxKind.InvocationExpression);

    private static void CheckInvalidParams(InvocationExpressionSyntax invocation, IMethodSymbol invocationSymbol, SonarSyntaxNodeReportingContext c, ImmutableArray<KnownType> knownTypes)
    {
        var paramsParameter = invocationSymbol.Parameters.FirstOrDefault(x => x.IsParams);
        if (paramsParameter is null)
        {
            return;
        }

        var paramsIndex = invocationSymbol.Parameters.IndexOf(paramsParameter);
        var invalidArguments = invocation.ArgumentList.Arguments
            .Where(x => x.GetArgumentIndex() >= paramsIndex
                        && IsInvalidArgument(x, c.SemanticModel, knownTypes))
            .Select(x => x.GetLocation())
            .ToArray();

        if (invalidArguments.Length > 0)
        {
            c.ReportIssue(Diagnostic.Create(Rule, invocation.Expression.GetLocation(), (IEnumerable<Location>)invalidArguments));
        }
    }

    private static void CheckInvalidTypeParams(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol, SonarSyntaxNodeReportingContext c, ImmutableArray<KnownType> knownTypes)
    {
        if (!IsNLogIgnoredOverload(methodSymbol) && methodSymbol.TypeArguments.Any(x => x.DerivesFromAny(knownTypes)))
        {
            var typeParameterNames = methodSymbol.TypeParameters.Select(x => x.MetadataName).ToArray();
            var positions = methodSymbol.ConstructedFrom.Parameters.Where(x => typeParameterNames.Contains(x.Type.MetadataName)).Select(x => methodSymbol.ConstructedFrom.Parameters.IndexOf(x));
            var invalidArguments = InvalidArguments(invocation, c.SemanticModel, positions, knownTypes).Select(x => x.GetLocation());
            c.ReportIssue(Diagnostic.Create(Rule, invocation.Expression.GetLocation(), invalidArguments));
        }
    }

    private static bool HasLoggingMethodName(InvocationExpressionSyntax invocation) =>
        MicrosoftExtensionsLogging.Contains(invocation.GetName())
        || NLogLoggingMethods.Contains(invocation.GetName())
        || Serilog.Contains(invocation.GetName())
        || CastleCoreOrCommonCore.Contains(invocation.GetName());

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
}
