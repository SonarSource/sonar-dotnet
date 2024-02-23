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
public sealed class ExceptionsShouldBeLogged : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6667";
    private const string MessageFormat = "Logging in a catch clause should pass the caught exception as a parameter.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var catchClauseSyntax = (CatchClauseSyntax)c.Node;
                var walker = new CatchLoggingInvocationWalker(c.SemanticModel);
                if (walker.SafeVisit(catchClauseSyntax) && !walker.IsExceptionLogged && walker.LoggingInvocations.Any())
                {
                    var primaryLocation = walker.LoggingInvocations[0].GetLocation();
                    var additionalLocations = walker.LoggingInvocations.Skip(1).Select(x => x.GetLocation());
                    c.ReportIssue(Diagnostic.Create(Rule, primaryLocation, additionalLocations));
                }
            },
            SyntaxKind.CatchClause);

    // The walker is used to:
    // - visit the catch clause (all the nested catch clauses are skipped; they will be visited independently)
    // - save the declared exception
    // - find all the logging invocations and check if the exception is logged
    // - if the exception is logged, it will stop looking for the other invocations and set IsExceptionLogged to true
    // - if the exception is not logged, it will visit all the invocations
    private sealed class CatchLoggingInvocationWalker : SafeCSharpSyntaxWalker
    {
        private readonly SemanticModel model;
        private bool isFirstCatchClauseVisited;
        private bool hasWhenFilterWithDeclarations;
        private ISymbol caughtException;

        public bool IsExceptionLogged { get; private set; }
        public readonly List<InvocationExpressionSyntax> LoggingInvocations = new();

        public CatchLoggingInvocationWalker(SemanticModel model)
        {
            this.model = model;
        }

        public override void Visit(SyntaxNode node)
        {
            if (!IsExceptionLogged)
            {
                base.Visit(node);
            }
        }

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
                caughtException = model.GetDeclaredSymbol(node.Declaration);
            }
            base.VisitCatchClause(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (IsLoggingInvocation(node, model))
            {
                var currentException = GetArgumentSymbolsDerivedFromException(node, model).FirstOrDefault();
                if (currentException != null && (hasWhenFilterWithDeclarations || currentException.Equals(caughtException)))
                {
                    IsExceptionLogged = true;
                    return;
                }
                else
                {
                    LoggingInvocations.Add(node);
                }
            }
            base.VisitInvocationExpression(node);
        }

        private static IEnumerable<ISymbol> GetArgumentSymbolsDerivedFromException(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            var expressions = invocation.ArgumentList.Arguments
                .Where(argument => semanticModel.GetTypeInfo(argument.Expression).Type.DerivesFrom(KnownType.System_Exception))
                .Select(x => x.Expression);
            foreach (var expression in expressions)
            {
                if (expression is MemberAccessExpressionSyntax memberAccess && memberAccess.NameIs("InnerException"))
                {
                    yield return semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
                }
                yield return semanticModel.GetSymbolInfo(expression).Symbol;
            }
        }

        private static bool IsLoggingInvocation(InvocationExpressionSyntax invocation, SemanticModel model) =>
            IsLoggingInvocation(invocation, model, MicrosoftExtensionsLogging, KnownType.Microsoft_Extensions_Logging_LoggerExtensions, false)
            || IsLoggingInvocation(invocation, model, CastleCoreOrCommonCore, KnownType.Castle_Core_Logging_ILogger, true)
            || IsLoggingInvocation(invocation, model, CastleCoreOrCommonCore, KnownType.Common_Logging_ILog, true)
            || IsLoggingInvocation(invocation, model, Log4NetILog, KnownType.log4net_ILog, true)
            || IsLoggingInvocation(invocation, model, Log4NetILogExtensions, KnownType.log4net_Util_ILogExtensions, false)
            || IsLoggingInvocation(invocation, model, NLogILogger, KnownType.NLog_ILogger, true)
            || IsLoggingInvocation(invocation, model, NLogILoggerBase, KnownType.NLog_ILoggerBase, true);

        private static bool IsLoggingInvocation(InvocationExpressionSyntax invocation, SemanticModel model, ICollection<string> methodNames, KnownType containingType, bool checkDerivedTypes) =>
            methodNames.Contains(invocation.GetIdentifier().ToString())
            && model.GetSymbolInfo(invocation).Symbol is { } invocationSymbol
            && invocationSymbol.HasContainingType(containingType, checkDerivedTypes);
    }
}
