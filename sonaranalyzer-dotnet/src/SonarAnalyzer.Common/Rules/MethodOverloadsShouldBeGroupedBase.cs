/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SonarAnalyzer.Rules
{
    public abstract class MethodOverloadsShouldBeGroupedBase<TMemberDeclarationSyntax> : SonarDiagnosticAnalyzer
        where TMemberDeclarationSyntax : SyntaxNode
    {

        internal const string DiagnosticId = "S4136";
        protected const string MessageFormat = "All '{0}' method overloads should be adjacent.";

        private readonly DiagnosticDescriptor rule;
    
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected MethodOverloadsShouldBeGroupedBase(System.Resources.ResourceManager resources)
        {
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, resources);
        }
        
        protected abstract MemberInfo CreateMemberInfo(SyntaxNodeAnalysisContext c, TMemberDeclarationSyntax member);
        
        protected void CheckMembers(SyntaxNodeAnalysisContext c, IEnumerable<TMemberDeclarationSyntax> members)
        {
            foreach (var misplacedMembers in GetMisplacedOverloads(c, members))
            {
                var firstMember = misplacedMembers.First();
                var secondaryLocations = misplacedMembers.Skip(1).Select(x => new SecondaryLocation(x.NameSyntax.GetLocation(), "Non-adjacent overload"));
                c.ReportDiagnosticWhenActive(
                    Diagnostic.Create(
                        descriptor: rule,
                        location: firstMember.NameSyntax.GetLocation(),
                        additionalLocations: secondaryLocations.ToAdditionalLocations(),
                        properties: secondaryLocations.ToProperties(),
                        messageArgs: firstMember.NameSyntax.ValueText));
            }
        }

        protected List<MemberInfo>[] GetMisplacedOverloads(SyntaxNodeAnalysisContext c, IEnumerable<TMemberDeclarationSyntax> members)
        {
            var misplacedOverloads = new Dictionary<MemberInfo, List<MemberInfo>>();
            MemberInfo previous = null;
            foreach (var member in members)
            {
                if (CreateMemberInfo(c, member) is MemberInfo current)
                {
                    if (misplacedOverloads.TryGetValue(current, out var values))
                    {
                        if (!current.Equals(previous))
                        {
                            values.Add(current);
                        }
                    }
                    else
                    {
                        misplacedOverloads.Add(current, new List<MemberInfo> { current });
                    }
                    previous = current;
                }
                else
                {
                    previous = null;
                }
            }
            return misplacedOverloads.Values.Where(x => x.Count > 1).ToArray();
        }

        protected class MemberInfo
        {

            public readonly TMemberDeclarationSyntax Member;
            public readonly string Accessibility;
            public readonly SyntaxToken NameSyntax;
            public readonly bool IsStatic;
            public readonly bool IsAbstract;

            private readonly bool isCaseSensitive;

            public MemberInfo(SyntaxNodeAnalysisContext context, TMemberDeclarationSyntax member, SyntaxToken nameSyntax, bool isStatic, bool isAbstract, bool isCaseSensitive)
            {
                Member = member;
                Accessibility = context.SemanticModel.GetDeclaredSymbol(member)?.DeclaredAccessibility.ToString();
                NameSyntax = nameSyntax;
                IsStatic = isStatic;
                IsAbstract = isAbstract;
                this.isCaseSensitive = isCaseSensitive;                
            }
            
            public override bool Equals(object obj)
            {
                // Groups that should be together are defined by accessibility, abstract, static and member name #4136
                return obj is MemberInfo info
                    && Accessibility == info.Accessibility
                    && NameSyntax.ValueText.Equals(info.NameSyntax.ValueText, isCaseSensitive && info.isCaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase)
                    && IsStatic == info.IsStatic
                    && IsAbstract == info.IsAbstract;
            }

            public override int GetHashCode()
            {
                return NameSyntax.ValueText.ToLowerInvariant().GetHashCode();
            }

        }

    }
}
