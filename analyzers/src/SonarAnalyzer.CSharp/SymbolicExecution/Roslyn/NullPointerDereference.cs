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

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;

public class NullPointerDereference : NullPointerDereferenceBase
{
    private const string MessageFormat = "'{0}' is null on at least one execution path.";

    internal static readonly DiagnosticDescriptor S2259 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    protected override DiagnosticDescriptor Rule => S2259;

    protected override bool IsSupressed(SyntaxNode node) =>
        node.Parent.WalkUpParentheses() is { RawKind: (int)SyntaxKindEx.SuppressNullableWarningExpression };

    public override bool ShouldExecute()
    {
        var walker = new SyntaxKindWalker();
        walker.SafeVisit(Node);
        return walker.Result;
    }

    internal sealed class SyntaxKindWalker : SafeCSharpSyntaxWalker
    {
        public bool Result { get; private set; }

        public override void Visit(SyntaxNode node)
        {
            Result |= node.Kind() is SyntaxKindEx.ForEachVariableStatement;
            if (!Result)
            {
                base.Visit(node);
            }
        }

        public override void VisitAwaitExpression(AwaitExpressionSyntax node) =>
            Result = true;

        public override void VisitElementAccessExpression(ElementAccessExpressionSyntax node) =>
            Result = true;

        public override void VisitForEachStatement(ForEachStatementSyntax node) =>
            Result = true;

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            if (node.Kind() is SyntaxKind.SimpleMemberAccessExpression)
            {
                Result = true;
            }
            else
            {
                base.VisitMemberAccessExpression(node);
            }
        }
    }
}
