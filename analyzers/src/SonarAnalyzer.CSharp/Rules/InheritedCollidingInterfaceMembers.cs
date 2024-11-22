/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class InheritedCollidingInterfaceMembers : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3444";
        private const string MessageFormat = "Rename or add member{1} {0} to this interface to resolve ambiguities.";
        private const int MaxMemberDisplayCount = 2;
        private const int MinBaseListTypes = 2;

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var interfaceDeclaration = (InterfaceDeclarationSyntax)c.Node;
                    if (interfaceDeclaration.BaseList == null || interfaceDeclaration.BaseList.Types.Count < MinBaseListTypes)
                    {
                        return;
                    }

                    var interfaceSymbol = c.SemanticModel.GetDeclaredSymbol(interfaceDeclaration);
                    if (interfaceSymbol == null)
                    {
                        return;
                    }

                    var collidingMembers = GetCollidingMembers(interfaceSymbol).Take(MaxMemberDisplayCount + 1).ToList();
                    if (collidingMembers.Any())
                    {
                        var membersText = GetIssueMessageText(collidingMembers, c.SemanticModel, interfaceDeclaration.SpanStart);
                        var pluralize = collidingMembers.Count > 1 ? "s" : string.Empty;
                        var secondaryLocations = collidingMembers.SelectMany(x => x.Locations).Where(x => x.IsInSource).ToSecondary();
                        c.ReportIssue(Rule, interfaceDeclaration.Identifier, secondaryLocations, membersText, pluralize);
                    }
                },
                SyntaxKind.InterfaceDeclaration);

        private static IEnumerable<IMethodSymbol> GetCollidingMembers(ITypeSymbol interfaceSymbol)
        {
            var implementedInterfaces = interfaceSymbol.Interfaces;

            var membersFromDerivedInterface = interfaceSymbol.GetMembers().OfType<IMethodSymbol>().ToList();

            for (var i = 0; i < implementedInterfaces.Length; i++)
            {
                var notRedefinedMembersFromInterface = implementedInterfaces[i]
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(method => method.DeclaredAccessibility != Accessibility.Private
                                     && !membersFromDerivedInterface.Any(redefinedMember => AreCollidingMethods(method, redefinedMember)));

                var collidingMembers = notRedefinedMembersFromInterface.SelectMany(member => GetCollidingMembersForMember(member, implementedInterfaces.Skip(i + 1)));

                foreach (var collidingMember in collidingMembers)
                {
                    yield return collidingMember;
                }

                IEnumerable<IMethodSymbol> GetCollidingMembersForMember(IMethodSymbol member, IEnumerable<INamedTypeSymbol> interfaces) =>
                    interfaces.SelectMany(x => GetCollidingMembersForMemberAndInterface(member, x));

                IEnumerable<IMethodSymbol> GetCollidingMembersForMemberAndInterface(IMethodSymbol member, INamedTypeSymbol interfaceToCheck) =>
                    interfaceToCheck
                        .GetMembers(member.Name)
                        .OfType<IMethodSymbol>()
                        .Where(IsNotEventRemoveAccessor)
                        .Where(x => AreCollidingMethods(member, x));
            }
        }

        private static bool IsNotEventRemoveAccessor(IMethodSymbol methodSymbol) =>
            // we only want to report on events once, so we are not collecting the "remove" accessors,
            // and handle the "add" accessor reporting separately in <see cref="GetMemberDisplayName"/>
            methodSymbol.MethodKind != MethodKind.EventRemove;

        private static string GetIssueMessageText(IEnumerable<IMethodSymbol> collidingMembers, SemanticModel semanticModel, int spanStart)
        {
            var names = collidingMembers.Take(MaxMemberDisplayCount)
                                        .Select(member => GetMemberDisplayName(member, spanStart, semanticModel))
                                        .Distinct()
                                        .ToList();

            return names.Count switch
            {
                1 => names[0],
                2 => $"{names[0]} and {names[1]}",
                _ => names.JoinStr(", ") + ", ..."
            };
        }

        private static readonly ISet<SymbolDisplayPartKind> PartKindsToStartWith = new HashSet<SymbolDisplayPartKind>
        {
            SymbolDisplayPartKind.MethodName,
            SymbolDisplayPartKind.PropertyName,
            SymbolDisplayPartKind.EventName
        };

        private static string GetMemberDisplayName(IMethodSymbol method, int spanStart, SemanticModel semanticModel)
        {
            if (method.AssociatedSymbol is IPropertySymbol { IsIndexer: true } property)
            {
                var text = property.ToMinimalDisplayString(semanticModel, spanStart, SymbolDisplayFormat.CSharpShortErrorMessageFormat);
                return $"'{text}'";
            }

            var parts = method.ToMinimalDisplayParts(semanticModel, spanStart, SymbolDisplayFormat.CSharpShortErrorMessageFormat)
                .SkipWhile(part => !PartKindsToStartWith.Contains(part.Kind))
                .ToList();

            if (method.MethodKind == MethodKind.EventAdd)
            {
                parts = parts.Take(parts.Count - 2).ToList();
            }

            return $"'{string.Join(string.Empty, parts)}'";
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
    }
}
