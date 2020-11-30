/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.Common
{
    public abstract class FlagsEnumWithoutInitializerBase : SonarDiagnosticAnalyzer
    {
        protected const int AllowedEmptyMemberCount = 3;

        protected const string DiagnosticId = "S2345";
        protected const string MessageFormat = "Initialize all the members of this 'Flags' enumeration.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
    }

    public abstract class FlagsEnumWithoutInitializerBase<TLanguageKindEnum, TEnumDeclarationSyntax, TEnumMemberDeclarationSyntax> : FlagsEnumWithoutInitializerBase
        where TLanguageKindEnum : struct
        where TEnumDeclarationSyntax : SyntaxNode
        where TEnumMemberDeclarationSyntax : SyntaxNode
    {
        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    var enumDeclaration = (TEnumDeclarationSyntax)c.Node;
                    if (!enumDeclaration.HasFlagsAttribute(c.SemanticModel))
                    {
                        return;
                    }

                    if (!AreAllRequiredMembersInitialized(enumDeclaration))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], GetIdentifier(enumDeclaration).GetLocation()));
                    }
                },
                SyntaxKindsOfInterest.ToArray());
        }

        public abstract ImmutableArray<TLanguageKindEnum> SyntaxKindsOfInterest { get; }

        protected abstract SyntaxToken GetIdentifier(TEnumDeclarationSyntax declaration);

        protected abstract IList<TEnumMemberDeclarationSyntax> GetMembers(TEnumDeclarationSyntax declaration);

        protected abstract bool IsInitialized(TEnumMemberDeclarationSyntax member);

        private bool AreAllRequiredMembersInitialized(TEnumDeclarationSyntax declaration)
        {
            var members = GetMembers(declaration);
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

            var firstNonInitializedIndex = members.IndexOf(firstNonInitialized);
            var firstInitializedIndex = members.IndexOf(firstInitialized);

            if (firstNonInitializedIndex > firstInitializedIndex ||
                firstInitializedIndex >= AllowedEmptyMemberCount)
            {
                // Have first uninitialized member after the first initialized member, or
                // Have too many uninitialized in the beginning
                return false;
            }

            return members
                .Skip(firstInitializedIndex)
                .All(m => IsInitialized(m));
        }
    }
}
