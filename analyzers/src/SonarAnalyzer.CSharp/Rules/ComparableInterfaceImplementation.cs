/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ComparableInterfaceImplementation : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1210";
        private const string MessageFormat = "When implementing {0}, you should also override {1}.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private const string ObjectEquals = nameof(object.Equals);

        private static readonly ImmutableArray<KnownType> ComparableInterfaces =
            ImmutableArray.Create(
                KnownType.System_IComparable,
                KnownType.System_IComparable_T);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var classDeclaration = (TypeDeclarationSyntax)c.Node;

                    var classSymbol = c.SemanticModel.GetDeclaredSymbol(classDeclaration);
                    if (classSymbol == null)
                    {
                        return;
                    }

                    var baseImplementsIComparable = classSymbol
                        .BaseType
                        .GetSelfAndBaseTypes()
                        .Any(t => t.ImplementsAny(ComparableInterfaces));
                    if (baseImplementsIComparable)
                    {
                        return;
                    }

                    var implementedComparableInterfaces = GetImplementedComparableInterfaces(classSymbol);
                    if (!implementedComparableInterfaces.Any())
                    {
                        return;
                    }

                    var classMembers = classSymbol.GetMembers().OfType<IMethodSymbol>();

                    var membersToOverride = GetMembersToOverride(classMembers).ToList();

                    if (membersToOverride.Any())
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(
                            rule,
                            classDeclaration.Identifier.GetLocation(),
                            string.Join(" or ", implementedComparableInterfaces),
                            string.Join(", ", membersToOverride)));
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration);

        private static IEnumerable<string> GetImplementedComparableInterfaces(INamedTypeSymbol classSymbol) =>
            classSymbol
            .Interfaces
            .Where(i => i.OriginalDefinition.IsAny(ComparableInterfaces))
            .Select(GetClassNameOnly)
            .ToList();

        private static string GetClassNameOnly(INamedTypeSymbol typeSymbol) =>
            typeSymbol.OriginalDefinition.ToDisplayString()
                .Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                .Last();

        private static IEnumerable<string> GetMembersToOverride(IEnumerable<IMethodSymbol> methods)
        {
            if (!methods.Any(KnownMethods.IsObjectEquals))
            {
                yield return ObjectEquals;
            }

            var overridenOperators = methods
                .Where(m => m.MethodKind == MethodKind.UserDefinedOperator)
                .Select(m => m.ComparisonKind());

            foreach (var comparisonKind in ComparisonKinds.Except(overridenOperators))
            {
                yield return comparisonKind.CSharp();
            }
        }

        private static readonly ComparisonKind[] ComparisonKinds =
            Enum.GetValues(typeof(ComparisonKind)).Cast<ComparisonKind>()
                .Where(x => x != ComparisonKind.None)
                .ToArray();
    }
}
