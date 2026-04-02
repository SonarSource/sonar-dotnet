/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.VisualBasic.Rules;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class VariableUnused : VariableUnusedBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language { get; } = VisualBasicFacade.Instance;

    protected override bool IsExcludedDeclaration(SyntaxNode node) =>
        node is ModifiedIdentifierSyntax { Parent.Parent: UsingStatementSyntax or ForStatementSyntax or ForEachStatementSyntax }  // Using/For/For Each x ... in VB
                or ModifiedIdentifierSyntax { Parent: CatchStatementSyntax }                                                      // Catch e As Exception in VB
                or ModifiedIdentifierSyntax { Parent: CollectionRangeVariableSyntax or VariableNameEqualsSyntax };                // From x In / Select x = / Let x = / Into x = in VB LINQ
}
