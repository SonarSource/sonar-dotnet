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

namespace SonarAnalyzer.VisualBasic.Rules
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class UseReturnStatement : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S5944";
        private const string MessageFormat = "{0}";
        private const string UseReturnStatementMessage = "Use a 'Return' statement; assigning returned values to function names is obsolete.";
        private const string DontUseImplicitMessage = "Do not make use of the implicit return value.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
            {
                var method = (MethodStatementSyntax)((MethodBlockSyntax)c.Node).BlockStatement;
                new IdentifierWalker(c, method.Identifier.ValueText).SafeVisit(c.Node);
            },
            SyntaxKind.FunctionBlock);

        private class IdentifierWalker : SafeVisualBasicSyntaxWalker
        {
            private readonly SonarSyntaxNodeReportingContext context;
            private readonly string name;

            public IdentifierWalker(SonarSyntaxNodeReportingContext context, string name)
            {
                this.context = context;
                this.name = name;
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                if (IsImplicitReturnValue(node))
                {
                    context.ReportIssue(Rule, node, IsAssignmentStatement(node) ? UseReturnStatementMessage : DontUseImplicitMessage);
                }
            }

            public override void VisitImplementsClause(ImplementsClauseSyntax node) { /* Skip */ }

            public override void VisitAttributeList(AttributeListSyntax node) { /* Skip */ }

            private bool IsImplicitReturnValue(IdentifierNameSyntax node) =>
                name.Equals(node.Identifier.ValueText, StringComparison.InvariantCultureIgnoreCase)
                && !IsExcluded(node);

            private static bool IsExcluded(SyntaxNode node) =>
                node.Parent switch
                {
                    InvocationExpressionSyntax => true,
                    MemberAccessExpressionSyntax => true,
                    QualifiedNameSyntax => true,
                    NamedFieldInitializerSyntax => true,
                    NameOfExpressionSyntax => true,
                    AsClauseSyntax => true,
                    ObjectCreationExpressionSyntax => true,
                    UnaryExpressionSyntax unary => unary.IsKind(SyntaxKind.AddressOfExpression),
                    NameColonEqualsSyntax nameColon => nameColon.Name == node,
                    _ => false,
                };

            private static bool IsAssignmentStatement(SyntaxNode node) =>
                node.Parent is AssignmentStatementSyntax assignement
                && assignement.Left == node;
        }
    }
}
