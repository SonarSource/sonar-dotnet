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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class ShiftDynamicNotInteger : ShiftDynamicNotIntegerBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        protected override bool ShouldRaise(SemanticModel semanticModel, SyntaxNode left, SyntaxNode right) =>
            IsObject(left, semanticModel) || !IsConvertibleToInt(right, semanticModel);

        protected override bool CanBeConvertedTo(SyntaxNode expression, ITypeSymbol type, SemanticModel semanticModel) =>
            expression.IsKind(SyntaxKind.NothingLiteralExpression) // x >> Nothing will not throw, so ignore
            || semanticModel.GetTypeInfo(expression).Type.IsAny(KnownType.IntegralNumbers);

        private static bool IsObject(SyntaxNode expression, SemanticModel semanticModel) =>
            semanticModel.GetTypeInfo(expression).Type is { } type
            && type.Is(KnownType.System_Object);
    }
}
