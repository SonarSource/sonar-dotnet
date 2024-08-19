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

namespace SonarAnalyzer.Rules.VisualBasic;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class PublicMethodWithMultidimensionalArray : PublicMethodWithMultidimensionalArrayBase<SyntaxKind>
{
    private static readonly ImmutableArray<SyntaxKind> KindsOfInterest = ImmutableArray.Create(SyntaxKind.SubStatement, SyntaxKind.FunctionStatement, SyntaxKind.ConstructorBlock);

    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;
    protected override ImmutableArray<SyntaxKind> SyntaxKindsOfInterest => KindsOfInterest;

    protected override Location GetIssueLocation(SyntaxNode node) =>
        node is ConstructorBlockSyntax x
            ? x.SubNewStatement.NewKeyword.GetLocation()
            : Language.Syntax.NodeIdentifier(node)?.GetLocation();

    protected override string GetType(SyntaxNode node) =>
        node is MethodStatementSyntax ? "method" : "constructor";
}
