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
