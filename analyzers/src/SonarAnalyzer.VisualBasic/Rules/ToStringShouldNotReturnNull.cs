﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class ToStringShouldNotReturnNull : ToStringShouldNotReturnNullBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override SyntaxKind MethodKind => SyntaxKind.FunctionBlock;

    protected override IEnumerable<SyntaxNode> Conditionals(SyntaxNode node) =>
        node is TernaryConditionalExpressionSyntax conditional
        ? new SyntaxNode[] { conditional.WhenTrue, conditional.WhenFalse }
        : Array.Empty<SyntaxNode>();

    protected override bool IsLocalOrLambda(SyntaxNode node) =>
        node.IsKind(SyntaxKind.MultiLineFunctionLambdaExpression);
}
