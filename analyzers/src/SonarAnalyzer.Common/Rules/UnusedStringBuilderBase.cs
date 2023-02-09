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

public abstract class UnusedStringBuilderBase<TSyntaxKind, TVariableDeclarator, TIdentifierName> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TVariableDeclarator : SyntaxNode
    where TIdentifierName : SyntaxNode
{
    private const string DiagnosticId = "S3063";

    internal readonly string[] StringBuilderAccessInvocations = { "ToString", "CopyTo", "GetChunks" };
    internal readonly string[] StringBuilderAccessExpressions = { "Length" };

    protected override string MessageFormat => """Remove this "StringBuilder"; ".ToString()" is never called.""";

    protected abstract string GetName(SyntaxNode declaration);
    protected abstract SyntaxNode GetScope(TVariableDeclarator declarator);
    protected abstract ILocalSymbol RetrieveStringBuilderObject(TVariableDeclarator declaration, SemanticModel semanticModel);
    protected abstract bool IsStringBuilderRead(string name, ILocalSymbol symbol, SyntaxNode node, SemanticModel model);
    protected abstract bool DescendIntoChildren(SyntaxNode node);

    protected UnusedStringBuilderBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            var variableDeclaration = (TVariableDeclarator)c.Node;

            if (RetrieveStringBuilderObject(variableDeclaration, c.SemanticModel) is not { } symbol
                || GetScope(variableDeclaration).DescendantNodes(DescendIntoChildren).Any(node =>
                    IsStringBuilderRead(GetName(variableDeclaration), symbol, node, c.SemanticModel)))
            {
                return;
            }
            c.ReportIssue(Diagnostic.Create(Rule, variableDeclaration.GetLocation()));
        }, Language.SyntaxKind.VariableDeclarator);

    internal bool IsSameReference(SyntaxNode expression, string name, ILocalSymbol symbol, SemanticModel semanticModel) =>
        expression is not null && GetLocalReferences(expression).Any(x => IsSameVariable(x, name, symbol, semanticModel));

    internal bool IsSameVariable(SyntaxNode identifier, string name, ILocalSymbol symbol, SemanticModel semanticModel) =>
        GetName(identifier).Equals(name, Language.NameComparison) && symbol.Equals(semanticModel.GetSymbolInfo(identifier).Symbol);

    internal static IEnumerable<TIdentifierName> GetLocalReferences(SyntaxNode node) =>
        node.DescendantNodesAndSelf().OfType<TIdentifierName>();
}
