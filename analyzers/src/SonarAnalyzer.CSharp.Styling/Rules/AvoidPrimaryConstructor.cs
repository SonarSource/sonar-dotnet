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
public class AvoidPrimaryConstructor : StylingAnalyzer
{
    public AvoidPrimaryConstructor() : base("T0043", "Do not use primary constructors.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
        {
            if (c.Node.Parent is ClassDeclarationSyntax or StructDeclarationSyntax)
            {
                c.ReportIssue(Rule, c.Node);
            }
        }, SyntaxKind.ParameterList);
}
