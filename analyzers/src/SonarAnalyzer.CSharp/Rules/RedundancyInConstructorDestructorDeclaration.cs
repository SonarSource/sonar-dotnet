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
    public sealed class RedundancyInConstructorDestructorDeclaration : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3253";
        private const string MessageFormat = "Remove this redundant {0}.";

        private static readonly SyntaxKind[] TypesWithPrimaryConstructorDeclarations =
        {
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKindEx.RecordClassDeclaration,
            SyntaxKindEx.RecordStructDeclaration
        };

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(CheckConstructorDeclaration, SyntaxKind.ConstructorDeclaration);
            context.RegisterNodeAction(CheckDestructorDeclaration, SyntaxKind.DestructorDeclaration);
            context.RegisterNodeAction(CheckTypesWithPrimaryConstructor, TypesWithPrimaryConstructorDeclarations);
        }

        private static void CheckTypesWithPrimaryConstructor(SonarSyntaxNodeReportingContext context)
        {
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;
            if (typeDeclaration.ParameterList() is { Parameters.Count: 0 } parameterList
                && !IsStructWithInitializedFieldOrProperty(typeDeclaration, new Lazy<INamedTypeSymbol>(() => (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(context.Node))))
            {
                context.ReportIssue(Diagnostic.Create(Rule, parameterList.GetLocation(), "primary constructor"));
            }
        }

        private static void CheckDestructorDeclaration(SonarSyntaxNodeReportingContext context)
        {
            var destructorDeclaration = (DestructorDeclarationSyntax)context.Node;

            if (destructorDeclaration.Body is { Statements.Count: 0 })
            {
                context.ReportIssue(Diagnostic.Create(Rule, destructorDeclaration.GetLocation(), "destructor"));
            }
        }

        private static void CheckConstructorDeclaration(SonarSyntaxNodeReportingContext context)
        {
            var constructorDeclaration = (ConstructorDeclarationSyntax)context.Node;

            if (IsConstructorRedundant(constructorDeclaration, new Lazy<INamedTypeSymbol>(() => context.SemanticModel.GetDeclaredSymbol(constructorDeclaration)?.ContainingType)))
            {
                context.ReportIssue(Diagnostic.Create(Rule, constructorDeclaration.GetLocation(), "constructor"));
                return;
            }

            var initializer = constructorDeclaration.Initializer;
            if (initializer != null
                && IsInitializerRedundant(initializer))
            {
                context.ReportIssue(Diagnostic.Create(Rule, initializer.GetLocation(), "'base()' call"));
            }
        }

        private static bool IsInitializerRedundant(ConstructorInitializerSyntax initializer) =>
            initializer.IsKind(SyntaxKind.BaseConstructorInitializer)
            && initializer.ArgumentList != null
            && !initializer.ArgumentList.Arguments.Any();

        private static bool IsConstructorRedundant(ConstructorDeclarationSyntax constructorDeclaration, Lazy<INamedTypeSymbol> typeSymbol) =>
            constructorDeclaration is { ParameterList.Parameters.Count: 0, Body.Statements.Count: 0 }
            && !IsStructWithInitializedFieldOrProperty(constructorDeclaration.Parent, typeSymbol)
            && (IsSinglePublicConstructor(constructorDeclaration, typeSymbol)
                || constructorDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword));

        private static bool IsStructWithInitializedFieldOrProperty(SyntaxNode parentType, Lazy<INamedTypeSymbol> typeSymbol) =>
            parentType.IsAnyKind(SyntaxKind.StructDeclaration, SyntaxKindEx.RecordStructDeclaration)
            && ContainsInitializedFieldOrProperty(typeSymbol.Value);

        private static bool IsSinglePublicConstructor(ConstructorDeclarationSyntax constructorDeclaration, Lazy<INamedTypeSymbol> typeSymbol) =>
            constructorDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword)
            && IsInitializerEmptyOrRedundant(constructorDeclaration.Initializer)
            && TypeHasExactlyOneConstructor(typeSymbol.Value);

        private static bool IsInitializerEmptyOrRedundant(ConstructorInitializerSyntax initializer) =>
            initializer is null
            || (initializer.ArgumentList.Arguments.Count == 0
                && initializer.ThisOrBaseKeyword.IsKind(SyntaxKind.BaseKeyword));

        private static bool TypeHasExactlyOneConstructor(INamedTypeSymbol typeSymbol) =>
            typeSymbol != null
            && typeSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Count(m => m.MethodKind == MethodKind.Constructor && !m.IsImplicitlyDeclared) == 1;

        private static bool ContainsInitializedFieldOrProperty(INamedTypeSymbol typeSymbol) =>
            typeSymbol != null && typeSymbol.GetMembers().Any(f => f.DeclaringSyntaxReferences.Any(d => d.GetSyntax().GetInitializer() is not null));
    }
}
