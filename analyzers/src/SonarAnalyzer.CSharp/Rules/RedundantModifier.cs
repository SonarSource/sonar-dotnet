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
    public sealed class RedundantModifier : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2333";
        private const string MessageFormat = "'{0}' is {1} in this context.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        private static readonly ISet<SyntaxKind> UnsafeConstructKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.AddressOfExpression,
            SyntaxKind.PointerIndirectionExpression,
            SyntaxKind.SizeOfExpression,
            SyntaxKind.PointerType,
            SyntaxKind.FixedStatement,
            SyntaxKind.StackAllocArrayCreationExpression
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                CheckSealedMemberInSealedClass,
                SyntaxKind.EventDeclaration,
                SyntaxKind.EventFieldDeclaration,
                SyntaxKind.IndexerDeclaration,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.PropertyDeclaration);

            context.RegisterNodeAction(
                CheckTypeDeclarationForRedundantPartial,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKindEx.RecordClassDeclaration,
                SyntaxKindEx.RecordStructDeclaration,
                SyntaxKind.StructDeclaration);

            context.RegisterNodeAction(
                CheckForUnnecessaryUnsafeBlocks,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKindEx.RecordClassDeclaration,
                SyntaxKindEx.RecordStructDeclaration,
                SyntaxKind.StructDeclaration);

            context.RegisterNodeAction(c =>
                {
                    if (CheckedWalker.IsTopLevel(c.Node))
                    {
                        new CheckedWalker(c).SafeVisit(c.Node);
                    }
                },
                SyntaxKind.CheckedExpression,
                SyntaxKind.CheckedStatement,
                SyntaxKind.UncheckedExpression,
                SyntaxKind.UncheckedStatement);
        }

        private static void CheckForUnnecessaryUnsafeBlocks(SonarSyntaxNodeReportingContext context)
        {
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;
            if (typeDeclaration.Parent is TypeDeclarationSyntax || context.IsRedundantPositionalRecordContext())
            {
                // only process top level type declarations
                return;
            }

            CheckForUnnecessaryUnsafeBlocksBelow(context, typeDeclaration);
        }

        private static void CheckForUnnecessaryUnsafeBlocksBelow(SonarSyntaxNodeReportingContext context, TypeDeclarationSyntax typeDeclaration)
        {
            var unsafeKeyword = FindUnsafeKeyword(typeDeclaration);
            if (unsafeKeyword == default)
            {
                foreach (var member in typeDeclaration.Members)
                {
                    CheckForUnnecessaryUnsafeBlocksInMember(context, member);
                }
            }
            else
            {
                MarkAllUnsafeBlockInside(context, typeDeclaration);
                if (!HasUnsafeConstructInside(typeDeclaration, context.SemanticModel))
                {
                    ReportOnUnsafeBlock(context, unsafeKeyword.GetLocation());
                }
            }
        }

        private static void CheckForUnnecessaryUnsafeBlocksInMember(SonarSyntaxNodeReportingContext context, MemberDeclarationSyntax member)
        {
            var unsafeKeyword = FindUnsafeKeyword(member);
            if (unsafeKeyword != default)
            {
                MarkAllUnsafeBlockInside(context, member);
                if (!HasUnsafeConstructInside(member, context.SemanticModel))
                {
                    ReportOnUnsafeBlock(context, unsafeKeyword.GetLocation());
                }
            }
            else if (member is TypeDeclarationSyntax nestedTypeDeclaration)
            {
                CheckForUnnecessaryUnsafeBlocksBelow(context, nestedTypeDeclaration);
            }
            else
            {
                var topLevelUnsafeBlocks = member.DescendantNodes(n => !n.IsKind(SyntaxKind.UnsafeStatement)).OfType<UnsafeStatementSyntax>();
                foreach (var topLevelUnsafeBlock in topLevelUnsafeBlocks)
                {
                    MarkAllUnsafeBlockInside(context, topLevelUnsafeBlock);
                    if (!HasUnsafeConstructInside(member, context.SemanticModel))
                    {
                        ReportOnUnsafeBlock(context, topLevelUnsafeBlock.UnsafeKeyword.GetLocation());
                    }
                }
            }
        }

        private static bool HasUnsafeConstructInside(SyntaxNode container, SemanticModel semanticModel) =>
            ContainsUnsafeConstruct(container)
            || ContainsFixedDeclaration(container)
            || ContainsUnsafeTypedIdentifier(container, semanticModel)
            || ContainsUnsafeInvocationReturnValue(container, semanticModel)
            || ContainsUnsafeParameter(container, semanticModel);

        private static bool ContainsUnsafeParameter(SyntaxNode container, SemanticModel semanticModel) =>
            container.DescendantNodes()
                .OfType<ParameterSyntax>()
                .Any(x => IsUnsafe(semanticModel.GetDeclaredSymbol(x)?.Type));

        private static bool ContainsUnsafeInvocationReturnValue(SyntaxNode container, SemanticModel semanticModel) =>
            container.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(x => semanticModel.GetSymbolInfo(x).Symbol is IMethodSymbol method && IsUnsafe(method.ReturnType));

        private static bool ContainsUnsafeTypedIdentifier(SyntaxNode container, SemanticModel semanticModel) =>
            container.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Any(x => IsUnsafe(semanticModel.GetTypeInfo(x).Type));

        private static bool ContainsFixedDeclaration(SyntaxNode container) =>
            container.DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .Any(x => x.Modifiers.Any(SyntaxKind.FixedKeyword));

        private static bool ContainsUnsafeConstruct(SyntaxNode container) =>
            container.DescendantNodes().Any(x => UnsafeConstructKinds.Contains(x.Kind()));

        private static bool IsUnsafe(ITypeSymbol type) =>
            type != null
            && (type.TypeKind == TypeKind.Pointer
                || (type.TypeKind == TypeKind.Array && IsUnsafe(((IArrayTypeSymbol)type).ElementType)));

        private static void MarkAllUnsafeBlockInside(SonarSyntaxNodeReportingContext context, SyntaxNode container)
        {
            foreach (var @unsafe in container.DescendantNodes().SelectMany(x => x.ChildTokens()).Where(x => x.IsKind(SyntaxKind.UnsafeKeyword)))
            {
                ReportOnUnsafeBlock(context, @unsafe.GetLocation());
            }
        }

        private static void ReportOnUnsafeBlock(SonarSyntaxNodeReportingContext context, Location issueLocation) =>
            context.ReportIssue(CreateDiagnostic(Rule, issueLocation, "unsafe", "redundant"));

        private static SyntaxToken FindUnsafeKeyword(MemberDeclarationSyntax memberDeclaration) =>
            Modifiers(memberDeclaration).FirstOrDefault(x => x.IsKind(SyntaxKind.UnsafeKeyword));

        private static void CheckTypeDeclarationForRedundantPartial(SonarSyntaxNodeReportingContext context)
        {
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;
            if (!context.IsRedundantPositionalRecordContext()
                && typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
                && context.SemanticModel.GetDeclaredSymbol(typeDeclaration) is { DeclaringSyntaxReferences.Length: <= 1 })
            {
                var keyword = typeDeclaration.Modifiers.First(m => m.IsKind(SyntaxKind.PartialKeyword));
                context.ReportIssue(CreateDiagnostic(Rule, keyword.GetLocation(), "partial", "gratuitous"));
            }
        }

        private static SyntaxTokenList Modifiers(MemberDeclarationSyntax memberDeclaration) =>
            memberDeclaration switch
            {
                BasePropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Modifiers,
                BaseMethodDeclarationSyntax methodDeclaration => methodDeclaration.Modifiers,
                BaseFieldDeclarationSyntax fieldDeclaration => fieldDeclaration.Modifiers,
                DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.Modifiers,
                TypeDeclarationSyntax typeDeclaration => typeDeclaration.Modifiers,
                _ => default
            };

        private static void CheckSealedMemberInSealedClass(SonarSyntaxNodeReportingContext context)
        {
            if (Modifiers((MemberDeclarationSyntax)context.Node) is var modifiers
                && modifiers.Any(SyntaxKind.SealedKeyword)
                && context.ContainingSymbol.ContainingType is { IsSealed: true })
            {
                var keyword = modifiers.First(m => m.IsKind(SyntaxKind.SealedKeyword));
                context.ReportIssue(CreateDiagnostic(Rule, keyword.GetLocation(), "sealed", "redundant"));
            }
        }

        private sealed class CheckedWalker : SafeCSharpSyntaxWalker
        {
            private static readonly ISet<SyntaxKind> BinaryOperationsForChecked = new HashSet<SyntaxKind>
            {
                SyntaxKind.AddExpression,
                SyntaxKind.SubtractExpression,
                SyntaxKind.MultiplyExpression,
                SyntaxKind.DivideExpression
            };

            private static readonly ISet<SyntaxKind> AssignmentsForChecked = new HashSet<SyntaxKind>
            {
                SyntaxKind.AddAssignmentExpression,
                SyntaxKind.SubtractAssignmentExpression,
                SyntaxKind.MultiplyAssignmentExpression,
                SyntaxKind.DivideAssignmentExpression
            };

            private static readonly ISet<SyntaxKind> UnaryOperationsForChecked = new HashSet<SyntaxKind>
            {
                SyntaxKind.UnaryMinusExpression,
                SyntaxKind.PostDecrementExpression,
                SyntaxKind.PostIncrementExpression,
                SyntaxKind.PreDecrementExpression,
                SyntaxKind.PreIncrementExpression
            };

            private readonly SonarSyntaxNodeReportingContext context;
            private bool isCurrentContextChecked;
            private bool currentContextHasIntegralOperation;

            public CheckedWalker(SonarSyntaxNodeReportingContext context)
            {
                this.context = context;
                isCurrentContextChecked = context.Node switch
                {
                    CheckedStatementSyntax statement => statement.IsKind(SyntaxKind.CheckedStatement),
                    CheckedExpressionSyntax expression => expression.IsKind(SyntaxKind.CheckedExpression),
                    _ => false
                };
            }

            public override void VisitCheckedExpression(CheckedExpressionSyntax node) =>
                VisitChecked(node, SyntaxKind.CheckedExpression, node.Keyword, base.VisitCheckedExpression);

            public override void VisitCheckedStatement(CheckedStatementSyntax node) =>
                VisitChecked(node, SyntaxKind.CheckedStatement, node.Keyword, base.VisitCheckedStatement);

            public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
            {
                base.VisitAssignmentExpression(node);
                if (AssignmentsForChecked.Contains(node.Kind()))
                {
                    SetHasIntegralOperation(node);
                }
            }

            public override void VisitBinaryExpression(BinaryExpressionSyntax node)
            {
                base.VisitBinaryExpression(node);
                if (BinaryOperationsForChecked.Contains(node.Kind()))
                {
                    SetHasIntegralOperation(node);
                }
            }

            public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
            {
                base.VisitPrefixUnaryExpression(node);
                if (UnaryOperationsForChecked.Contains(node.Kind()))
                {
                    SetHasIntegralOperation(node);
                }
            }

            public override void VisitCastExpression(CastExpressionSyntax node)
            {
                base.VisitCastExpression(node);
                SetHasIntegralOperation(node);
            }

            public static bool IsTopLevel(SyntaxNode node) =>
                !node.Ancestors().Any(x => x is CheckedStatementSyntax || x is CheckedExpressionSyntax);

            private void VisitChecked<T>(T node, SyntaxKind checkedKind, SyntaxToken tokenToReport, Action<T> baseCall)
                where T : SyntaxNode
            {
                var isThisNodeChecked = node.IsKind(checkedKind);
                var originalIsCurrentContextChecked = isCurrentContextChecked;
                var originalContextHasIntegralOperation = currentContextHasIntegralOperation;
                isCurrentContextChecked = isThisNodeChecked;
                currentContextHasIntegralOperation = false;

                baseCall(node);

                var isSimplyRedundant = IsCurrentNodeEmbeddedInsideSameChecked(node, isThisNodeChecked, originalIsCurrentContextChecked);
                if (isSimplyRedundant || !currentContextHasIntegralOperation)
                {
                    var keywordToReport = isThisNodeChecked ? "checked" : "unchecked";
                    context.ReportIssue(CreateDiagnostic(Rule, tokenToReport.GetLocation(), keywordToReport, "redundant"));
                }
                isCurrentContextChecked = originalIsCurrentContextChecked;
                currentContextHasIntegralOperation = originalContextHasIntegralOperation || (currentContextHasIntegralOperation && isSimplyRedundant);
            }

            private bool IsCurrentNodeEmbeddedInsideSameChecked(SyntaxNode node, bool isThisNodeChecked, bool isCurrentContextChecked) =>
                isThisNodeChecked == isCurrentContextChecked && node != context.Node;

            private void SetHasIntegralOperation(CastExpressionSyntax node)
            {
                if (!currentContextHasIntegralOperation)
                {
                    var expressionType = context.SemanticModel.GetTypeInfo(node.Expression).Type;
                    var castedToType = context.SemanticModel.GetTypeInfo(node.Type).Type;
                    currentContextHasIntegralOperation = castedToType is not null && expressionType is not null && castedToType.IsAny(KnownType.IntegralNumbers);
                }
            }

            private void SetHasIntegralOperation(ExpressionSyntax node) =>
                currentContextHasIntegralOperation = currentContextHasIntegralOperation
                    || (context.SemanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol methodSymbol && methodSymbol.ReceiverType.IsAny(KnownType.IntegralNumbers));
        }
    }
}
