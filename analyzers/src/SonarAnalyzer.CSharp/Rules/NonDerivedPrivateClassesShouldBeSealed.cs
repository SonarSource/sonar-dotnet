﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NonDerivedPrivateClassesShouldBeSealed : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3260";
        private const string MessageFormat = "{0} {1} which are not derived in the current {2} should be marked as 'sealed'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly ImmutableHashSet<SyntaxKind> KindsToBeDescended = ImmutableHashSet.Create(
            SyntaxKind.CompilationUnit,
            SyntaxKind.NamespaceDeclaration,
            SyntaxKindEx.FileScopedNamespaceDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKindEx.RecordDeclaration,
            SyntaxKindEx.RecordStructDeclaration);

        private static readonly ImmutableHashSet<SyntaxKind> PossiblyVirtualKinds = ImmutableHashSet.Create(
            SyntaxKind.MethodDeclaration,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.EventDeclaration,
            SyntaxKind.IndexerDeclaration);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterTreeAction(c =>
            {
                var declarations = c.Tree.GetRoot()
                     .DescendantNodes(x => x.IsAnyKind(KindsToBeDescended))
                     .Where(x => x.Kind() is SyntaxKind.ClassDeclaration or SyntaxKindEx.RecordDeclaration)
                     .Select(x => (TypeDeclarationSyntax)x);

                var model = new Lazy<SemanticModel>(() => c.Compilation.GetSemanticModel(c.Tree));
                var symbols = new Lazy<List<INamedTypeSymbol>>(() => declarations.Select(x => model.Value.GetDeclaredSymbol(x)).ToList());

                foreach (var declaration in declarations)
                {
                    if (!IsSealed(declaration)
                        && !HasVirtualMembers(declaration)
                        && !IsPossiblyDerived(declaration, model, symbols, out var modifier, out var inheritanceScope))
                    {
                        var type = declaration.IsKind(SyntaxKind.ClassDeclaration) ? "classes" : "record classes";
                        c.ReportIssue(Rule, declaration.Identifier, modifier, type, inheritanceScope);
                    }
                }
            });

        private static bool HasVirtualMembers(TypeDeclarationSyntax typeDeclaration) =>
            typeDeclaration.Members
                .Where(member => member.IsAnyKind(PossiblyVirtualKinds))
                .Any(member => member.Modifiers().Any(SyntaxKind.VirtualKeyword));

        private static bool IsSealed(TypeDeclarationSyntax typeDeclaration) =>
            typeDeclaration.Modifiers.Any(SyntaxKind.SealedKeyword)
            || typeDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword)
            || typeDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword);

        private static bool IsPossiblyDerived(
            TypeDeclarationSyntax declaration,
            Lazy<SemanticModel> model,
            Lazy<List<INamedTypeSymbol>> otherSymbols,
            out string modifierDescription,
            out string scopeDescription)
        {
            if (declaration.Modifiers.Any(SyntaxKind.PrivateKeyword))
            {
                modifierDescription = "Private";
                scopeDescription = "assembly";
                var symbol = model.Value.GetDeclaredSymbol(declaration);
                return symbol.ContainingType.GetAllNamedTypes().Any(other =>
                    !other.MetadataName.Equals(symbol.MetadataName)
                    && other.DerivesFrom(symbol));
            }
            if (declaration.Modifiers.Any(SyntaxKindEx.FileKeyword))
            {
                modifierDescription = "File-scoped";
                scopeDescription = "file";
                var symbol = model.Value.GetDeclaredSymbol(declaration);
                return otherSymbols.Value.Exists(other =>
                    !other.MetadataName.Equals(symbol.MetadataName)
                    && other.DerivesFrom(symbol));
            }

            modifierDescription = scopeDescription =  string.Empty;
            return true;
        }
    }
}
