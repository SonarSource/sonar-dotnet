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

namespace SonarAnalyzer.Rules
{
    public abstract class FlagsEnumZeroMemberBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S2346";
        protected override string MessageFormat => "Rename '{0}' to 'None'.";

        protected FlagsEnumZeroMemberBase() : base(DiagnosticId) { }

        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
                {
                    if (c.Node.HasFlagsAttribute(c.SemanticModel)
                        && ZeroMember(c.Node, c.SemanticModel) is { } zeroMember
                        && Language.Syntax.NodeIdentifier(zeroMember) is { } identifier
                        && identifier.ValueText != "None")
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, zeroMember.GetLocation(), identifier.ValueText));
                    }
                },
                Language.SyntaxKind.EnumDeclaration);

        private SyntaxNode ZeroMember(SyntaxNode node, SemanticModel semanticModel)
        {
            foreach (var item in Language.Syntax.EnumMembers(node))
            {
                if (semanticModel.GetDeclaredSymbol(item) is IFieldSymbol symbol && symbol.ConstantValue is { } constValue)
                {
                    try
                    {
                        if (Convert.ToInt64(constValue) == 0)
                        {
                            return item;
                        }
                    }
                    catch (OverflowException)
                    {
                        return null;
                    }
                }
            }
            return null;
        }
    }
}
