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

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;

public static class BaseMethodDeclarationSyntaxExtensions
{
    extension(BaseMethodDeclarationSyntax method)
    {
        public IEnumerable<SyntaxNode> BodyDescendantNodes =>
            (method ?? throw new ArgumentNullException(nameof(method))).Body == null
                ? method.ExpressionBody().DescendantNodes()
                : method.Body.DescendantNodes();

        public bool IsStatic => method.Modifiers.Any(SyntaxKind.StaticKeyword);

        public bool IsExtern => method.Modifiers.Any(SyntaxKind.ExternKeyword);

        public bool HasBodyOrExpressionBody => method.BodyOrExpressionBody is not null;

        public SyntaxNode BodyOrExpressionBody => (method?.Body as SyntaxNode) ?? method?.ExpressionBody()?.Expression;

        public bool ContainsMethodInvocation(SemanticModel semanticModel, Func<InvocationExpressionSyntax, bool> syntaxPredicate, Func<IMethodSymbol, bool> symbolPredicate)
        {
            var childNodes = method?.Body?.DescendantNodes()
                ?? method?.ExpressionBody()?.DescendantNodes()
                ?? Enumerable.Empty<SyntaxNode>();

            // See issue: https://github.com/SonarSource/sonar-dotnet/issues/416
            // Where clause excludes nodes that are not defined on the same SyntaxTree as the SemanticModel
            // (because of partial definition).
            // More details: https://github.com/dotnet/roslyn/issues/18730
            return childNodes
                .OfType<InvocationExpressionSyntax>()
                .Where(syntaxPredicate)
                .Select(x => x.Expression.SyntaxTree.SemanticModelOrDefault(semanticModel)?.GetSymbolInfo(x.Expression).Symbol)
                .OfType<IMethodSymbol>()
                .Any(symbolPredicate);
        }

        public SyntaxToken? IdentifierOrDefault =>
            method switch
            {
                ConstructorDeclarationSyntax constructor => (SyntaxToken?)constructor.Identifier,
                DestructorDeclarationSyntax destructor => (SyntaxToken?)destructor.Identifier,
                MethodDeclarationSyntax methodDeclaration => (SyntaxToken?)methodDeclaration.Identifier,
                _ => null,
            };

        public Location IdentifierLocation => method.IdentifierOrDefault?.GetLocation();
    }
}
