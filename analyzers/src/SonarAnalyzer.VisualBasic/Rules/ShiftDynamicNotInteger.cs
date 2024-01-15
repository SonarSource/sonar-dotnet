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
