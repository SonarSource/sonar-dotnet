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

using SonarAnalyzer.CSharp.Walkers;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExceptionsShouldBeLoggedOrThrown : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S2139";
    private const string MessageFormat = "Either log this exception and handle it, or rethrow it with some contextual information.";
    private const string LoggingStatementMessage = "Logging statement.";
    private const string ThrownExceptionMessage = "Thrown exception.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
    private static readonly KnownAssembly[] SupportedLoggingFrameworks = [
        KnownAssembly.MicrosoftExtensionsLoggingAbstractions,
        KnownAssembly.Log4Net,
        KnownAssembly.NLog,
        KnownAssembly.CastleCore,
        KnownAssembly.CommonLoggingCore,
        KnownAssembly.Serilog];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(cc =>
            {
                if (cc.Compilation.ReferencesAny(SupportedLoggingFrameworks))
                {
                     cc.RegisterNodeAction(c =>
                         {
                             var catchClauseSyntax = (CatchClauseSyntax)c.Node;
                             var walker = new LoggingInvocationWalker(c.Model);
                             if (catchClauseSyntax.Declaration?.Identifier is { } exceptionIdentifier // there is an exception to log
                                 && catchClauseSyntax.DescendantNodes().Any(x => x.Kind() is SyntaxKind.ThrowStatement or SyntaxKindEx.ThrowExpression) // and a throw statement (preliminary check)
                                 && walker.SafeVisit(catchClauseSyntax)
                                 && walker.IsExceptionLogged
                                 && walker.ThrowNode is { } throwStatement)
                             {
                                 var secondaryLocations = new List<SecondaryLocation>
                                 {
                                     new(walker.LoggingInvocationWithException.GetLocation(), LoggingStatementMessage),
                                     new(throwStatement.GetLocation(), ThrownExceptionMessage)
                                 };
                                 c.ReportIssue(Rule, exceptionIdentifier, secondaryLocations);
                             }
                         },
                         SyntaxKind.CatchClause);
                }
            });

    private sealed class LoggingInvocationWalker(SemanticModel model) : CatchLoggingInvocationWalker(model)
    {
        public SyntaxNode ThrowNode { get; private set; }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            // Skip processing to avoid false positives.
        }

        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            // Skip processing to avoid false positives.
        }

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            // Skip processing to avoid false positives.
        }

        public override void Visit(SyntaxNode node)
        {
            if (node.IsKind(SyntaxKind.CoalesceExpression))
            {
                return;
            }
            base.Visit(node);
        }

        public override void VisitThrowStatement(ThrowStatementSyntax node)
        {
            if (ThrowNode == null
                && RethrowsCaughtException(node.Expression))
            {
                ThrowNode = node;
            }
            base.VisitThrowStatement(node);
        }

        private bool RethrowsCaughtException(ExpressionSyntax expression) =>
            expression is null || Equals(Model.GetSymbolInfo(expression).Symbol, CaughtException);
    }
}
