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

using Microsoft.CodeAnalysis.CSharp;

namespace StyleCop.Analyzers.Lightup;

public static partial class SyntaxKindEx
{
    public const SyntaxKind GreaterThanGreaterThanGreaterThanToken = (SyntaxKind)8286;
    public const SyntaxKind GreaterThanGreaterThanGreaterThanEqualsToken = (SyntaxKind)8287;
    public const SyntaxKind NameOfKeyword = (SyntaxKind)8434;
    public const SyntaxKind SingleLineRawStringLiteralToken = (SyntaxKind)8518;
    public const SyntaxKind MultiLineRawStringLiteralToken = (SyntaxKind)8519;
    public const SyntaxKind Utf8StringLiteralToken = (SyntaxKind)8520;
    public const SyntaxKind Utf8SingleLineRawStringLiteralToken = (SyntaxKind)8521;
    public const SyntaxKind Utf8MultiLineRawStringLiteralToken = (SyntaxKind)8522;
    public const SyntaxKind PragmaChecksumDirectiveTrivia = (SyntaxKind)8560;
    public const SyntaxKind Utf8StringLiteralExpression = (SyntaxKind)8756;
    public const SyntaxKind GlobalStatement = (SyntaxKind)8841;
    public const SyntaxKind ArrowExpressionClause = (SyntaxKind)8917;
    public const SyntaxKind RelationalPattern = (SyntaxKind)9029;
    public const SyntaxKind TypePattern = (SyntaxKind)9030;
    public const SyntaxKind OrPattern = (SyntaxKind)9031;
    public const SyntaxKind AndPattern = (SyntaxKind)9032;
    public const SyntaxKind NotPattern = (SyntaxKind)9033;
    public const SyntaxKind FunctionPointerParameterList = (SyntaxKind)9058;
    public const SyntaxKind FunctionPointerCallingConvention = (SyntaxKind)9059;
    public const SyntaxKind RecordDeclaration = (SyntaxKind)9063;
    public const SyntaxKind FunctionPointerUnmanagedCallingConvention = (SyntaxKind)9067;
    public const SyntaxKind ExpressionColon = (SyntaxKind)9069;
    public const SyntaxKind InterpolatedSingleLineRawStringStartToken = (SyntaxKind)9072;
    public const SyntaxKind InterpolatedMultiLineRawStringStartToken = (SyntaxKind)9073;
    public const SyntaxKind InterpolatedRawStringEndToken = (SyntaxKind)9074;
    public const SyntaxKind ScopedType = (SyntaxKind)9075;
    public const SyntaxKind SpreadElement = (SyntaxKind)9078;
}
