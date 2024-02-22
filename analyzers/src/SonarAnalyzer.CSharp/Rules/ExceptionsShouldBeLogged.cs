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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExceptionsShouldBeLogged : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6667";
    private const string MessageFormat = "Logging in a catch clause should include the exception.";

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
        private static readonly HashSet<string> MicrosoftExtensionsLoggingMethods =
        [
            "Log",
            "LogCritical",
            "LogDebug",
            "LogError",
            "LogInformation",
            "LogTrace",
            "LogWarning"
        ];

        private static readonly HashSet<string> CastleCoreOrCommonCoreLoggingMethods =
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

        private static readonly HashSet<string> Log4NetLoggingMethods =
        [
            "Debug",
            "Error",
            "Fatal",
            "Info",
            "Warn"
        ];

        private static readonly HashSet<string> Log4NetLoggingExtensionMethods =
        [
            "DebugExt",
            "ErrorExt",
            "FatalExt",
            "InfoExt",
            "WarnExt"
        ];

        private static readonly HashSet<string> NLogLoggingMethods =
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

        private static readonly HashSet<string> NLogILoggerBaseLoggingMethods = ["Log"];

        private readonly SemanticModel model;
        private bool isFirstCatchClauseVisited;
        private bool hasWhenFilterWithDeclarations;
        private ISymbol catchException;

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
                catchException = model.GetDeclaredSymbol(node.Declaration);
            }
            base.VisitCatchClause(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (IsLoggingInvocation(node, model))
            {
                var currentException = node.GetArgumentSymbolsDerivedFromKnownType(KnownType.System_Exception, model).FirstOrDefault();
                if (currentException != null && (hasWhenFilterWithDeclarations || currentException.Equals(catchException)))
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

        private static bool IsLoggingInvocation(InvocationExpressionSyntax invocation, SemanticModel model) =>
            IsLoggingInvocation(invocation, model, MicrosoftExtensionsLoggingMethods, KnownType.Microsoft_Extensions_Logging_LoggerExtensions, true)
            || IsLoggingInvocation(invocation, model, CastleCoreOrCommonCoreLoggingMethods, KnownType.Castle_Core_Logging_ILogger, false)
            || IsLoggingInvocation(invocation, model, CastleCoreOrCommonCoreLoggingMethods, KnownType.Common_Logging_ILog, false)
            || IsLoggingInvocation(invocation, model, Log4NetLoggingMethods, KnownType.log4net_ILog, false)
            || IsLoggingInvocation(invocation, model, Log4NetLoggingExtensionMethods, KnownType.log4net_Util_ILogExtensions, true)
            || IsLoggingInvocation(invocation, model, NLogLoggingMethods, KnownType.NLog_ILogger, false)
            || IsLoggingInvocation(invocation, model, NLogILoggerBaseLoggingMethods, KnownType.NLog_ILoggerBase, false);

        private static bool IsLoggingInvocation(InvocationExpressionSyntax invocation, SemanticModel model, ICollection<string> methodNames, KnownType containingType, bool isExtensionMethod) =>
            methodNames.Contains(invocation.GetIdentifier().ToString())
            && model.GetSymbolInfo(invocation).Symbol is { } invocationSymbol
            && HasMatchingContainingType(invocationSymbol, containingType, isExtensionMethod);

        private static bool HasMatchingContainingType(ISymbol symbol, KnownType containingType, bool isExtensionMethod) =>
            isExtensionMethod
                ? symbol.ContainingType.Is(containingType)
                : symbol.ContainingType.DerivesOrImplements(containingType);
    }
}
