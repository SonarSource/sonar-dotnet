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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class CollectionPropertiesShouldBeReadOnly : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4004";
        private const string MessageFormat = "Make the '{0}' property read-only by removing the property setter or making it private.";

        private static readonly ImmutableArray<KnownType> collectionTypes =
            ImmutableArray.Create(
                KnownType.System_Collections_Generic_ICollection_T,
                KnownType.System_Collections_ICollection
            );

        private static readonly ImmutableArray<KnownType> ignoredCollectionTypes =
            ImmutableArray.Create(
                KnownType.System_Array,
                KnownType.System_Security_PermissionSet
            );

        private static readonly ISet<Accessibility> privateOrInternalAccessibility = new HashSet<Accessibility>
        {
            Accessibility.Private,
            Accessibility.Internal,
        };

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var propertyDeclaration = (PropertyDeclarationSyntax)c.Node;
                    var propertySymbol = c.SemanticModel.GetDeclaredSymbol(propertyDeclaration);

                    if (propertyDeclaration.AccessorList != null &&
                        propertySymbol != null &&
                        HasPublicSetter(propertySymbol) &&
                        IsObservedCollectionType(propertySymbol))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, propertyDeclaration.Identifier.GetLocation(),
                            propertySymbol.Name));
                    }
                },
                SyntaxKind.PropertyDeclaration);
        }

        private static bool IsObservedCollectionType(IPropertySymbol propertySymbol) =>
            !propertySymbol.IsOverride &&
            !propertySymbol.HasAttribute(KnownType.System_Runtime_Serialization_DataMemberAttribute) &&
            !propertySymbol.ContainingType.HasAttribute(KnownType.System_SerializableAttribute) &&
             propertySymbol.Type.OriginalDefinition.DerivesOrImplementsAny(collectionTypes) &&
            !propertySymbol.Type.OriginalDefinition.DerivesOrImplementsAny(ignoredCollectionTypes) &&
            !IsInterfaceImplementation(propertySymbol);

        private static bool HasPublicSetter(IPropertySymbol propertySymbol) =>
            propertySymbol.SetMethod != null &&
            !privateOrInternalAccessibility.Contains(propertySymbol.GetEffectiveAccessibility()) &&
            !privateOrInternalAccessibility.Contains(propertySymbol.SetMethod.DeclaredAccessibility);

        private static bool IsInterfaceImplementation(IPropertySymbol propertySymbol)
        {
            foreach (var @interface in propertySymbol.ContainingType.Interfaces)
            {
                if (@interface.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(x => x.Name == propertySymbol.Name)
                    .Any(x => propertySymbol.ContainingType.FindImplementationForInterfaceMember(x) == propertySymbol))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
