/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

namespace SonarAnalyzer.CSharp.Walkers;

// The walker is used to:
// - visit the catch clause (all the nested catch clauses are skipped; they will be visited independently)
// - save the declared exception
// - save the logging invocation that uses the exception
// - find all the logging invocations and check if the exception is logged
// - if the exception is logged, it will stop looking for the other invocations and set IsExceptionLogged to true
// - if the exception is not logged, it will visit all the invocations
public class CatchLoggingInvocationWalker : SafeCSharpSyntaxWalker
{
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

    private bool isFirstCatchClauseVisited;
    private bool hasWhenFilterWithDeclarations;

    public bool IsExceptionLogged { get; private set; }
    public InvocationExpressionSyntax LoggingInvocationWithException { get; private set; }
    public IList<InvocationExpressionSyntax> LoggingInvocationsWithoutException { get; } = [];

    internal SemanticModel Model { get; }
    internal ISymbol CaughtException { get; private set; }

    public CatchLoggingInvocationWalker(SemanticModel model) =>
        Model = model;

    public override void VisitCatchClause(CatchClauseSyntax node)
    {
        // We want to look for logging invocations only in the main catch clause.
        if (isFirstCatchClauseVisited)
        {
            return;
        }

        isFirstCatchClauseVisited = true;
        hasWhenFilterWithDeclarations = node.Filter is not null && node.Filter.DescendantNodes().Any(DeclarationPatternSyntaxWrapper.IsInstance);
        if (node.Declaration is not null && !node.Declaration.Identifier.IsKind(SyntaxKind.None))
        {
            CaughtException = Model.GetDeclaredSymbol(node.Declaration);
        }
        base.VisitCatchClause(node);
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (!IsExceptionLogged && IsLoggingInvocation(node, Model))
        {
            if (ArgumentSymbolDerivedFromException(node, Model) is { } currentException
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

    // A null expression is a bare "throw;", which always rethrows the caught exception.
    internal bool RethrowsCaughtException(ExpressionSyntax expression) =>
        expression is null
        || (Model.GetSymbolInfo(expression).Symbol is { } thrown
            && (Equals(thrown, CaughtException) || IsCaughtExceptionAlias(thrown)));

    private bool IsCaughtExceptionAlias(ISymbol symbol) =>
        CaughtException is not null
        && symbol.DeclaringSyntaxReferences is { Length: 1 } declarations
        && declarations[0].GetSyntax().AncestorsAndSelf().FirstOrDefault(DeclarationPatternSyntaxWrapper.IsInstance) is { Parent: { } isPattern }
        && IsPatternExpressionSyntaxWrapper.IsInstance(isPattern)
        && Equals(Model.GetSymbolInfo(((IsPatternExpressionSyntaxWrapper)isPattern).Expression).Symbol, CaughtException);

    private static ISymbol ArgumentSymbolDerivedFromException(InvocationExpressionSyntax invocation, SemanticModel model) =>
        invocation.ArgumentList.Arguments
            .Where(x => model.GetTypeInfo(x.Expression).Type.DerivesFrom(KnownType.System_Exception))
            .Select(x => x.Expression is MemberAccessExpressionSyntax memberAccess && memberAccess.NameIs("InnerException")
                            ? model.GetSymbolInfo(memberAccess.Expression).Symbol
                            : model.GetSymbolInfo(x.Expression).Symbol)
            .FirstOrDefault();

    private static bool IsLoggingInvocation(InvocationExpressionSyntax invocation, SemanticModel model) =>
        LoggingInvocationDescriptors.Any(x => IsLoggingInvocation(invocation, model, x.MethodNames, x.ContainingType, x.CheckDerivedTypes));

    private static bool IsLoggingInvocation(InvocationExpressionSyntax invocation, SemanticModel model, ICollection<string> methodNames, KnownType containingType, bool checkDerivedTypes) =>
        methodNames.Contains(invocation.GetIdentifier().ToString())
        && model.GetSymbolInfo(invocation).Symbol is { } invocationSymbol
        && invocationSymbol.HasContainingType(containingType, checkDerivedTypes);

    private record struct LoggingInvocationDescriptor(HashSet<string> MethodNames, KnownType ContainingType, bool CheckDerivedTypes);
}
