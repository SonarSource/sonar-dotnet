/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ConsumeValueTaskCorrectly : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S5034";
        internal const string MessageFormat = "{0}";

        // 'await', 'AsTask', 'Result' and '.GetAwaiter().GetResult()' should be called only once on a ValueTask
        private const string ConsumeOnlyOnceMessage = "Refactor this 'ValueTask' usage to consume it only once.";
        // 'Result' and '.GetAwaiter().GetResult()' should be consumed inside an 'if (valueTask.IsCompletedSuccessfully)'
        private const string ConsumeOnlyIfCompletedMessage = "Refactor this 'ValueTask' usage to consume the result only if the operation has completed successfully.";

        private static readonly DiagnosticDescriptor rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        private static readonly ImmutableArray<KnownType> ValueTaskTypes =
            new[] {
                KnownType.System_Runtime_CompilerServices_ValueTaskAwaiter,
                KnownType.System_Runtime_CompilerServices_ValueTaskAwaiter_TResult,
                KnownType.System_Threading_Tasks_ValueTask,
                KnownType.System_Threading_Tasks_ValueTask_TResult
            }.ToImmutableArray();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    var walker = new ConsumeValueTaskWalker(c.SemanticModel);
                    walker.SafeVisit(c.Node);

                    foreach (var syntaxNodes in walker.SymbolUsages.Values)
                    {
                        if (syntaxNodes.Count > 1)
                        {
                            c.ReportIssue(rule.CreateDiagnostic(c.Compilation,
                                syntaxNodes.First().GetLocation(),
                                additionalLocations: syntaxNodes.Skip(1).Select(node => node.GetLocation()).ToArray(),
                                messageArgs: ConsumeOnlyOnceMessage));
                        }
                    }

                    foreach (var node in walker.ConsumedButNotCompleted)
                    {
                        c.ReportIssue(CreateDiagnostic(rule, node.GetLocation(), messageArgs: ConsumeOnlyIfCompletedMessage   ));
                    }
                },
                // when visiting a method or another member with logic inside, lambdas and local functions will be visited as well
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.ConversionOperatorDeclaration,
                SyntaxKind.OperatorDeclaration,
                SyntaxKind.PropertyDeclaration,
                SyntaxKind.DestructorDeclaration,
                SyntaxKind.MethodDeclaration);

        private sealed class ConsumeValueTaskWalker : SafeCSharpSyntaxWalker
        {
            private readonly SemanticModel semanticModel;

            // The key is the 'ValueTask' variable symbol, the value is a list of nodes where it is consumed
            public IDictionary<ISymbol, IList<SyntaxNode>> SymbolUsages { get; }

            // A list of 'ValueTask' nodes on which '.Result' or '.GetAwaiter().GetResult()' has been invoked when the operation has not yet completed
            public IList<SyntaxNode> ConsumedButNotCompleted { get; }

            // List of 'ValueTask' symbols which have been accessed for 'IsCompletedSuccessfully'
            public ISet<ISymbol> VerifiedForSuccessfulCompletion { get; }

            public ConsumeValueTaskWalker(SemanticModel semanticModel)
            {
                this.semanticModel = semanticModel;
                SymbolUsages = new Dictionary<ISymbol, IList<SyntaxNode>>();
                ConsumedButNotCompleted = new List<SyntaxNode>();
                VerifiedForSuccessfulCompletion = new HashSet<ISymbol>();
            }

            /**
             * Check if 'await' is done on a 'ValueTask'
             */
            public override void VisitAwaitExpression(AwaitExpressionSyntax node)
            {
                if (node.Expression is IdentifierNameSyntax identifierName &&
                    this.semanticModel.GetSymbolInfo(identifierName).Symbol is ISymbol symbol &&
                    symbol.GetSymbolType().OriginalDefinition.IsAny(ValueTaskTypes))
                {
                    AddToSymbolUsages(symbol, identifierName);
                }

                base.VisitAwaitExpression(node);
            }

            /**
             * Check if it's the wanted method on a ValueTask
             * - we treat AsTask() like await - always add it to the list
             * - for GetAwaiter().GetResult() - ignore the call if it's called inside an 'if (valueTask.IsCompletedSuccessfully)'
             */
            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (node.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    if (node.IsMethodInvocation(ValueTaskTypes, "AsTask", semanticModel) &&
                        GetLeftMostIdentifier(memberAccess) is IdentifierNameSyntax identifier1)
                    {
                        AddToSymbolUsages(this.semanticModel.GetSymbolInfo(identifier1).Symbol, identifier1);
                    }

                    if (node.IsMethodInvocation(ValueTaskTypes, "GetResult", semanticModel) &&
                        GetLeftMostIdentifier(memberAccess) is IdentifierNameSyntax identifier2 &&
                        this.semanticModel.GetSymbolInfo(identifier2).Symbol is ISymbol symbol2 &&
                        !VerifiedForSuccessfulCompletion.Contains(symbol2))
                    {
                        AddToSymbolUsages(symbol2, identifier2);
                        ConsumedButNotCompleted.Add(identifier2);
                    }
                }

                base.VisitInvocationExpression(node);
            }

            /**
             * Check if ".Result" is accessed on a 'ValueTask'
             * - ignore the call if it's called inside an 'if (valueTask.IsCompletedSuccessfully)'
             */
            public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                if (node.IsPropertyInvocation(ValueTaskTypes, "Result", semanticModel) &&
                    GetLeftMostIdentifier(node) is IdentifierNameSyntax identifierName &&
                    this.semanticModel.GetSymbolInfo(identifierName).Symbol is ISymbol symbol &&
                    !VerifiedForSuccessfulCompletion.Contains(symbol))
                {
                    AddToSymbolUsages(symbol, identifierName);
                    ConsumedButNotCompleted.Add(identifierName);
                }

                base.VisitMemberAccessExpression(node);
            }

            public override void VisitIfStatement(IfStatementSyntax node)
            {
                var valueTaskMemberAccess = node.Condition.DescendantNodesAndSelf().FirstOrDefault(n =>
                    n is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.IsPropertyInvocation(ValueTaskTypes, "IsCompletedSuccessfully", semanticModel));
                if (valueTaskMemberAccess is MemberAccessExpressionSyntax member &&
                    GetLeftMostIdentifier(member) is IdentifierNameSyntax identifierName &&
                    this.semanticModel.GetSymbolInfo(identifierName).Symbol is ISymbol symbol &&
                    !VerifiedForSuccessfulCompletion.Contains(symbol))
                {
                    VerifiedForSuccessfulCompletion.Add(symbol);
                }
                base.VisitIfStatement(node);
            }

            private SyntaxNode GetLeftMostIdentifier(MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Expression is InvocationExpressionSyntax invocation &&
                    invocation.Expression is MemberAccessExpressionSyntax innerMemberAccess &&
                    innerMemberAccess.Expression is IdentifierNameSyntax leftMostIdentifier)
                {
                    return leftMostIdentifier;
                }
                else if (memberAccess.Expression is IdentifierNameSyntax identifierName)
                {
                    return identifierName;
                }
                else if (memberAccess.Expression is MemberAccessExpressionSyntax leftMemberAccess &&
                    leftMemberAccess.Name is IdentifierNameSyntax name)
                {
                    // gets 'task' from 'this.Task.Result' or 'Foo.Task.Result'
                    return name;
                }
                return memberAccess.Expression;
            }

            private void AddToSymbolUsages(ISymbol symbol, SyntaxNode syntaxNode)
            {
                if (SymbolUsages.TryGetValue(symbol, out var syntaxNodes))
                {
                    syntaxNodes.Add(syntaxNode);
                }
                else
                {
                    SymbolUsages.Add(symbol, new List<SyntaxNode>() { syntaxNode });
                }
            }

        }
    }
}
