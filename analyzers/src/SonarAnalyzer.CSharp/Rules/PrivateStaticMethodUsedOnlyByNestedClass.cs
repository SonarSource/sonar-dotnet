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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PrivateStaticMethodUsedOnlyByNestedClass : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S3398";
    private const string MessageFormat = "Move the method inside '{0}'.";

    private static readonly SyntaxKind[] AnalyzedSyntaxKinds = new[]
    {
        SyntaxKind.ClassDeclaration,
        SyntaxKind.StructDeclaration,
        SyntaxKind.InterfaceDeclaration,
        SyntaxKindEx.RecordClassDeclaration,
        SyntaxKindEx.RecordStructDeclaration
    };

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var declaredType = (TypeDeclarationSyntax)c.Node;

                if (!IsPartial(declaredType)
                    && NestedTypeDeclarationsOf(declaredType) is { Length: > 0 } nestedTypes
                    && PrivateStaticMethodsOf(declaredType) is { Length: > 0 } candidates)
                {
                    var references = PotentialReferencesOfMethodsInsideType(candidates, declaredType)
                                        .Where(x => x.Value.Any(id => ContainingTypeDeclaration(id) != declaredType))
                                        .Select(x => new { Refs = x, MethodSymbol = c.SemanticModel.GetDeclaredSymbol(x.Key) })
                                        .Select(x => new { MethodDeclaration = x.Refs.Key, References = x.Refs.Value.Where(t => c.SemanticModel.GetSymbolInfo(t).Symbol is IMethodSymbol { } methodReference && (methodReference == x.MethodSymbol || methodReference.ConstructedFrom == x.MethodSymbol) && ContainingMethodDeclaration(t) != x.Refs.Key).Select(t => new { Identifier = t, Type = ContainingTypeDeclaration(t) }) })
                                        .Where(x => x.References.Any())
                                        .ToArray();

                    foreach (var reference in references)
                    {
                        // scenario 1: used in outer class
                        if (reference.References.Any(x => x.Type == declaredType))
                        {
                            continue;
                        }

                        // scenario 2: used only in one of the nested classes
                        if (reference.References.Select(x => x.Type).Distinct().Count() == 1)
                        {
                            string nestedClassName = reference.References.First().Type.Identifier.ValueText;
                            c.ReportIssue(Diagnostic.Create(Rule, reference.MethodDeclaration.Identifier.GetLocation(), nestedClassName));
                        }

                        // scenario 3: used by multiple nested classes
                    }
                    }
            },
            AnalyzedSyntaxKinds);

    private static MethodDeclarationSyntax[] PrivateStaticMethodsOf(TypeDeclarationSyntax type) =>
        type.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(x => IsPrivateAndStatic(x, type))
                .ToArray();

    private static TypeDeclarationSyntax[] NestedTypeDeclarationsOf(TypeDeclarationSyntax type) =>
        type.Members
            .OfType<TypeDeclarationSyntax>()
            .Where(x => x is ClassDeclarationSyntax or StructDeclarationSyntax or InterfaceDeclarationSyntax
                        || RecordDeclarationSyntaxWrapper.IsInstance(x))
            .ToArray();

    private static bool IsPrivateAndStatic(MethodDeclarationSyntax method, TypeDeclarationSyntax containingType) =>
        method.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword))
        && ((HasAnyModifier(method, SyntaxKind.PrivateKeyword) && !HasAnyModifier(method, SyntaxKind.ProtectedKeyword))
           || (!HasAnyModifier(method, SyntaxKind.PublicKeyword, SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword) && IsClassOrRecordClassOrInterfaceDeclaration(containingType)));

    private static bool IsPartial(TypeDeclarationSyntax type) =>
        type.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword));

    private static bool IsClassOrRecordClassOrInterfaceDeclaration(TypeDeclarationSyntax type) =>
        type is ClassDeclarationSyntax or InterfaceDeclarationSyntax
        || (RecordDeclarationSyntaxWrapper.IsInstance(type) && !((RecordDeclarationSyntaxWrapper)type).ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword));

    private static bool HasAnyModifier(MethodDeclarationSyntax method, params SyntaxKind[] modifiers) =>
        method.Modifiers.Any(x => x.IsAnyKind(modifiers));

    private static IDictionary<MethodDeclarationSyntax, List<IdentifierNameSyntax>> PotentialReferencesOfMethodsInsideType(IEnumerable<MethodDeclarationSyntax> methods, TypeDeclarationSyntax type)
    {
        var collector = new PotentialMethodReferenceCollector(methods);
        collector.Visit(type);
        return collector.PotentialMethodReferences;
    }

    private static TypeDeclarationSyntax ContainingTypeDeclaration(IdentifierNameSyntax identifier) =>
        identifier
            .Ancestors()
            .OfType<TypeDeclarationSyntax>()
            .First();

    private static MethodDeclarationSyntax ContainingMethodDeclaration(IdentifierNameSyntax identifier) =>
        identifier
            .Ancestors()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault();

    private record MethodAndReferences(MethodDeclarationSyntax SyntaxNode, ISymbol MethodSymbol, ISymbol[] PotentialMethodReferences);

    private class PotentialMethodReferenceCollector : CSharpSyntaxWalker
    {
        private readonly ISet<MethodDeclarationSyntax> methodsToFind;
        internal Dictionary<MethodDeclarationSyntax, List<IdentifierNameSyntax>> PotentialMethodReferences { get; } = new();

        public PotentialMethodReferenceCollector(IEnumerable<MethodDeclarationSyntax> methodsToFind)
        {
            this.methodsToFind = new HashSet<MethodDeclarationSyntax>(methodsToFind);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax identifier)
        {
            if (methodsToFind.FirstOrDefault(x => x.Identifier.ValueText == identifier.Identifier.ValueText) is { } method)
            {
                var referenceList = PotentialMethodReferences.GetOrAdd(method, _ => new());
                referenceList.Add(identifier);
            }
        }
    }
}
