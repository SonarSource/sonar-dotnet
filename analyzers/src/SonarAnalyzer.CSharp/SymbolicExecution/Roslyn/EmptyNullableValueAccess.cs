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

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;

public class EmptyNullableValueAccess : EmptyNullableValueAccessBase
{
    private const string MessageFormat = "'{0}' is null on at least one execution path.";

    internal static readonly DiagnosticDescriptor S3655 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    protected override DiagnosticDescriptor Rule => S3655;

    public override bool ShouldExecute()
    {
        var walker = new SyntaxKindWalker(SemanticModel);
        walker.SafeVisit(Node);
        return walker.Result;
    }

    private sealed class SyntaxKindWalker : SafeCSharpSyntaxWalker
    {
        public bool Result { get; private set; }
        private readonly SemanticModel semanticModel;

        public SyntaxKindWalker(SemanticModel semanticModel) => this.semanticModel = semanticModel;

        public override void Visit(SyntaxNode node)
        {
            if (!Result)
            {
                Result |= CheckMemberAccess(node) || CheckCast(node);

                if (!Result)
                {
                    base.Visit(node);
                }
            }

            bool CheckMemberAccess(SyntaxNode node) =>
                node is MemberAccessExpressionSyntax memberAccess
                && (memberAccess.NameIs(nameof(Nullable<int>.Value)) || memberAccess.NameIs(nameof(Nullable<int>.GetType)))
                // FIXME: check with Pavel
                && semanticModel.GetTypeInfo(memberAccess.Expression) is { Type: var memberType }
                && memberType.IsNullableValueType();

            bool CheckCast(SyntaxNode node) =>
                node is CastExpressionSyntax cast
                && semanticModel.GetTypeInfo(cast.Expression) is { Type: var targetType }
                && semanticModel.GetTypeInfo(cast.Type) is { Type: var castType }
                && targetType.IsNullableValueType()
                && castType.IsValueType;
        }
    }
}
