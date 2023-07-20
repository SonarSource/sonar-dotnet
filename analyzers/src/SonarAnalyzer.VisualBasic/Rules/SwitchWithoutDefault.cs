/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class SwitchWithoutDefault : SwitchWithoutDefaultBase<SyntaxKind>
    {
        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ImmutableArray<SyntaxKind> kindsOfInterest = ImmutableArray.Create(SyntaxKind.SelectBlock);
        public override ImmutableArray<SyntaxKind> SyntaxKindsOfInterest => kindsOfInterest;

        protected override bool TryGetDiagnostic(SyntaxNode node, out Diagnostic diagnostic)
        {
            diagnostic = null;
            var switchNode = (SelectBlockSyntax)node;
            if (!HasDefaultLabel(switchNode))
            {
                diagnostic = CreateDiagnostic(rule, switchNode.SelectStatement.SelectKeyword.GetLocation(), "Case Else", "Select");
                return true;
            }

            return false;
        }

        private static bool HasDefaultLabel(SelectBlockSyntax node)
        {
            return node.CaseBlocks.Any(SyntaxKind.CaseElseBlock);
        }

        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => VisualBasicGeneratedCodeRecognizer.Instance;
    }
}
