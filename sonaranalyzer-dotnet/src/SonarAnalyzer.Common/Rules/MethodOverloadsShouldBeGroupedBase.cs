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
            foreach (var misplacedOverload in GetMisplacedOverloads(c, members))
            {
                var firstName = misplacedOverload.First().NameSyntax;
                var secondaryLocations = misplacedOverload.Skip(1).Select(x => new SecondaryLocation(x.NameSyntax.GetLocation(), "Non-adjacent overload"));
                c.ReportDiagnosticWhenActive(
                    Diagnostic.Create(
                        descriptor: rule,
                        location: firstName.GetLocation(),
                        additionalLocations: secondaryLocations.ToAdditionalLocations(),
                        properties: secondaryLocations.ToProperties(),
                        messageArgs: firstName.ValueText));
            }
        }

        protected List<MemberInfo>[] GetMisplacedOverloads(SyntaxNodeAnalysisContext c, IEnumerable<TMemberDeclarationSyntax> members)
        {
            var misplacedOverloads = new Dictionary<MemberInfo, List<MemberInfo>>();
            var groupedByInterface = GroupedByInterface(c, members);
            MemberInfo previous = null;
            foreach (var member in members)
            {
                if (CreateMemberInfo(c, member) is MemberInfo current)
                {
                    if (misplacedOverloads.TryGetValue(current, out var values))
                    {
                        if (!current.NameEquals(previous) && ProcessByInterface(member, values))
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

            bool ProcessByInterface(TMemberDeclarationSyntax member, List<MemberInfo> others)
            {
                if(groupedByInterface.TryGetValue(member, out var interfaces))
                {
                    return others.Any(other => FindInterfaces(c.SemanticModel, other.Member).Intersect(interfaces).Any());
                }
                return true; // Not member of an interface => process
            }
        }

        private Dictionary<TMemberDeclarationSyntax, ImmutableArray<INamedTypeSymbol>> GroupedByInterface(SyntaxNodeAnalysisContext c, IEnumerable<TMemberDeclarationSyntax> members)
        {
            var ret = new Dictionary<TMemberDeclarationSyntax, ImmutableArray<INamedTypeSymbol>>();
            ImmutableArray<INamedTypeSymbol> currentInterfaces, previousInterfaces = ImmutableArray<INamedTypeSymbol>.Empty;
            TMemberDeclarationSyntax previous = null;
            foreach (var member in members)
            {
                currentInterfaces = FindInterfaces(c.SemanticModel, member);
                if (currentInterfaces.Intersect(previousInterfaces).Any())
                {
                    ret.Add(member, currentInterfaces);
                    if (previous != null && !ret.ContainsKey(previous))
                    {
                        ret.Add(previous, previousInterfaces);
                    }
                }
                previousInterfaces = currentInterfaces;
                previous = member;
            }
            return ret;
        }

        private ImmutableArray<INamedTypeSymbol> FindInterfaces(SemanticModel semanticModel, TMemberDeclarationSyntax member)
        {
            var ret = new HashSet<INamedTypeSymbol>();
            var symbol = semanticModel.GetDeclaredSymbol(member);
            if (symbol != null)
            {
                ret.AddRange(ExplicitInterfaces(symbol).Select(x => x.ContainingType));
                foreach (var @interface in symbol.ContainingType.AllInterfaces)
                {
                    if (@interface.GetMembers().Any(x => symbol.ContainingType.FindImplementationForInterfaceMember(x) == symbol))
                    {
                        ret.Add(@interface);
                    }
                }
            }
            return ret.ToImmutableArray();
        }

        private IEnumerable<ISymbol> ExplicitInterfaces(ISymbol symbol) =>
            symbol switch
            {
                IEventSymbol e => e.ExplicitInterfaceImplementations,
                IMethodSymbol m => m.ExplicitInterfaceImplementations,
                IPropertySymbol p => p.ExplicitInterfaceImplementations,
                _ => Enumerable.Empty<ISymbol>()
            };

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

            public bool NameEquals(MemberInfo other)
            {
                return NameSyntax.ValueText.Equals(other?.NameSyntax.ValueText, isCaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase);
            }

            public override bool Equals(object obj)
            {
                // Groups that should be together are defined by accessibility, abstract, static and member name #4136
                return obj is MemberInfo other
                    && NameEquals(other)
                    && Accessibility == other.Accessibility
                    && IsStatic == other.IsStatic
                    && IsAbstract == other.IsAbstract;
            }

            public override int GetHashCode()
            {
                return NameSyntax.ValueText.ToLowerInvariant().GetHashCode();
            }

        }

    }
}
