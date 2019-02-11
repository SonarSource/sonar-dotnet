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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class PropertiesShouldBePreferred : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4049";
        private const string MessageFormat = "Consider making method '{0}' a property.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    if (!(c.SemanticModel.GetDeclaredSymbol(c.Node) is INamedTypeSymbol typeSymbol))
                    {
                        return;
                    }

                    var propertyCandidates = typeSymbol
                        .GetMembers()
                        .OfType<IMethodSymbol>()
                        .Where(SymbolHelper.IsPubliclyAccessible)
                        .Where(IsPropertyCanditate);

                    foreach (var candidate in propertyCandidates)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(
                            rule,
                            candidate.Locations.FirstOrDefault(),
                            messageArgs: candidate.Name));
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKind.InterfaceDeclaration);
        }

        private bool IsPropertyCanditate(IMethodSymbol method)
        {
            if (method.IsConstructor() ||
                method.MethodKind == MethodKind.PropertyGet ||
                method.IsOverride ||
                method.GetInterfaceMember() != null ||
                method.IsAsync ||
                method.ReturnType.OriginalDefinition.IsAny(KnownType.SystemTasks))
            {
                return false;
            }

            return method.Parameters.Length == 0 &&
                   !method.ReturnsVoid &&
                   !method.ReturnType.Is(TypeKind.Array) &&
                   method.Name != "GetEnumerator" &&
                   method.Name != "GetAwaiter" &&
                   NameStartsWithGet(method);
        }

        private bool NameStartsWithGet(IMethodSymbol method)
        {
            var nameParts = method.Name.SplitCamelCaseToWords().ToList();

            return nameParts.Count > 1 &&
                   nameParts[0] == "GET";
        }
    }
}
