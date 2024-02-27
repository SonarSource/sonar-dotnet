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
public sealed class ExceptionsShouldBePassedCorrectly : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6668";
    private const string MessageFormat = "Logging arguments should be passed to the correct parameter.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var invocation = (InvocationExpressionSyntax)c.Node;
                if (NLogOrSerilogInvocationSymbol(invocation, c.SemanticModel) is { } nLogInvocationSymbol)
                {
                    VisitNLogOrSerilogInvocation(invocation, nLogInvocationSymbol, c);
                }
                else if (LoggingInvocationSymbol(invocation, c.SemanticModel) is { } invocationSymbol)
                {
                    VisitInvocation(invocation, invocationSymbol, c);
                }
            },
            SyntaxKind.InvocationExpression);

    private static void VisitNLogOrSerilogInvocation(InvocationExpressionSyntax invocation, IMethodSymbol invocationSymbol, SonarSyntaxNodeReportingContext c)
    {
        if (IsNLogIgnoredOverload(invocationSymbol)
            // The overload with the exception as the first argument is compliant.
            || (invocationSymbol.Parameters.Length > 0 && invocationSymbol.Parameters[0].Type.DerivesFrom(KnownType.System_Exception)))
        {
            return;
        }

        if (invocationSymbol.TypeArguments.Any(x => x.DerivesFrom(KnownType.System_Exception)))
        {
            foreach (var wrongArgument in ExceptionArguments(invocation, c.SemanticModel))
            {
                c.ReportIssue(Diagnostic.Create(Rule, wrongArgument.GetLocation()));
            }
        }
        else
        {
            VisitInvocation(invocation, invocationSymbol, c);
        }
    }

    private static void VisitInvocation(InvocationExpressionSyntax invocation, IMethodSymbol invocationSymbol, SonarSyntaxNodeReportingContext c)
    {
        var exceptionParameterIndex = ExceptionParameterIndex(invocationSymbol);
        var exceptionArguments = ExceptionArguments(invocation, c.SemanticModel).ToArray();
        // Do not raise if there is at least one argument in the right place.
        if (Array.Exists(exceptionArguments, x => x.GetArgumentIndex() == exceptionParameterIndex))
        {
            return;
        }
        foreach (var wrongArgument in exceptionArguments)
        {
            c.ReportIssue(Diagnostic.Create(Rule, wrongArgument.GetLocation()));
        }
    }

    private static bool IsNLogIgnoredOverload(IMethodSymbol methodSymbol) =>
        // These overloads are ignored since they will try to convert the T value to an exception.
        MatchesParams(methodSymbol, KnownType.System_Exception)
        || MatchesParams(methodSymbol, KnownType.System_IFormatProvider, KnownType.System_Exception)
        || MatchesParams(methodSymbol, KnownType.NLog_ILogger, KnownType.System_Exception)
        || MatchesParams(methodSymbol, KnownType.NLog_ILogger, KnownType.System_IFormatProvider, KnownType.System_Exception);

    private static bool MatchesParams(IMethodSymbol methodSymbol, params KnownType[] knownTypes) =>
        methodSymbol.Parameters.Length == knownTypes.Length
        && !methodSymbol.Parameters.Where((x, index) => !x.Type.DerivesFrom(knownTypes[index])).Any();

    private static IMethodSymbol NLogOrSerilogInvocationSymbol(InvocationExpressionSyntax invocation, SemanticModel model) =>
        (NLogLoggingMethods.Contains(invocation.GetName()) || Serilog.Contains(invocation.GetName()))
        && model.GetSymbolInfo(invocation).Symbol is IMethodSymbol symbol
        && (symbol.HasContainingType(KnownType.NLog_ILoggerExtensions, false)
            || symbol.HasContainingType(KnownType.NLog_ILoggerBase, true)
            || symbol.HasContainingType(KnownType.Serilog_ILogger, true)
            || symbol.HasContainingType(KnownType.Serilog_Log, false))
            ? symbol
            : null;

    private static IMethodSymbol LoggingInvocationSymbol(InvocationExpressionSyntax invocation, SemanticModel model) =>
        // The implementation is simplified for the sake of performance, to retrieve the symbol info only once.
        // It will first check if the method is a logging method (from any framework), and then if it is part of a logging type.
        IsLoggingMethodName(invocation.GetName())
        && model.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
        && IsLoggingType(methodSymbol)
            ? methodSymbol
            : null;

    private static bool IsLoggingMethodName(string methodName) =>
        MicrosoftExtensionsLogging.Contains(methodName)
        || CastleCoreOrCommonCore.Contains(methodName);

    private static bool IsLoggingType(ISymbol symbol) =>
        symbol.HasContainingType(KnownType.Microsoft_Extensions_Logging_LoggerExtensions, false)
        || symbol.HasContainingType(KnownType.Castle_Core_Logging_ILogger, true);

    private static IEnumerable<ArgumentSyntax> ExceptionArguments(InvocationExpressionSyntax invocation, SemanticModel model) =>
        invocation.ArgumentList.Arguments.Where(x => model.GetTypeInfo(x.Expression).Type.DerivesFrom(KnownType.System_Exception));

    private static int ExceptionParameterIndex(IMethodSymbol invocationSymbol)
    {
        var exceptionParameter = invocationSymbol.Parameters.FirstOrDefault(x => x.Type.DerivesFrom(KnownType.System_Exception));
        return invocationSymbol.Parameters.IndexOf(exceptionParameter);
    }
}
