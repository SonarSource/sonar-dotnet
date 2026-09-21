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

using SonarAnalyzer.CSharp.Walkers;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExceptionsShouldBeLogged : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6667";
    private const string MessageFormat = "Logging in a catch clause should pass the caught exception as a parameter.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly KnownAssembly[] SupportedLoggingFrameworks =
    [
        KnownAssembly.MicrosoftExtensionsLoggingAbstractions,
        KnownAssembly.CastleCore,
        KnownAssembly.CommonLoggingCore,
        KnownAssembly.Log4Net,
        KnownAssembly.NLog,
        KnownAssembly.Serilog
    ];

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
                            if (walker.SafeVisit(catchClauseSyntax)
                                && !walker.IsExceptionLogged
                                && !walker.IsExceptionRethrown
                                && walker.LoggingInvocationsWithoutException.Any())
                            {
                                var primaryLocation = walker.LoggingInvocationsWithoutException[0].GetLocation();
                                var secondaryLocations = walker.LoggingInvocationsWithoutException.Skip(1).ToSecondaryLocations(MessageFormat);
                                c.ReportIssue(Rule, primaryLocation, secondaryLocations);
                            }
                        },
                        SyntaxKind.CatchClause);
                }
            });

    private sealed class LoggingInvocationWalker : CatchLoggingInvocationWalker
    {
        // A jump can bypass a later throw until we leave its scope or reach its target label.
        private readonly HashSet<SyntaxNode> scopesWithJumps = [];
        private int conditionalOrNestedFunctionDepth;

        public bool IsExceptionRethrown { get; private set; }

        public LoggingInvocationWalker(SemanticModel model) : base(model) { }

        public override void Visit(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.AnonymousMethodExpression:
                case SyntaxKindEx.LocalFunctionStatement:
                case SyntaxKind.IfStatement:
                case SyntaxKind.SwitchStatement:
                case SyntaxKind.ConditionalExpression:
                case SyntaxKind.CoalesceExpression:
                case SyntaxKindEx.CoalesceAssignmentExpression:
                case SyntaxKindEx.SwitchExpression:
                // A nested catch can swallow a rethrow from the try block. Its finally block is still unconditional.
                case SyntaxKind.Block when node.Parent is TryStatementSyntax { Catches.Count: > 0 }:
                // Loop bodies may never run. DoStatement is absent on purpose: its body always runs at least once.
                case SyntaxKind.WhileStatement:
                case SyntaxKind.ForStatement:
                case SyntaxKind.ForEachStatement:
                case SyntaxKindEx.ForEachVariableStatement:
                    // Keep visiting the subtree: the logging invocations inside it are still reported.
                    conditionalOrNestedFunctionDepth++;
                    VisitChildren(node);
                    conditionalOrNestedFunctionDepth--;
                    return;
                case SyntaxKind.ReturnStatement:
                    scopesWithJumps.Add(ContainingFunctionOrCatch(node));
                    break;
                case SyntaxKind.BreakStatement when node.Ancestors().FirstOrDefault(x => IsLoop(x) || x.IsKind(SyntaxKind.SwitchStatement)) is { } breakTarget:
                    scopesWithJumps.Add(breakTarget);
                    break;
                case SyntaxKind.ContinueStatement when node.Ancestors().FirstOrDefault(IsLoop) is { } continueTarget:
                    scopesWithJumps.Add(continueTarget);
                    break;
                case SyntaxKind.GotoStatement:
                    // Backward jumps within this scope only repeat a path. Jumps leaving it can still bypass the rethrow.
                    if (Model.GetSymbolInfo(((GotoStatementSyntax)node).Expression).Symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is { } label
                        && (label.SpanStart > node.SpanStart || !ContainingFunctionOrCatch(node).Span.Contains(label.Span)))
                    {
                        scopesWithJumps.Add(label);
                    }
                    break;
                case SyntaxKind.GotoCaseStatement:
                case SyntaxKind.GotoDefaultStatement:
                    if (node.Ancestors().FirstOrDefault(x => x.IsKind(SyntaxKind.SwitchStatement)) is { } switchTarget)
                    {
                        scopesWithJumps.Add(switchTarget);
                    }
                    break;
                case SyntaxKind.LabeledStatement:
                    scopesWithJumps.Remove(node);
                    break;
                case SyntaxKind.ThrowStatement when conditionalOrNestedFunctionDepth == 0 && scopesWithJumps.Count == 0:
                    IsExceptionRethrown |= RethrowsCaughtException(((ThrowStatementSyntax)node).Expression);
                    break;
                case SyntaxKindEx.ThrowExpression when conditionalOrNestedFunctionDepth == 0 && scopesWithJumps.Count == 0:
                    IsExceptionRethrown |= RethrowsCaughtException(((ThrowExpressionSyntaxWrapper)node).Expression);
                    break;
            }
            VisitChildren(node);
        }

        public override void VisitTryStatement(TryStatementSyntax node)
        {
            if (node.Finally is null)
            {
                base.VisitTryStatement(node);
                return;
            }

            var jumpsBeforeTry = scopesWithJumps.ToArray();
            Visit(node.Block);
            // Nested catch clauses are analyzed separately, as in CatchLoggingInvocationWalker.
            var jumpsFromTry = scopesWithJumps.ToArray();

            // Jumps inside this try cannot skip its finally, but jumps before the try can skip both.
            scopesWithJumps.Clear();
            scopesWithJumps.UnionWith(jumpsBeforeTry);
            Visit(node.Finally);
            scopesWithJumps.UnionWith(jumpsFromTry);
        }

        private static SyntaxNode ContainingFunctionOrCatch(SyntaxNode node) =>
            node.Ancestors().First(x => x.Kind() is SyntaxKind.CatchClause or SyntaxKind.AnonymousMethodExpression or SyntaxKindEx.LocalFunctionStatement);

        private static bool IsLoop(SyntaxNode node) =>
            node.Kind() is SyntaxKind.DoStatement or SyntaxKind.WhileStatement or SyntaxKind.ForStatement or SyntaxKind.ForEachStatement or SyntaxKindEx.ForEachVariableStatement;

        private void VisitChildren(SyntaxNode node)
        {
            base.Visit(node);
            scopesWithJumps.Remove(node);
        }
    }
}
