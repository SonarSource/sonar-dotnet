/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Rules
{
    public abstract class EnumNameHasEnumSuffixBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S2344";
        private readonly IEnumerable<string> nameEndings = ImmutableArray.Create("enum", "flags");

        protected override string MessageFormat => "Rename this enumeration to remove the '{0}' suffix.";

        protected EnumNameHasEnumSuffixBase() : base(DiagnosticId) { }

        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
                {
                    if (Language.Syntax.NodeIdentifier(c.Node) is { } identifier
                        && nameEndings.FirstOrDefault(ending => identifier.ValueText.EndsWith(ending, System.StringComparison.OrdinalIgnoreCase)) is { } nameEnding)
                    {
                        c.ReportIssue(Rule, identifier, identifier.ValueText.Substring(identifier.ValueText.Length - nameEnding.Length));
                    }
                },
                Language.SyntaxKind.EnumDeclaration);
    }
}
