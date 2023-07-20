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

namespace SonarAnalyzer.Rules
{
    public abstract class GenericInheritanceShouldNotBeRecursiveBase<TSyntaxKind, TDeclaration> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
        where TDeclaration : SyntaxNode
    {
        protected const string DiagnosticId = "S3464";

        protected abstract TSyntaxKind[] SyntaxKinds { get; }

        protected abstract INamedTypeSymbol GetNamedTypeSymbol(TDeclaration declaration, SemanticModel semanticModel);
        protected abstract Location GetLocation(TDeclaration declaration);
        protected abstract SyntaxToken GetKeyword(TDeclaration declaration);

        protected override string MessageFormat => "Refactor this {0} so that the generic inheritance chain is not recursive.";

        protected GenericInheritanceShouldNotBeRecursiveBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer,
                c =>
                {
                    var declaration = (TDeclaration)c.Node;

                    if (!c.IsRedundantPositionalRecordContext()
                        && IsRecursiveInheritance(GetNamedTypeSymbol(declaration, c.SemanticModel)))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, GetLocation(declaration), GetKeyword(declaration)));
                    }
                },
                SyntaxKinds);

        private static IEnumerable<INamedTypeSymbol> GetBaseTypes(INamedTypeSymbol typeSymbol)
        {
            var interfaces = typeSymbol.Interfaces.Where(IsGenericType);
            return typeSymbol.IsClass()
                ? interfaces.Concat(new[] { typeSymbol.BaseType })
                : interfaces;
        }

        private static bool HasRecursiveGenericSubstitution(INamedTypeSymbol typeSymbol, INamedTypeSymbol declaredType)
        {
            bool IsSameAsDeclaredType(INamedTypeSymbol type) =>
                type.OriginalDefinition.Equals(declaredType) && HasSubstitutedTypeArguments(type);

            bool ContainsRecursiveGenericSubstitution(IEnumerable<ITypeSymbol> types) =>
                types.OfType<INamedTypeSymbol>()
                    .Any(type => IsSameAsDeclaredType(type) || ContainsRecursiveGenericSubstitution(type.TypeArguments));

            return ContainsRecursiveGenericSubstitution(typeSymbol.TypeArguments);
        }

        private static bool IsGenericType(INamedTypeSymbol type) =>
            type != null && type.IsGenericType;

        private static bool HasSubstitutedTypeArguments(INamedTypeSymbol type) =>
            type.TypeArguments.OfType<INamedTypeSymbol>().Any();

        private static bool IsRecursiveInheritance(INamedTypeSymbol typeSymbol)
        {
            if (!IsGenericType(typeSymbol))
            {
                return false;
            }

            var baseTypes = GetBaseTypes(typeSymbol);

            return baseTypes.Any(t => IsGenericType(t) && HasRecursiveGenericSubstitution(t, typeSymbol));
        }
    }
}
