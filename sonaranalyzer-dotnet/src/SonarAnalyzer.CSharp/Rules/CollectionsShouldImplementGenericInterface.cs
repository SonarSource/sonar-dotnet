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
    public sealed class CollectionsShouldImplementGenericInterface : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3909";
        private const string MessageFormat = "Refactor this collection to implement '{0}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly Dictionary<string, KnownType> nongenericToGenericMapping =
            new Dictionary<string, KnownType>
            {
                [KnownType.System_Collections_ICollection.TypeName] = KnownType.System_Collections_Generic_ICollection_T,
                [KnownType.System_Collections_IList.TypeName] = KnownType.System_Collections_Generic_IList_T,
                [KnownType.System_Collections_IEnumerable.TypeName] = KnownType.System_Collections_Generic_IEnumerable_T,
                [KnownType.System_Collections_CollectionBase.TypeName] = KnownType.System_Collections_ObjectModel_Collection_T,
            };

        private static readonly ImmutableArray<KnownType> genericTypes = nongenericToGenericMapping.Values.ToImmutableArray();

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var classDeclaration = c.Node as ClassDeclarationSyntax;

                    var implementedTypes = classDeclaration?.BaseList?.Types;
                    if (implementedTypes == null)
                    {
                        return;
                    }

                    var issues = new List<Diagnostic>();
                    foreach (var typeSyntax in implementedTypes)
                    {
                        var typeSymbol = c.SemanticModel.GetSymbolInfo(typeSyntax.Type).Symbol?.GetSymbolType();
                        if (typeSymbol == null)
                        {
                            continue;
                        }

                        if (typeSymbol.OriginalDefinition.IsAny(genericTypes))
                        {
                            return;
                        }

                        var suggestedGenericType = SuggestGenericCollectionType(typeSymbol);
                        if (suggestedGenericType != null)
                        {
                            issues.Add(Diagnostic.Create(rule,
                                        classDeclaration.Identifier.GetLocation(),
                                        suggestedGenericType));
                        }
                    }

                    issues.ForEach(d => c.ReportDiagnosticWhenActive(d));
                },
                SyntaxKind.ClassDeclaration);
        }

        private static string SuggestGenericCollectionType(ITypeSymbol typeSymbol)
        {
            return nongenericToGenericMapping.GetValueOrDefault(typeSymbol.ToDisplayString())?.TypeName;
        }
    }
}
