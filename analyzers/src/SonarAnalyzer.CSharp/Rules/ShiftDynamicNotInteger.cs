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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ShiftDynamicNotInteger : ShiftDynamicNotIntegerBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override bool ShouldRaise(SemanticModel model, SyntaxNode left, SyntaxNode right) =>
        left.IsDynamic(model) && !IsConvertibleToInt(right, model);

    protected override bool CanBeConvertedTo(SyntaxNode expression, ITypeSymbol type, SemanticModel model) =>
        model.ClassifyConversion(expression as ExpressionSyntax, type) is { Exists: true } and ({ IsIdentity: true } or { IsImplicit: true });
}
