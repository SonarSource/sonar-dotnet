/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Rules;

public abstract class MethodOverloadsShouldBeGroupedBase<TSyntaxKind, TMemberDeclarationSyntax> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TMemberDeclarationSyntax : SyntaxNode
{
    protected const string DiagnosticId = "S4136";

    protected abstract TSyntaxKind[] SyntaxKinds { get; }

    protected abstract IEnumerable<TMemberDeclarationSyntax> MemberDeclarations(SyntaxNode node);
    protected abstract MemberInfo CreateMemberInfo(SonarSyntaxNodeReportingContext c, TMemberDeclarationSyntax member);

    protected override string MessageFormat => "All '{0}' method overloads should be adjacent.";

    protected MethodOverloadsShouldBeGroupedBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer, c =>
            {
                if (c.IsRedundantPositionalRecordContext())
                {
                    return;
                }

                foreach (var misplacedOverload in MisplacedOverloads(c, MemberDeclarations(c.Node)))
                {
                    var firstName = misplacedOverload[0].NameSyntax;
                    var secondaryLocations = misplacedOverload.Skip(1).Select(x => new SecondaryLocation(x.NameSyntax.GetLocation(), "Non-adjacent overload")).ToList();
                    c.ReportIssue(Rule, firstName, secondaryLocations, firstName.ValueText);
                }
            },
            SyntaxKinds);

    protected List<MemberInfo>[] MisplacedOverloads(SonarSyntaxNodeReportingContext c, IEnumerable<TMemberDeclarationSyntax> members)
    {
        var misplacedOverloads = new Dictionary<MemberInfo, List<MemberInfo>>();
        var membersGroupedByInterface = MembersGroupedByInterface(c, members);
        MemberInfo previous = null;
        foreach (var member in members)
        {
            if (CreateMemberInfo(c, member) is not MemberInfo current)
            {
                previous = null;
                continue;
            }

            if (misplacedOverloads.TryGetValue(current, out var values))
            {
                if (!current.NameEquals(previous) && IsMisplacedCandidate(member, values))
                {
                    values.Add(current);
                }
            }
            else
            {
                misplacedOverloads.Add(current, [current]);
            }
            previous = current;
        }
        return misplacedOverloads.Values.Where(x => x.Count > 1).ToArray();

        bool IsMisplacedCandidate(TMemberDeclarationSyntax member, List<MemberInfo> others) =>
            !membersGroupedByInterface.TryGetValue(member, out var interfaces)
            || (interfaces.Length == 1 && others.Any(x => FindInterfaces(c.Model, x.Member).Contains(interfaces.Single())));
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
        var previousInterfaces = ImmutableArray<INamedTypeSymbol>.Empty;
        TMemberDeclarationSyntax previous = null;
        foreach (var member in members)
        {
            var currentInterfaces = FindInterfaces(c.Model, member);
            if (currentInterfaces.Intersect(previousInterfaces).Any())
            {
                ret.Add(member, currentInterfaces);
                if (previous is not null && !ret.ContainsKey(previous))
                {
                    ret.Add(previous, previousInterfaces);
                }
            }
            previousInterfaces = currentInterfaces;
            previous = member;
        }
        return ret;
    }

    private static ImmutableArray<INamedTypeSymbol> FindInterfaces(SemanticModel model, TMemberDeclarationSyntax member)
    {
        var ret = new HashSet<INamedTypeSymbol>();
        if (model.GetDeclaredSymbol(member) is { } symbol)
        {
            ret.AddRange(ExplicitInterfaceImplementations(symbol).Select(x => x.ContainingType));
            ret.AddRange(symbol.ContainingType.AllInterfaces.Where(IsImplicityImplemented));
        }
        return ret.ToImmutableArray();

        bool IsImplicityImplemented(INamedTypeSymbol @interface) =>
            @interface.GetMembers().Any(x => symbol.ContainingType.FindImplementationForInterfaceMember(x)?.Equals(symbol) == true);
    }

    private static IEnumerable<ISymbol> ExplicitInterfaceImplementations(ISymbol symbol) =>
        symbol switch
        {
            IEventSymbol @event => @event.ExplicitInterfaceImplementations,
            IMethodSymbol method => method.ExplicitInterfaceImplementations,
            IPropertySymbol property => property.ExplicitInterfaceImplementations,
            _ => []
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
            accessibility = context.Model.GetDeclaredSymbol(member)?.DeclaredAccessibility.ToString();
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
