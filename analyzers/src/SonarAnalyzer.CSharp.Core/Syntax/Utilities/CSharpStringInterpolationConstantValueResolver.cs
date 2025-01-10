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

namespace SonarAnalyzer.CSharp.Core.Syntax.Utilities;

public class CSharpStringInterpolationConstantValueResolver : StringInterpolationConstantValueResolver<SyntaxKind,
                                                                                                       InterpolatedStringExpressionSyntax,
                                                                                                       InterpolatedStringContentSyntax,
                                                                                                       InterpolationSyntax,
                                                                                                       InterpolatedStringTextSyntax>
{
    private static readonly Lazy<CSharpStringInterpolationConstantValueResolver> Singleton = new(() => new CSharpStringInterpolationConstantValueResolver());

    public static CSharpStringInterpolationConstantValueResolver Instance => Singleton.Value;

    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override IEnumerable<InterpolatedStringContentSyntax> Contents(InterpolatedStringExpressionSyntax interpolatedStringExpression) =>
        interpolatedStringExpression.Contents;

    protected override SyntaxToken TextToken(InterpolatedStringTextSyntax interpolatedStringText) =>
        interpolatedStringText.TextToken;
}
