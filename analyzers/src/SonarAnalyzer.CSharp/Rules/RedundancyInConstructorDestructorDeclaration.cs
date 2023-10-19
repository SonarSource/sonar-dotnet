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

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(CheckConstructorDeclaration, SyntaxKind.ConstructorDeclaration);
            context.RegisterNodeAction(CheckDestructorDeclaration, SyntaxKind.DestructorDeclaration);
            context.RegisterNodeAction(CheckPrimaryConstructorRef, SyntaxKind.ClassDeclaration, SyntaxKindEx.RecordClassDeclaration);
            context.RegisterNodeAction(CheckPrimaryConstructorVal, SyntaxKind.StructDeclaration, SyntaxKindEx.RecordStructDeclaration);
        }

        private static void CheckPrimaryConstructorRef(SonarSyntaxNodeReportingContext context) =>
            CheckPrimaryConstructor(context, checkInitializedFieldOrProperty: false);

        private static void CheckPrimaryConstructorVal(SonarSyntaxNodeReportingContext context) =>
            CheckPrimaryConstructor(context, checkInitializedFieldOrProperty: true);

        private static void CheckPrimaryConstructor(SonarSyntaxNodeReportingContext context, bool checkInitializedFieldOrProperty)
        {
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;
            if (TypeDeclarationSyntaxExtensions.ParameterList(typeDeclaration) is { Parameters.Count: 0 } parameterList)
            {
                if (checkInitializedFieldOrProperty && ContainsInitializedFieldOrProperty((INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(context.Node)))
                {
                    return;
                }
                context.ReportIssue(Diagnostic.Create(Rule, parameterList.GetLocation(), "primary constructor"));
            }
        }

        private static void CheckDestructorDeclaration(SonarSyntaxNodeReportingContext context)
        {
            var destructorDeclaration = (DestructorDeclarationSyntax)context.Node;

            if (IsBodyEmpty(destructorDeclaration.Body))
            {
                context.ReportIssue(Diagnostic.Create(Rule, destructorDeclaration.GetLocation(), "destructor"));
            }
        }

        private static void CheckConstructorDeclaration(SonarSyntaxNodeReportingContext context)
        {
            var constructorDeclaration = (ConstructorDeclarationSyntax)context.Node;

            if (IsConstructorRedundant(constructorDeclaration, context.SemanticModel))
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

        private static bool IsConstructorRedundant(ConstructorDeclarationSyntax constructorDeclaration, SemanticModel semanticModel) =>
            IsConstructorParameterless(constructorDeclaration)
            && IsBodyEmpty(constructorDeclaration.Body)
            && (IsSinglePublicConstructor(constructorDeclaration, semanticModel)
                || constructorDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword));

        private static bool IsSinglePublicConstructor(ConstructorDeclarationSyntax constructorDeclaration, SemanticModel semanticModel) =>
            constructorDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword)
            && IsInitializerEmptyOrRedundant(constructorDeclaration.Initializer)
            && TypeHasExactlyOneConstructor(constructorDeclaration, semanticModel);

        private static bool IsInitializerEmptyOrRedundant(ConstructorInitializerSyntax initializer)
        {
            if (initializer == null)
            {
                return true;
            }

            return initializer.ArgumentList.Arguments.Count == 0
                   && initializer.ThisOrBaseKeyword.IsKind(SyntaxKind.BaseKeyword);
        }

        private static bool TypeHasExactlyOneConstructor(BaseMethodDeclarationSyntax constructorDeclaration, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetDeclaredSymbol(constructorDeclaration);
            return symbol != null
                   && symbol.ContainingType
                            .GetMembers()
                            .OfType<IMethodSymbol>()
                            .Count(m => m.MethodKind == MethodKind.Constructor && !m.IsImplicitlyDeclared) == 1;
        }

        private static bool IsBodyEmpty(BlockSyntax block) =>
            block != null && !block.Statements.Any();

        private static bool IsConstructorParameterless(BaseMethodDeclarationSyntax constructorDeclaration) =>
            constructorDeclaration.ParameterList.Parameters.Count == 0;

        private static bool ContainsInitializedFieldOrProperty(INamedTypeSymbol symbol) =>
            symbol.GetMembers().OfType<IFieldSymbol>().Any(f => f.DeclaringSyntaxReferences.Any(d => d.GetSyntax() is VariableDeclaratorSyntax { Initializer: not null }))
            || symbol.GetMembers().OfType<IPropertySymbol>().Any(p => p.DeclaringSyntaxReferences.Any(d => d.GetSyntax() is PropertyDeclarationSyntax { Initializer: not null }));
    }
}
