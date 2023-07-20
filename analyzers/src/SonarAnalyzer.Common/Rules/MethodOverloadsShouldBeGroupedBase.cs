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
    public abstract class MethodOverloadsShouldBeGroupedBase<TSyntaxKind, TMemberDeclarationSyntax> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
        where TMemberDeclarationSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S4136";

        protected abstract TSyntaxKind[] SyntaxKinds { get; }

        protected abstract IEnumerable<TMemberDeclarationSyntax> GetMemberDeclarations(SyntaxNode node);
        protected abstract MemberInfo CreateMemberInfo(SonarSyntaxNodeReportingContext c, TMemberDeclarationSyntax member);

        protected override string MessageFormat => "All '{0}' method overloads should be adjacent.";

        protected MethodOverloadsShouldBeGroupedBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
            {
                if (c.IsRedundantPositionalRecordContext())
                {
                    return;
                }

                foreach (var misplacedOverload in GetMisplacedOverloads(c, GetMemberDeclarations(c.Node)))
                {
                    var firstName = misplacedOverload.First().NameSyntax;
                    var secondaryLocations = misplacedOverload.Skip(1).Select(x => new SecondaryLocation(x.NameSyntax.GetLocation(), "Non-adjacent overload")).ToList();
                    c.ReportIssue(CreateDiagnostic(
                        descriptor: Rule,
                        location: firstName.GetLocation(),
                        additionalLocations: secondaryLocations.ToAdditionalLocations(),
                        properties: secondaryLocations.ToProperties(),
                        messageArgs: firstName.ValueText));
                }
            },
            SyntaxKinds);

        protected List<MemberInfo>[] GetMisplacedOverloads(SonarSyntaxNodeReportingContext c, IEnumerable<TMemberDeclarationSyntax> members)
        {
            var misplacedOverloads = new Dictionary<MemberInfo, List<MemberInfo>>();
            var membersGroupedByInterface = MembersGroupedByInterface(c, members);
            MemberInfo previous = null;
            foreach (var member in members)
            {
                if (CreateMemberInfo(c, member) is MemberInfo current)
                {
                    if (misplacedOverloads.TryGetValue(current, out var values))
                    {
                        if (!current.NameEquals(previous) && IsMisplacedCandidate(member, values))
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

            bool IsMisplacedCandidate(TMemberDeclarationSyntax member, List<MemberInfo> others)
            {
                if (membersGroupedByInterface.TryGetValue(member, out var interfaces))
                {
                    return interfaces.Length == 1 && others.Any(other => FindInterfaces(c.SemanticModel, other.Member).Contains(interfaces.Single()));
                }
                return true; // Not member of an interface => process
            }
        }

        /// <summary>
        /// Function returns members that are considered to be grouped with another member of the same interface (adjacent members).
        /// These members are allowed to be grouped by interface and not forced to be grouped by member name.
        /// Another overload (not related by interface) can be placed somewhere else.
        ///
        /// Returned ImmutableArray of interfaces for each member is used to determine whether overloads of the same interface should be grouped by name.
        /// If all methods of the class implement single interface, we want the overloads to be placed together within interface group.
        /// </summary>
        private static Dictionary<TMemberDeclarationSyntax, ImmutableArray<INamedTypeSymbol>> MembersGroupedByInterface(SonarSyntaxNodeReportingContext c, IEnumerable<TMemberDeclarationSyntax> members)
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

        private static ImmutableArray<INamedTypeSymbol> FindInterfaces(SemanticModel semanticModel, TMemberDeclarationSyntax member)
        {
            var ret = new HashSet<INamedTypeSymbol>();
            var symbol = semanticModel.GetDeclaredSymbol(member);
            if (symbol != null)
            {
                ret.AddRange(ExplicitInterfaceImplementations(symbol).Select(x => x.ContainingType));
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

        private static IEnumerable<ISymbol> ExplicitInterfaceImplementations(ISymbol symbol) =>
            symbol switch
            {
                IEventSymbol e => e.ExplicitInterfaceImplementations,
                IMethodSymbol m => m.ExplicitInterfaceImplementations,
                IPropertySymbol p => p.ExplicitInterfaceImplementations,
                _ => Enumerable.Empty<ISymbol>()
            };

        protected class MemberInfo
        {
            private readonly string accessibility;
            private readonly bool isStatic;
            private readonly bool isAbstract;
            private readonly bool isCaseSensitive;

            public TMemberDeclarationSyntax Member { get; }
            public SyntaxToken NameSyntax { get; }

            public MemberInfo(SonarSyntaxNodeReportingContext context, TMemberDeclarationSyntax member, SyntaxToken nameSyntax, bool isStatic, bool isAbstract, bool isCaseSensitive)
            {
                Member = member;
                accessibility = context.SemanticModel.GetDeclaredSymbol(member)?.DeclaredAccessibility.ToString();
                NameSyntax = nameSyntax;
                this.isStatic = isStatic;
                this.isAbstract = isAbstract;
                this.isCaseSensitive = isCaseSensitive;
            }

            public bool NameEquals(MemberInfo other) =>
                NameSyntax.ValueText.Equals(other?.NameSyntax.ValueText, isCaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase);

            public override bool Equals(object obj) =>
                // Groups that should be together are defined by accessibility, abstract, static and member name #4136
                obj is MemberInfo other
                    && NameEquals(other)
                    && accessibility == other.accessibility
                    && isStatic == other.isStatic
                    && isAbstract == other.isAbstract;

            public override int GetHashCode() =>
                NameSyntax.ValueText.ToUpperInvariant().GetHashCode();
        }
    }
}
