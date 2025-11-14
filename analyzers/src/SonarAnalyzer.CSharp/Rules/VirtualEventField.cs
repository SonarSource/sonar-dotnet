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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class VirtualEventField : SonarDiagnosticAnalyzer
    {
        private const string MessageFormat = "Remove this 'virtual' modifier of {0}.";

        internal const string DiagnosticId = "S2290";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var eventField = (EventFieldDeclarationSyntax)c.Node;

                    if (eventField.Modifiers.Any(SyntaxKind.VirtualKeyword))
                    {
                        var virt = eventField.Modifiers.First(modifier => modifier.IsKind(SyntaxKind.VirtualKeyword));
                        var names = string.Join(", ", eventField.Declaration.Variables.Select(syntax => $"'{syntax.Identifier.ValueText}'").OrderBy(s => s).JoinAnd());
                        c.ReportIssue(Rule, virt, names);
                    }
                },
                SyntaxKind.EventFieldDeclaration);
    }
}
