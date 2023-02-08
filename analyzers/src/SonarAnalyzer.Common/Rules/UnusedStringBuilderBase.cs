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

namespace SonarAnalyzer.Rules;

public abstract class UnusedStringBuilderBase<TSyntaxKind, TVariableDeclarator, TIdentifierName, TConditionalExpression> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TVariableDeclarator : SyntaxNode
    where TIdentifierName : SyntaxNode
    where TConditionalExpression : SyntaxNode
{
    private const string DiagnosticId = "S3063";
    protected override string MessageFormat => """Remove this "StringBuilder"; ".ToString()" is never called.""";

    internal readonly string[] StringBuilderAccessInvocations = { "ToString", "CopyTo", "GetChunks" };
    internal readonly string[] StringBuilderAccessExpressions = { "Length", "Capacity", "MaxCapacity" };

    protected abstract ILocalSymbol GetSymbol(TVariableDeclarator declaration, SemanticModel semanticModel);
    protected abstract bool NeedsToTrack(TVariableDeclarator declaration, SemanticModel semanticModel);
    protected abstract bool IsStringBuilderRead(SemanticModel model, ILocalSymbol local, SyntaxNode node);
    protected abstract SyntaxNode GetScope(TVariableDeclarator declarator);
    protected abstract bool DescendIntoChildren(SyntaxNode node);

    protected UnusedStringBuilderBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            var variableDeclaration = (TVariableDeclarator)c.Node;
            if (!NeedsToTrack(variableDeclaration, c.SemanticModel)
                || GetScope(variableDeclaration).DescendantNodes(DescendIntoChildren).Any(node => IsStringBuilderRead(c.SemanticModel, GetSymbol(variableDeclaration, c.SemanticModel), node)))
            {
                return;
            }
            c.ReportIssue(Diagnostic.Create(Rule, variableDeclaration.GetLocation()));
        }, Language.SyntaxKind.VariableDeclarator);

    internal static bool IsSameReference(SyntaxNode expression, ILocalSymbol variableSymbol, SemanticModel semanticModel)
    {
        var references = GetLocalReferences(expression, semanticModel);
        if (!references.Any() && expression.Ancestors().OfType<TConditionalExpression>().Any())
        {
            references = GetLocalReferences(expression.Ancestors().OfType<TConditionalExpression>().First(), semanticModel);
        }
        return references.Any(x => IsSameVariable(x, variableSymbol, semanticModel));
    }

    internal static bool IsSameVariable(SyntaxNode identifier, ILocalSymbol variableSymbol, SemanticModel semanticModel) =>
        variableSymbol.Equals(semanticModel.GetSymbolInfo(identifier).Symbol);

    internal static IEnumerable<TIdentifierName> GetLocalReferences(SyntaxNode node, SemanticModel semanticModel) =>
        node.DescendantNodesAndSelf().OfType<TIdentifierName>().Where(x => IsLocalReference(x, semanticModel));

    internal static bool IsLocalReference(SyntaxNode identifier, SemanticModel semanticModel) =>
        semanticModel.GetOperation(identifier) is { Kind: OperationKindEx.LocalReference };
}
