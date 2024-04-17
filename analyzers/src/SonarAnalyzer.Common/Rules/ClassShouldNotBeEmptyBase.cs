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

namespace SonarAnalyzer.Rules;

public abstract class ClassShouldNotBeEmptyBase<TSyntaxKind, TDeclarationSyntax> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TDeclarationSyntax : SyntaxNode
{
    private const string DiagnosticId = "S2094";

    private static readonly ImmutableArray<KnownType> BaseClassesToIgnore = ImmutableArray.Create(
        KnownType.Microsoft_AspNetCore_Mvc_RazorPages_PageModel,
        KnownType.System_Attribute,
        KnownType.System_Exception);

    private static readonly IEnumerable<string> IgnoredNames = ["AssemblyDoc", "NamespaceDoc"]; // https://github.com/Doraku/DefaultDocumentation

    private static readonly IEnumerable<string> IgnoredSuffixes = ["Command", "Event", "Message"];

    protected abstract bool IsEmptyAndNotPartial(SyntaxNode node);
    protected abstract TDeclarationSyntax GetIfHasDeclaredBaseClassOrInterface(SyntaxNode node);
    protected abstract bool HasInterfaceOrGenericBaseClass(TDeclarationSyntax declaration);
    protected abstract bool HasAnyAttribute(SyntaxNode node);
    protected abstract string DeclarationTypeKeyword(SyntaxNode node);
    protected abstract bool HasConditionalCompilationDirectives(SyntaxNode node);

    protected override string MessageFormat => "Remove this empty {0}, write its code or make it an \"interface\".";

    protected ClassShouldNotBeEmptyBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                if (Language.Syntax.NodeIdentifier(c.Node) is { IsMissing: false } identifier
                    && IsEmptyAndNotPartial(c.Node)
                    && !HasAnyAttribute(c.Node)
                    && !HasConditionalCompilationDirectives(c.Node)
                    && !ShouldIgnoreBecauseOfName(identifier)
                    && !ShouldIgnoreBecauseOfBaseClassOrInterface(c.Node, c.SemanticModel))
                {
                    c.ReportIssue(Diagnostic.Create(Rule, identifier.GetLocation(), DeclarationTypeKeyword(c.Node)));
                }
            },
            Language.SyntaxKind.ClassAndRecordClassDeclarations);

    private static bool ShouldIgnoreBecauseOfName(SyntaxToken identifier) =>
        IgnoredNames.Contains(identifier.ValueText)
        || IgnoredSuffixes.Any(identifier.ValueText.EndsWith);

    private bool ShouldIgnoreBecauseOfBaseClassOrInterface(SyntaxNode node, SemanticModel model) =>
        GetIfHasDeclaredBaseClassOrInterface(node) is { } declaration
        && (HasInterfaceOrGenericBaseClass(declaration) || ShouldIgnoreType(declaration, model) || HasNonPublicDefaultConstructor(declaration, model));

    private static bool ShouldIgnoreType(TDeclarationSyntax node, SemanticModel model) =>
        model.GetDeclaredSymbol(node) is INamedTypeSymbol classSymbol
            && (classSymbol.BaseType is { IsAbstract: true }
            || classSymbol.DerivesFromAny(BaseClassesToIgnore)
            || classSymbol.Interfaces.Any(x => !x.Is(KnownType.System_IEquatable_T))); // every record type implicitly implements System.IEquatable<T>

    private static bool HasNonPublicDefaultConstructor(TDeclarationSyntax declaration, SemanticModel model) =>
        model.GetDeclaredSymbol(declaration) is INamedTypeSymbol classSymbol
        && classSymbol.BaseType.Constructors.FirstOrDefault(x => x.Parameters.Length == 0) is { } constructor
        && (constructor.GetEffectiveAccessibility() < classSymbol.DeclaredAccessibility
            // GetEffectiveAccessibility is not handling Protected
            || (constructor.DeclaredAccessibility == Accessibility.Protected && classSymbol.DeclaredAccessibility > Accessibility.Protected));
}
