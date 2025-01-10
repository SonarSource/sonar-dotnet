/*
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TypeMemberVisibility : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3059";
        private const string MessageFormat = "Types should not have members with visibility set higher than the type's visibility";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        private static readonly HashSet<SyntaxKind> TypeKinds =
        [
            SyntaxKind.ClassDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKindEx.RecordDeclaration,
            SyntaxKindEx.RecordStructDeclaration,
            SyntaxKind.StructDeclaration,
        ];

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    if (c.IsRedundantPositionalRecordContext())
                    {
                        return;
                    }
                    var typeDeclaration = (BaseTypeDeclarationSyntax)c.Node;
                    var secondaryLocations = GetInvalidMemberLocations(c.SemanticModel, typeDeclaration);
                    if (secondaryLocations.Any())
                    {
                        c.ReportIssue(Rule, typeDeclaration.Identifier, secondaryLocations);
                    }
                },
                [.. TypeKinds]);

        private static SecondaryLocation[] GetInvalidMemberLocations(SemanticModel semanticModel, BaseTypeDeclarationSyntax type)
        {
            var parentType = GetParentType(type);
            if (parentType is null && type.Modifiers.AnyOfKind(SyntaxKind.InternalKeyword))
            {
                return type.DescendantNodes()
                           .OfType<MemberDeclarationSyntax>()
                           .Where(x => x.Modifiers().AnyOfKind(SyntaxKind.PublicKeyword)
                                       && !x.Modifiers().AnyOfKind(SyntaxKind.OverrideKeyword) // Overridden member need to keep the visibility of the base declaration
                                       && !(x.Kind() is SyntaxKind.OperatorDeclaration or SyntaxKind.ConversionOperatorDeclaration) // Operators must be public
                                       && !IsInterfaceImplementation(semanticModel, x))
                           .Select(x => x.Modifiers().Single(modifier => modifier.IsKind(SyntaxKind.PublicKeyword)).ToSecondaryLocation())
                           .ToArray();
            }

            return [];
        }

        private static bool IsInterfaceImplementation(SemanticModel semanticModel, MemberDeclarationSyntax declaration) =>
            semanticModel.GetDeclaredSymbol(declaration)?.GetInterfaceMember() is not null;

        private static BaseTypeDeclarationSyntax GetParentType(SyntaxNode node) =>
            (BaseTypeDeclarationSyntax)node.Ancestors().FirstOrDefault(x => x.IsAnyKind(TypeKinds));
    }
}
