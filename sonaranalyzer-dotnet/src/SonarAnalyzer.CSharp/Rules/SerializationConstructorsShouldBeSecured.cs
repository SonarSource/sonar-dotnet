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
    public sealed class SerializationConstructorsShouldBeSecured : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4212";
        private const string MessageFormat = "Secure this serialization constructor.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly AttributeComparer attributeComparer = new AttributeComparer();

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(FindPossibleViolations,
                SyntaxKind.ConstructorDeclaration);
        }

        private void FindPossibleViolations(SyntaxNodeAnalysisContext c)
        {
            var constructorSyntax = (ConstructorDeclarationSyntax)c.Node;
            var reportLocation = constructorSyntax?.Identifier.GetLocation();
            if (reportLocation == null)
            {
                return;
            }

            var serializationConstructor = c.SemanticModel.GetDeclaredSymbol(constructorSyntax);
            if (!serializationConstructor.IsSerializationConstructor())
            {
                return;
            }

            var classSymbol = serializationConstructor.ContainingType;
            if (!classSymbol.Implements(KnownType.System_Runtime_Serialization_ISerializable))
            {
                return;
            }

            var isAssemblyIsPartiallyTrusted = c.SemanticModel.Compilation.Assembly
                .GetAttributes(KnownType.System_Security_AllowPartiallyTrustedCallersAttribute)
                .Any();
            if (!isAssemblyIsPartiallyTrusted)
            {
                return;
            }

            var serializationConstructorAttributes = GetCASAttributes(serializationConstructor).ToHashSet();

            bool isConstructorMissingAttributes = classSymbol.Constructors
                .SelectMany(m => GetCASAttributes(m))
                .Any(attr => !serializationConstructorAttributes.Contains(attr, attributeComparer));

            if (isConstructorMissingAttributes)
            {
                c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, reportLocation));
            }
        }

        private static IEnumerable<AttributeData> GetCASAttributes(IMethodSymbol methodSymbol)
        {
            return methodSymbol.GetAttributes().Where(IsCASAttribute);

            bool IsCASAttribute(AttributeData data) =>
                data?.AttributeClass.DerivesFrom(KnownType.System_Security_Permissions_CodeAccessSecurityAttribute)
                    ?? false;
        }

        private class AttributeComparer : IEqualityComparer<AttributeData>
        {
            public bool Equals(AttributeData x, AttributeData y)
            {
                return Equals(x.AttributeConstructor, y.AttributeConstructor) &&
                    Enumerable.SequenceEqual(x.ConstructorArguments, y.ConstructorArguments) &&
                    AreNamedArgumentsEqual(x.NamedArguments, y.NamedArguments);
            }

            private bool AreNamedArgumentsEqual(
                IEnumerable<KeyValuePair<string, TypedConstant>> argumentsX,
                IEnumerable<KeyValuePair<string, TypedConstant>> argumentsY)
            {
                var dictX = argumentsX.ToDictionary(p => p.Key, p => p.Value);
                var dictY = argumentsY.ToDictionary(p => p.Key, p => p.Value);

                if (dictX.Count != dictY.Count)
                {
                    return false;
                }

                foreach (var key in dictX.Keys)
                {
                    if (!dictX.TryGetValue(key, out TypedConstant itemX) ||
                        !dictY.TryGetValue(key, out TypedConstant itemY) ||
                        !Equals(itemX, itemY))
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(AttributeData obj) => 1;
        }
    }
}
