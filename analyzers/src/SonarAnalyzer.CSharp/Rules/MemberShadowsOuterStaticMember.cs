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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MemberShadowsOuterStaticMember : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3218";
        private const string MessageFormat = "Rename this {0} to not shadow the outer class' member with the same name.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSymbolAction(c =>
                {
                    var innerClassSymbol = (INamedTypeSymbol)c.Symbol;
                    var containerClassSymbol = innerClassSymbol.ContainingType;

                    if (!IsValidType(innerClassSymbol)
                        || !IsValidType(containerClassSymbol)
                        || (innerClassSymbol.GetMembers().Where(x => !x.IsImplicitlyDeclared && !IsStaticAndVirtualOrAbstract(x)).ToList() is var members
                            && !members.Any()))
                    {
                        return;
                    }

                    var selfAndOuterNamedTypes = SelfAndOuterNamedTypes(containerClassSymbol);

                    foreach (var innerMember in members)
                    {
                        var outerMembersOfSameName = selfAndOuterNamedTypes.SelectMany(x => x.GetMembers(innerMember.Name)).ToList();

                        switch (innerMember)
                        {
                            case IPropertySymbol:
                            case IFieldSymbol:
                            case IEventSymbol:
                            case IMethodSymbol { MethodKind: MethodKind.DeclareMethod or MethodKind.Ordinary }:
                                CheckMember(c, outerMembersOfSameName, innerMember);
                                break;
                            case INamedTypeSymbol namedType:
                                CheckNamedType(c, outerMembersOfSameName, namedType);
                                break;
                        }
                    }
                },
                SymbolKind.NamedType);

        private static void CheckNamedType(SonarSymbolReportingContext context, IReadOnlyList<ISymbol> outerMembersOfSameName, INamedTypeSymbol namedType)
        {
            if (outerMembersOfSameName.Any(x => x is INamedTypeSymbol { TypeKind: TypeKind.Class or TypeKind.Struct or TypeKind.Delegate or TypeKind.Enum or TypeKind.Interface }))
            {
                foreach (var identifier in namedType.DeclaringReferenceIdentifiers())
                {
                    context.ReportIssue(CreateDiagnostic(Rule, identifier.GetLocation(), namedType.GetClassification()));
                }
            }
        }

        private static void CheckMember(SonarSymbolReportingContext context, IReadOnlyList<ISymbol> outerMembersOfSameName, ISymbol member)
        {
            if (outerMembersOfSameName.Any(x => (x.IsStatic && !x.IsAbstract && !x.IsVirtual) || x is IFieldSymbol { IsConst: true })
                && member.FirstDeclaringReferenceIdentifier() is { } identifier
                && identifier.GetLocation() is { Kind: LocationKind.SourceFile } location)
            {
                context.ReportIssue(CreateDiagnostic(Rule, location, member.GetClassification()));
            }
        }

        private static IReadOnlyList<INamedTypeSymbol> SelfAndOuterNamedTypes(INamedTypeSymbol symbol)
        {
            var namedTypes = new List<INamedTypeSymbol>();
            var current = symbol;
            while (current is not null)
            {
                namedTypes.Add(current);
                current = current.ContainingType;
            }
            return namedTypes;
        }

        private static bool IsValidType(INamedTypeSymbol symbol)
            => symbol.IsClassOrStruct() || symbol.IsInterface();

        private static bool IsStaticAndVirtualOrAbstract(ISymbol symbol)
            => symbol.IsStatic && (symbol.IsVirtual || symbol.IsAbstract);
    }
}
