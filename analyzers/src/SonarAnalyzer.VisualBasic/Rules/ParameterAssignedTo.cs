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
    public sealed class ParameterAssignedTo : ParameterAssignedToBase<SyntaxKind, IdentifierNameSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        protected override bool IsAssignmentToCatchVariable(ISymbol symbol, SyntaxNode node) =>
            // This could mimic the C# variant too, but that doesn't work https://github.com/dotnet/roslyn/issues/6209
            symbol is ILocalSymbol localSymbol
            && localSymbol.Locations.FirstOrDefault() is { } location
            && node.SyntaxTree.GetRoot().FindNode(location.SourceSpan, getInnermostNodeForTie: true) is IdentifierNameSyntax declarationName
            && declarationName.Parent is CatchStatementSyntax catchStatement
            && catchStatement.IdentifierName == declarationName;
    }
}
