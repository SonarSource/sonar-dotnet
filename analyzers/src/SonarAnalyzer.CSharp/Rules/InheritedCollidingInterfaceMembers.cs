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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InheritedCollidingInterfaceMembers : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S3444";
    private const string MessageFormat = "Rename or add member{1} {0} to this interface to resolve ambiguities.";
    private const string SecondaryMessageFormat = "This member collides with '{0}'";
    private const int MaxMemberDisplayCount = 2;
    private const int MinBaseListTypes = 2;

    private static readonly ISet<SymbolDisplayPartKind> PartKindsToStartWith = new HashSet<SymbolDisplayPartKind>
    {
        SymbolDisplayPartKind.MethodName,
        SymbolDisplayPartKind.PropertyName,
        SymbolDisplayPartKind.EventName
    };

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            c =>
            {
                var interfaceDeclaration = (InterfaceDeclarationSyntax)c.Node;
                if (interfaceDeclaration.BaseList is null || interfaceDeclaration.BaseList.Types.Count < MinBaseListTypes)
                {
                    return;
                }

                var interfaceSymbol = c.Model.GetDeclaredSymbol(interfaceDeclaration);
                if (interfaceSymbol is null)
                {
                    return;
                }

                var collidingMembers = GetCollidingMembers(interfaceSymbol).Take(MaxMemberDisplayCount + 1).ToList();
                if (collidingMembers.Any())
                {
                    var membersText = GetIssueMessageText(collidingMembers, c.Model, interfaceDeclaration.SpanStart);
                    var pluralize = collidingMembers.Count > 1 ? "s" : string.Empty;
                    c.ReportIssue(Rule, interfaceDeclaration.Identifier, SecondaryLocations(collidingMembers, c.Model), membersText, pluralize);
                }
            },
            SyntaxKind.InterfaceDeclaration);

    private static IEnumerable<SecondaryLocation> SecondaryLocations(List<CollidingMember> collidingMembers, SemanticModel model)
    {
        return collidingMembers
            .SelectMany(x => x.Member.Locations.Select(l => new { Location = l, x.CollideWith, CollideWithSymbolName = CollideWithSymbolName(x) }))
            .Where(x => x.Location.IsInSource)
            .Select(x => x.Location.ToSecondary(x.CollideWithSymbolName is { Length: > 0 } ? SecondaryMessageFormat : null, x.CollideWithSymbolName));

        string CollideWithSymbolName(CollidingMember collidingMember)
        {
            return collidingMember.CollideWith.Locations.FirstOrDefault() is { } location
                && location.SourceTree.SemanticModelOrDefault(model) is { } semanticModel
                ? GetMemberDisplayName(collidingMember.CollideWith, location.SourceSpan.Start, semanticModel, [SymbolDisplayPartKind.ClassName, SymbolDisplayPartKind.InterfaceName])
                : string.Empty;
        }
    }

    private static IEnumerable<CollidingMember> GetCollidingMembers(ITypeSymbol interfaceSymbol)
    {
        var implementedInterfaces = interfaceSymbol.Interfaces;

        var membersFromDerivedInterface = interfaceSymbol.GetMembers().OfType<IMethodSymbol>().ToList();

        for (var i = 0; i < implementedInterfaces.Length; i++)
        {
            var notRedefinedMembersFromInterface = implementedInterfaces[i]
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Where(x => x.DeclaredAccessibility != Accessibility.Private
                    && !membersFromDerivedInterface.Any(redefinedMember => AreCollidingMethods(x, redefinedMember)));

            var collidingMembers = notRedefinedMembersFromInterface.SelectMany(x => GetCollidingMembersForMember(x, implementedInterfaces.Skip(i + 1)));

            foreach (var collidingMember in collidingMembers)
            {
                yield return collidingMember;
            }

            IEnumerable<CollidingMember> GetCollidingMembersForMember(IMethodSymbol member, IEnumerable<INamedTypeSymbol> interfaces) =>
                interfaces.SelectMany(x => GetCollidingMembersForMemberAndInterface(member, x));

            IEnumerable<CollidingMember> GetCollidingMembersForMemberAndInterface(IMethodSymbol member, INamedTypeSymbol interfaceToCheck) =>
                interfaceToCheck
                    .GetMembers(member.Name)
                    .OfType<IMethodSymbol>()
                    .Where(IsNotEventRemoveAccessor)
                    .Where(x => AreCollidingMethods(member, x))
                    .Select(x => new CollidingMember(x, member));
        }
    }

    private static bool IsNotEventRemoveAccessor(IMethodSymbol methodSymbol) =>
        // we only want to report on events once, so we are not collecting the "remove" accessors,
        // and handle the "add" accessor reporting separately in <see cref="GetMemberDisplayName"/>
        methodSymbol.MethodKind != MethodKind.EventRemove;

    private static string GetIssueMessageText(IEnumerable<CollidingMember> collidingMembers, SemanticModel model, int spanStart)
    {
        var names = collidingMembers.Take(MaxMemberDisplayCount)
            .Select(x => $"'{GetMemberDisplayName(x.Member, spanStart, model)}'")
            .Distinct()
            .ToList();

        return names.Count switch
        {
            1 => names[0],
            2 => $"{names[0]} and {names[1]}",
            _ => names.JoinStr(", ") + ", ..."
        };
    }

    private static string GetMemberDisplayName(IMethodSymbol method, int spanStart, SemanticModel model, HashSet<SymbolDisplayPartKind> additionalPartKindsToStartWith = null)
    {
        if (method.AssociatedSymbol is IPropertySymbol { IsIndexer: true } property)
        {
            var text = property.ToMinimalDisplayString(model, spanStart, SymbolDisplayFormat.CSharpShortErrorMessageFormat);
            return $"{text}";
        }

        var parts = method.ToMinimalDisplayParts(model, spanStart, SymbolDisplayFormat.CSharpShortErrorMessageFormat)
            .SkipWhile(x => (additionalPartKindsToStartWith is null || !additionalPartKindsToStartWith.Contains(x.Kind)) && !PartKindsToStartWith.Contains(x.Kind))
            .ToList();

        if (method.MethodKind == MethodKind.EventAdd)
        {
            parts = parts.Take(parts.Count - 2).ToList();
        }

        return $"{string.Join(string.Empty, parts)}";
    }

    private static bool AreCollidingMethods(IMethodSymbol methodSymbol1, IMethodSymbol methodSymbol2)
    {
        if (methodSymbol1.Name != methodSymbol2.Name
            || methodSymbol1.MethodKind != methodSymbol2.MethodKind
            || methodSymbol1.Parameters.Length != methodSymbol2.Parameters.Length
            || methodSymbol1.Arity != methodSymbol2.Arity)
        {
            return false;
        }

        for (var i = 0; i < methodSymbol1.Parameters.Length; i++)
        {
            var param1 = methodSymbol1.Parameters[i];
            var param2 = methodSymbol2.Parameters[i];

            if (param1.RefKind != param2.RefKind
                || !Equals(param1.Type, param2.Type))
            {
                return false;
            }
        }

        return true;
    }

    private sealed record CollidingMember(IMethodSymbol Member, IMethodSymbol CollideWith);
}
