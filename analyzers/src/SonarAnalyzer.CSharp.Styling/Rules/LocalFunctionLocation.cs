/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LocalFunctionLocation : StylingAnalyzer
{
    public LocalFunctionLocation() : base("T0015", "This local function should be at the end of the method.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var function = (LocalFunctionStatementSyntax)c.Node;
                if (!IsInTopLevelBlock(function) || !IsLastStatement(function))
                {
                    c.ReportIssue(Rule, function.Identifier);
                }
            },
            SyntaxKind.LocalFunctionStatement);

    private static bool IsInTopLevelBlock(LocalFunctionStatementSyntax function) =>
        function.Parent is GlobalStatementSyntax or BlockSyntax
        {
            Parent: BaseMethodDeclarationSyntax
                    or BasePropertyDeclarationSyntax
                    or AccessorDeclarationSyntax
        };

    private static bool IsLastStatement(LocalFunctionStatementSyntax function) =>
        function is { FollowingStatement: null or LocalFunctionStatementSyntax };
}
