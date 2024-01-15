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

using System.Runtime.Serialization;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.VisualBasic;

public sealed class RestrictDeserializedTypes : RestrictDeserializedTypesBase
{
    public static readonly DiagnosticDescriptor S5773 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    protected override DiagnosticDescriptor Rule => S5773;

    public override bool ShouldExecute()
    {
        var walker = new Walker();
        walker.SafeVisit(Node);
        return walker.Result;
    }

    protected override SyntaxNode FindBindToTypeMethodDeclaration(IOperation operation) =>
        MethodCandidates(operation).FirstOrDefault(x =>
            x is MethodBlockSyntax { SubOrFunctionStatement: { Identifier.Text: nameof(SerializationBinder.BindToType), ParameterList: { Parameters.Count: 2 } parameterList } }
            && parameterList.EnsureCorrectSemanticModelOrDefault(SemanticModel) is { } semanticModel
            && parameterList.Parameters[0].AsClause.Type.IsKnownType(KnownType.System_String, semanticModel)
            && parameterList.Parameters[1].AsClause.Type.IsKnownType(KnownType.System_String, semanticModel));

    protected override SyntaxNode FindResolveTypeMethodDeclaration(IOperation operation) =>
        MethodCandidates(operation)?.FirstOrDefault(x =>
            x is MethodBlockSyntax { SubOrFunctionStatement: { Identifier.Text: "ResolveType", ParameterList: { Parameters.Count: 1 } parameterList } }
            && parameterList.EnsureCorrectSemanticModelOrDefault(SemanticModel) is { } semanticModel
            && parameterList.Parameters[0].AsClause.Type.IsKnownType(KnownType.System_String, semanticModel));

    protected override bool ThrowsOrReturnsNull(SyntaxNode methodDeclaration) =>
        methodDeclaration.DescendantNodes().OfType<ThrowStatementSyntax>().Any() ||
        methodDeclaration.DescendantNodes().OfType<ExpressionSyntax>().Any(expression => expression.IsKind(SyntaxKindEx.ThrowExpression)) ||
        methodDeclaration.DescendantNodes().OfType<ReturnStatementSyntax>().Any(returnStatement => returnStatement.Expression.IsKind(SyntaxKind.NothingLiteralExpression)) ||
        // For simplicity this returns true for any method witch contains a NullLiteralExpression but this could be a source of FNs
        methodDeclaration.DescendantNodes().OfType<ExpressionSyntax>().Any(expression => expression.IsKind(SyntaxKind.NothingLiteralExpression));

    protected override SyntaxToken GetIdentifier(SyntaxNode methodDeclaration) => ((MethodBlockSyntax)methodDeclaration).SubOrFunctionStatement.Identifier;

    private static IEnumerable<SyntaxNode> MethodCandidates(IOperation operation) =>
        operation.Type?.DeclaringSyntaxReferences.SelectMany(x => x.GetSyntax().Parent.DescendantNodes());

    private sealed class Walker : SafeVisualBasicSyntaxWalker
    {
        public bool Result { get; private set; }

        public override void Visit(SyntaxNode node)
        {
            if (!Result)
            {
                base.Visit(node);
            }
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node) =>
            Result = node.NameIs(nameof(IFormatter.Deserialize));

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node) =>
            Result = node.Type.NameIs("LosFormatter");
    }
}
