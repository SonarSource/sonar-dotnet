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
    public abstract class FlagsEnumWithoutInitializerBase<TSyntaxKind, TEnumMemberDeclarationSyntax> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
        where TEnumMemberDeclarationSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S2345";
        private const int AllowedEmptyMemberCount = 3;

        protected abstract bool IsInitialized(TEnumMemberDeclarationSyntax member);

        protected override string MessageFormat => "Initialize all the members of this 'Flags' enumeration.";

        protected FlagsEnumWithoutInitializerBase() : base(DiagnosticId) { }

        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
                {
                    if (c.Node.HasFlagsAttribute(c.SemanticModel) && !AreAllRequiredMembersInitialized(c.Node) && Language.Syntax.NodeIdentifier(c.Node) is { } identifier)
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, identifier.GetLocation()));
                    }
                },
                Language.SyntaxKind.EnumDeclaration);

        private bool AreAllRequiredMembersInitialized(SyntaxNode declaration)
        {
            var members = Language.Syntax.EnumMembers(declaration).Cast<TEnumMemberDeclarationSyntax>().ToList();
            var firstNonInitialized = members.FirstOrDefault(m => !IsInitialized(m));
            if (firstNonInitialized == null)
            {
                // All members initialized
                return true;
            }

            var firstInitialized = members.FirstOrDefault(m => IsInitialized(m));
            if (firstInitialized == null)
            {
                // No members initialized
                return members.Count <= AllowedEmptyMemberCount;
            }

            var firstInitializedIndex = members.IndexOf(firstInitialized);
            if (firstInitializedIndex >= AllowedEmptyMemberCount || members.IndexOf(firstNonInitialized) > firstInitializedIndex)
            {
                // Have first uninitialized member after the first initialized member, or
                // Have too many uninitialized in the beginning
                return false;
            }
            return members.Skip(firstInitializedIndex).All(m => IsInitialized(m));
        }
    }
}
