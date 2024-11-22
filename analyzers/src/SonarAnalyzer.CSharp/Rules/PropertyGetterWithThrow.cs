/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
    public sealed class PropertyGetterWithThrow : PropertyGetterWithThrowBase<SyntaxKind, AccessorDeclarationSyntax>
    {
        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override SyntaxKind ThrowSyntaxKind => SyntaxKind.ThrowStatement;

        protected override bool IsGetter(AccessorDeclarationSyntax propertyGetter) =>
            propertyGetter.IsKind(SyntaxKind.GetAccessorDeclaration);
        protected override bool IsIndexer(AccessorDeclarationSyntax propertyGetter) =>
            propertyGetter.Parent.Parent is IndexerDeclarationSyntax;

        protected override SyntaxNode GetThrowExpression(SyntaxNode syntaxNode) =>
            ((ThrowStatementSyntax)syntaxNode).Expression;

        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer =>
            CSharpGeneratedCodeRecognizer.Instance;
    }
}
