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
    public sealed class DeliveringDebugFeaturesInProduction : DeliveringDebugFeaturesInProductionBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        public DeliveringDebugFeaturesInProduction() : this(AnalyzerConfiguration.Hotspot) { }

        internal /*for testing*/ DeliveringDebugFeaturesInProduction(IAnalyzerConfiguration configuration) : base(configuration) { }

        // For simplicity we will avoid creating noise if the validation is invoked in the same method (https://github.com/SonarSource/sonar-dotnet/issues/5032)
        protected override bool IsDevelopmentCheckInvoked(SyntaxNode node, SemanticModel semanticModel) =>
            node.EnclosingScope()
                .DescendantNodes()
                .Any(x => IsDevelopmentCheck(x, semanticModel));

        protected override bool IsInDevelopmentContext(SyntaxNode node) =>
            node.Ancestors()
                .OfType<ClassDeclarationSyntax>()
                .Any(x => x.Identifier.Text == StartupDevelopment);

        private bool IsDevelopmentCheck(SyntaxNode node, SemanticModel semanticModel) =>
            node is InvocationExpressionSyntax condition
            && IsValidationMethod(semanticModel, condition, condition.Expression.GetIdentifier()?.ValueText);
    }
}
