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

namespace SonarAnalyzer.CSharp.Walkers;

// The walker is used to:
// - visit the catch clause (all the nested catch clauses are skipped; they will be visited independently)
// - save the declared exception
// - save the logging invocation that uses the exception
// - find all the logging invocations and check if the exception is logged
// - if the exception is logged, it will stop looking for the other invocations and set IsExceptionLogged to true
// - if the exception is not logged, it will visit all the invocations
public class CatchLoggingInvocationWalker(SemanticModel model) : SafeCSharpSyntaxWalker
{
    internal readonly SemanticModel Model = model;

    private bool isFirstCatchClauseVisited;
    private bool hasWhenFilterWithDeclarations;

    internal ISymbol CaughtException;

    public bool IsExceptionLogged { get; private set; }
    public InvocationExpressionSyntax LoggingInvocationWithException { get; private set; }
    public IList<InvocationExpressionSyntax> LoggingInvocationsWithoutException { get; } = new List<InvocationExpressionSyntax>();

    private static readonly ImmutableArray<LoggingInvocationDescriptor> LoggingInvocationDescriptors = ImmutableArray.Create(
        new LoggingInvocationDescriptor(MicrosoftExtensionsLogging, KnownType.Microsoft_Extensions_Logging_LoggerExtensions, false),
        new LoggingInvocationDescriptor(CastleCoreOrCommonCore, KnownType.Castle_Core_Logging_ILogger, true),
        new LoggingInvocationDescriptor(CastleCoreOrCommonCore, KnownType.Common_Logging_ILog, true),
        new LoggingInvocationDescriptor(Log4NetILog, KnownType.log4net_ILog, true),
        new LoggingInvocationDescriptor(Log4NetILogExtensions, KnownType.log4net_Util_ILogExtensions, false),
        new LoggingInvocationDescriptor(NLogLoggingMethods, KnownType.NLog_ILogger, true),
        new LoggingInvocationDescriptor(NLogLoggingMethods, KnownType.NLog_ILoggerExtensions, false),
        new LoggingInvocationDescriptor(NLogILoggerBase, KnownType.NLog_ILoggerBase, true),
        new LoggingInvocationDescriptor(Serilog, KnownType.Serilog_ILogger, true),
        new LoggingInvocationDescriptor(Serilog, KnownType.Serilog_Log, false));

    public override void VisitCatchClause(CatchClauseSyntax node)
    {
        // We want to look for logging invocations only in the main catch clause.
        if (isFirstCatchClauseVisited)
        {
            return;
        }

        isFirstCatchClauseVisited = true;
        hasWhenFilterWithDeclarations = node.Filter != null && node.Filter.DescendantNodes().Any(DeclarationPatternSyntaxWrapper.IsInstance);
        if (node.Declaration != null && !node.Declaration.Identifier.IsKind(SyntaxKind.None))
        {
            CaughtException = Model.GetDeclaredSymbol(node.Declaration);
        }
        base.VisitCatchClause(node);
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (!IsExceptionLogged && IsLoggingInvocation(node, Model))
        {
            if (GetArgumentSymbolDerivedFromException(node, Model) is { } currentException
                && (hasWhenFilterWithDeclarations || currentException.Equals(CaughtException)))
            {
                IsExceptionLogged = true;
                LoggingInvocationWithException = node;
                return;
            }
            else
            {
                LoggingInvocationsWithoutException.Add(node);
            }
        }
        base.VisitInvocationExpression(node);
    }

    public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
    {
        // Skip processing to avoid false positives.
    }

    public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
    {
        // Skip processing to avoid false positives.
    }

    private static ISymbol GetArgumentSymbolDerivedFromException(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
        invocation.ArgumentList.Arguments
            .Where(x => semanticModel.GetTypeInfo(x.Expression).Type.DerivesFrom(KnownType.System_Exception))
            .Select(x => x.Expression is MemberAccessExpressionSyntax memberAccess && memberAccess.NameIs("InnerException")
                ? semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol
                : semanticModel.GetSymbolInfo(x.Expression).Symbol)
            .FirstOrDefault();

    private static bool IsLoggingInvocation(InvocationExpressionSyntax invocation, SemanticModel model) =>
        LoggingInvocationDescriptors.Any(x => IsLoggingInvocation(invocation, model, x.MethodNames, x.ContainingType, x.CheckDerivedTypes));

    private static bool IsLoggingInvocation(InvocationExpressionSyntax invocation, SemanticModel model, ICollection<string> methodNames, KnownType containingType, bool checkDerivedTypes) =>
        methodNames.Contains(invocation.GetIdentifier().ToString())
        && model.GetSymbolInfo(invocation).Symbol is { } invocationSymbol
        && invocationSymbol.HasContainingType(containingType, checkDerivedTypes);

    private record struct LoggingInvocationDescriptor(HashSet<string> MethodNames, KnownType ContainingType, bool CheckDerivedTypes);
}
