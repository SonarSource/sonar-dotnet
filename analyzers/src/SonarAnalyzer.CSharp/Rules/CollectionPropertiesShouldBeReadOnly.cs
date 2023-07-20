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
    public sealed class CollectionPropertiesShouldBeReadOnly : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4004";
        private const string MessageFormat = "Make the '{0}' property read-only by removing the property setter or making it private.";

        private static readonly ImmutableArray<KnownType> CollectionTypes =
            ImmutableArray.Create(
                KnownType.System_Collections_Generic_ICollection_T,
                KnownType.System_Collections_ICollection);

        private static readonly ImmutableArray<KnownType> IgnoredCollectionTypes =
            ImmutableArray.Create(
                KnownType.System_Array,
                KnownType.System_Security_PermissionSet);

        private static readonly ISet<Accessibility> PrivateOrInternalAccessibility = new HashSet<Accessibility>
        {
            Accessibility.Private,
            Accessibility.Internal,
        };

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var propertyDeclaration = (PropertyDeclarationSyntax)c.Node;
                    var propertySymbol = c.SemanticModel.GetDeclaredSymbol(propertyDeclaration);

                    if (propertyDeclaration.AccessorList != null
                        && !propertyDeclaration.AccessorList.Accessors.AnyOfKind(SyntaxKindEx.InitAccessorDeclaration)
                        && propertySymbol != null
                        && HasPublicSetter(propertySymbol)
                        && IsObservedCollectionType(propertySymbol))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, propertyDeclaration.Identifier.GetLocation(), propertySymbol.Name));
                    }
                },
                SyntaxKind.PropertyDeclaration);

        private static bool IsObservedCollectionType(IPropertySymbol propertySymbol) =>
            !propertySymbol.IsOverride
            && !propertySymbol.HasAttribute(KnownType.System_Runtime_Serialization_DataMemberAttribute)
            && !propertySymbol.ContainingType.HasAttribute(KnownType.System_SerializableAttribute)
            && propertySymbol.Type.OriginalDefinition.DerivesOrImplementsAny(CollectionTypes)
            && !propertySymbol.Type.OriginalDefinition.DerivesOrImplementsAny(IgnoredCollectionTypes)
            && !IsInterfaceImplementation(propertySymbol);

        private static bool HasPublicSetter(IPropertySymbol propertySymbol) =>
            propertySymbol.SetMethod != null
            && !PrivateOrInternalAccessibility.Contains(propertySymbol.GetEffectiveAccessibility())
            && !PrivateOrInternalAccessibility.Contains(propertySymbol.SetMethod.DeclaredAccessibility);

        private static bool IsInterfaceImplementation(IPropertySymbol propertySymbol)
        {
            foreach (var @interface in propertySymbol.ContainingType.Interfaces)
            {
                if (@interface.GetMembers()
                              .OfType<IPropertySymbol>()
                              .Where(x => x.Name == propertySymbol.Name)
                              .Any(x => Equals(propertySymbol.ContainingType.FindImplementationForInterfaceMember(x), propertySymbol)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
