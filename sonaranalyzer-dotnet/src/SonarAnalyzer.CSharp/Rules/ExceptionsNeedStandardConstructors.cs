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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ExceptionsNeedStandardConstructors : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4027";
        private const string MessageFormat = "Implement the missing constructors for this exception.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var classDeclaration = c.Node as ClassDeclarationSyntax;
                var classSymbol = c.SemanticModel.GetDeclaredSymbol(classDeclaration);

                if (!classDeclaration.Identifier.IsMissing &&
                    classSymbol.DerivesFrom(KnownType.System_Exception) &&
                    !HasStandardConstructors(classSymbol))
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, classDeclaration.Identifier.GetLocation()));
                }
            },
            SyntaxKind.ClassDeclaration);
        }

        private static bool HasStandardConstructors(INamedTypeSymbol classSymbol)
        {
            var ctors = classSymbol.Constructors;

            return HasConstructor(ctors, Accessibility.Public) &&
                   HasConstructor(ctors, Accessibility.Public, KnownType.System_String) &&
                   HasConstructor(ctors, Accessibility.Public, KnownType.System_String, KnownType.System_Exception) &&
                   HasConstructor(ctors,
                       classSymbol.IsSealed ? Accessibility.Private : Accessibility.Protected,
                       KnownType.System_Runtime_Serialization_SerializationInfo,
                       KnownType.System_Runtime_Serialization_StreamingContext);
        }

        private static bool HasConstructor(ImmutableArray<IMethodSymbol> constructors,
            Accessibility accessibility, params KnownType[] expectedParameterTypes)
        {
            return constructors.Any(c => IsMatchingConstructor(c, accessibility, expectedParameterTypes));
        }

        private static bool IsMatchingConstructor(IMethodSymbol constructor, Accessibility accessibility,
            KnownType[] expectedParameterTypes)
        {
            return constructor.DeclaredAccessibility == accessibility &&
                   EnumerableExtensions.Equals(constructor.Parameters, expectedParameterTypes,
                       (p1, p2) => p1.Type.Is(p2));
        }
    }
}
