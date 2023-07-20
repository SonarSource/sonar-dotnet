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

    private readonly string[] stringBuilderAccessInvocations = { "ToString", "CopyTo", "GetChunks" };
    private readonly string[] stringBuilderAccessExpressions = { "Length" };

    protected override string MessageFormat => """Remove this "StringBuilder"; ".ToString()" is never called.""";

    protected abstract SyntaxNode GetScope(TVariableDeclarator declarator);
    protected abstract ILocalSymbol RetrieveStringBuilderObject(SemanticModel semanticModel, TVariableDeclarator declaration);
    protected abstract bool IsStringBuilderRead(SemanticModel model, ILocalSymbol symbol, SyntaxNode node);
    protected abstract bool DescendIntoChildren(SyntaxNode node);

    protected UnusedStringBuilderBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            var variableDeclarator = (TVariableDeclarator)c.Node;

            if (RetrieveStringBuilderObject(c.SemanticModel, variableDeclarator) is { } symbol
                && GetScope(variableDeclarator) is { } scope
                && !IsStringBuilderAccessed(c.SemanticModel, symbol, scope))
            {
                c.ReportIssue(CreateDiagnostic(Rule, variableDeclarator.GetLocation()));
            }
        }, Language.SyntaxKind.VariableDeclarator);

    internal bool IsSameReference(SemanticModel semanticModel, ILocalSymbol symbol, SyntaxNode expression) =>
        expression is not null && GetLocalReferences(expression).Any(x => IsSameVariable(semanticModel, symbol, x));

    private bool IsSameVariable(SemanticModel semanticModel, ILocalSymbol symbol, SyntaxNode identifier) =>
        Language.GetName(identifier).Equals(symbol.Name, Language.NameComparison) && symbol.Equals(semanticModel.GetSymbolInfo(identifier).Symbol);

    private static IEnumerable<TIdentifierName> GetLocalReferences(SyntaxNode node) =>
        node.DescendantNodesAndSelf().OfType<TIdentifierName>();

    private bool IsStringBuilderAccessed(SemanticModel model, ILocalSymbol symbol, SyntaxNode scope) =>
        scope.DescendantNodes(DescendIntoChildren).Any(node => IsStringBuilderRead(model, symbol, node));

    internal bool IsAccessInvocation(string invocation) =>
        stringBuilderAccessInvocations.Any(x => x.Equals(invocation, Language.NameComparison));

    internal bool IsAccessExpression(string expression) =>
        stringBuilderAccessExpressions.Any(x => x.Equals(expression, Language.NameComparison));
}
