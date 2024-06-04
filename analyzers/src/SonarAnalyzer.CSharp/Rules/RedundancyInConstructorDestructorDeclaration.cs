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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedundancyInConstructorDestructorDeclaration : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S3253";
    private const string MessageFormat = "Remove this redundant {0}.";

    private static readonly SyntaxKind[] TypesWithPrimaryConstructorDeclarations =
    [
        SyntaxKind.ClassDeclaration,
        SyntaxKind.StructDeclaration,
        SyntaxKindEx.RecordClassDeclaration,
        SyntaxKindEx.RecordStructDeclaration
    ];

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
        if (!Excluded(typeDeclaration)
            && typeDeclaration.ParameterList() is { Parameters.Count: 0 } parameterList
            && !IsStructWithInitializedFieldOrProperty(typeDeclaration, context.SemanticModel))
        {
            context.ReportIssue(Rule, parameterList, "primary constructor");
        }

        static bool Excluded(TypeDeclarationSyntax node) =>
            RecordDeclarationSyntaxWrapper.IsInstance(node)
            && ((RecordDeclarationSyntaxWrapper)node).BaseList is { } baseList
            && baseList.DescendantNodes().OfType<ArgumentSyntax>().Any();
    }

    private static void CheckDestructorDeclaration(SonarSyntaxNodeReportingContext context)
    {
        var destructorDeclaration = (DestructorDeclarationSyntax)context.Node;

        if (destructorDeclaration.Body is { Statements.Count: 0 })
        {
            context.ReportIssue(Rule, destructorDeclaration, "destructor");
        }
    }

    private static void CheckConstructorDeclaration(SonarSyntaxNodeReportingContext context)
    {
        var constructorDeclaration = (ConstructorDeclarationSyntax)context.Node;

        if (IsConstructorRedundant(constructorDeclaration, context.SemanticModel))
        {
            context.ReportIssue(Rule, constructorDeclaration, "constructor");
            return;
        }

        var initializer = constructorDeclaration.Initializer;
        if (initializer != null
            && IsInitializerRedundant(initializer))
        {
            context.ReportIssue(Rule, initializer, "'base()' call");
        }
    }

    private static bool IsInitializerRedundant(ConstructorInitializerSyntax initializer) =>
        initializer.IsKind(SyntaxKind.BaseConstructorInitializer)
        && initializer.ArgumentList != null
        && !initializer.ArgumentList.Arguments.Any();

    private static bool IsConstructorRedundant(ConstructorDeclarationSyntax constructorDeclaration, SemanticModel semanticModel) =>
        constructorDeclaration is { ParameterList.Parameters.Count: 0, Body.Statements.Count: 0, Parent: BaseTypeDeclarationSyntax typeDeclaration }
        && !IsStructWithInitializedFieldOrProperty(typeDeclaration, semanticModel)
        && (IsSinglePublicConstructor(constructorDeclaration, semanticModel)
            || constructorDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword));

    private static bool IsStructWithInitializedFieldOrProperty(BaseTypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel) =>
        typeDeclaration.Kind() is SyntaxKind.StructDeclaration or SyntaxKindEx.RecordStructDeclaration
        && semanticModel.GetDeclaredSymbol(typeDeclaration) is { } typeSymbol
        && typeSymbol.GetMembers().Any(f => f.Kind is SymbolKind.Field or SymbolKind.Property && f.DeclaringSyntaxReferences.Any(d => d.GetSyntax().GetInitializer() is not null));

    private static bool IsSinglePublicConstructor(ConstructorDeclarationSyntax constructorDeclaration, SemanticModel semanticModel) =>
        constructorDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword)
        && IsInitializerEmptyOrRedundant(constructorDeclaration.Initializer)
        && constructorDeclaration is { Parent: BaseTypeDeclarationSyntax typeDeclaration }
        && TypeHasExactlyOneConstructor(typeDeclaration, semanticModel);

    private static bool IsInitializerEmptyOrRedundant(ConstructorInitializerSyntax initializer) =>
        initializer is null
        || (initializer.ArgumentList.Arguments.Count == 0
            && initializer.ThisOrBaseKeyword.IsKind(SyntaxKind.BaseKeyword));

    private static bool TypeHasExactlyOneConstructor(BaseTypeDeclarationSyntax containingTypeDeclaration, SemanticModel semanticModel) =>
        semanticModel.GetDeclaredSymbol(containingTypeDeclaration) is { } typeSymbol
        && typeSymbol
            .GetMembers(".ctor")
            .OfType<IMethodSymbol>()
            .Count(m => m is { MethodKind: MethodKind.Constructor, IsImplicitlyDeclared: false, IsStatic: false }) == 1;
}
