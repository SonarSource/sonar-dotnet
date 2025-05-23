﻿/*
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

namespace SonarAnalyzer.Rules.CSharp
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
