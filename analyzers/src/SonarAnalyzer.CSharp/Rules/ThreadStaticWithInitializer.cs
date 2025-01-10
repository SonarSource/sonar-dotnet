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
    public sealed class ThreadStaticWithInitializer : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2996";
        private const string MessageFormat = "Remove this initialization of '{0}' or make it lazy.";

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var fieldDeclaration = (FieldDeclarationSyntax)c.Node;

                    if (!fieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword)
                        || !HasThreadStaticAttribute(fieldDeclaration.AttributeLists, c.SemanticModel))
                    {
                        return;
                    }

                    foreach (var variableDeclaratorSyntax in fieldDeclaration.Declaration.Variables.Where(variableDeclaratorSyntax => variableDeclaratorSyntax.Initializer != null))
                    {
                        c.ReportIssue(Rule, variableDeclaratorSyntax.Initializer, variableDeclaratorSyntax.Identifier.ValueText);
                    }
                },
                SyntaxKind.FieldDeclaration);
        private static bool HasThreadStaticAttribute(SyntaxList<AttributeListSyntax> attributeLists, SemanticModel semanticModel) =>
            attributeLists.Any()
            && attributeLists.Any(attributeList => attributeList.Attributes.Any(attribute => attribute.IsKnownType(KnownType.System_ThreadStaticAttribute, semanticModel)));
    }
}
