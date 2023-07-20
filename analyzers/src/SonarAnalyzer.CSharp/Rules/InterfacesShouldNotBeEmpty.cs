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
    public sealed class InterfacesShouldNotBeEmpty : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4023";
        private const string MessageFormat = "Remove this interface or add members to it.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var interfaceDeclaration = (InterfaceDeclarationSyntax)c.Node;
                    if (interfaceDeclaration.Identifier.IsMissing
                        || interfaceDeclaration.Members.Count > 0)
                    {
                        return;
                    }

                    var interfaceSymbol = c.SemanticModel.GetDeclaredSymbol(interfaceDeclaration);
                    if (interfaceSymbol is { DeclaredAccessibility: Accessibility.Public }
                        && !IsAggregatingOtherInterfaces(interfaceSymbol)
                        && !IsSpecializedGeneric(interfaceSymbol)
                        && !HasEnhancingAttribute(interfaceSymbol))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, interfaceDeclaration.Identifier.GetLocation()));
                    }
                },
                SyntaxKind.InterfaceDeclaration);

        private static bool IsAggregatingOtherInterfaces(ITypeSymbol interfaceSymbol) =>
            interfaceSymbol.Interfaces.Length > 1;

        private static bool IsSpecializedGeneric(INamedTypeSymbol interfaceSymbol) =>
            IsImplementingInterface(interfaceSymbol) && (IsBoundGeneric(interfaceSymbol) || IsConstraintGeneric(interfaceSymbol));

        private static bool IsConstraintGeneric(INamedTypeSymbol interfaceSymbol) =>
            interfaceSymbol.TypeParameters.Any(x => x.HasAnyConstraint());

        private static bool IsBoundGeneric(INamedTypeSymbol interfaceSymbol) =>
            interfaceSymbol.Interfaces.Any(i => i.TypeArguments.Any(a => a is INamedTypeSymbol { IsUnboundGenericType: false }));

        private static bool HasEnhancingAttribute(INamedTypeSymbol interfaceSymbol) =>
            IsImplementingInterface(interfaceSymbol) // Attributes on interfaces without base interfaces do not make sense.
                                                     // Implementing types do not get the attribute applied even with AttributeUsageAttribute.Inherited = true
            && interfaceSymbol.GetAttributes().Any();

        private static bool IsImplementingInterface(INamedTypeSymbol interfaceSymbol) =>
            !interfaceSymbol.Interfaces.IsEmpty;
    }
}
