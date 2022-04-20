/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CollectionsShouldImplementGenericInterface : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3909";
        private const string MessageFormat = "Refactor this collection to implement '{0}'.";

        private static readonly DiagnosticDescriptor Rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        private static readonly Dictionary<string, KnownType> NongenericToGenericMapping =
            new()
            {
                { KnownType.System_Collections_ICollection.TypeName, KnownType.System_Collections_Generic_ICollection_T },
                { KnownType.System_Collections_IList.TypeName, KnownType.System_Collections_Generic_IList_T },
                { KnownType.System_Collections_IEnumerable.TypeName, KnownType.System_Collections_Generic_IEnumerable_T },
                { KnownType.System_Collections_CollectionBase.TypeName, KnownType.System_Collections_ObjectModel_Collection_T },
            };

        private static readonly ImmutableArray<KnownType> GenericTypes = NongenericToGenericMapping.Values.ToImmutableArray();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    if (c.IsRedundantPositionalRecordContext())
                    {
                        return;
                    }

                    var typeDeclaration = (BaseTypeDeclarationSyntax)c.Node;
                    var implementedTypes = typeDeclaration?.BaseList?.Types;
                    if (implementedTypes == null)
                    {
                        return;
                    }

                    List<Diagnostic> issues = null;
                    foreach (var typeSyntax in implementedTypes)
                    {
                        var typeSymbol = c.SemanticModel.GetSymbolInfo(typeSyntax.Type).Symbol?.GetSymbolType();
                        if (typeSymbol == null)
                        {
                            continue;
                        }

                        if (typeSymbol.OriginalDefinition.IsAny(GenericTypes))
                        {
                            return;
                        }

                        var suggestedGenericType = SuggestGenericCollectionType(typeSymbol);
                        if (suggestedGenericType != null)
                        {
                            issues ??= new();
                            issues.Add(Diagnostic.Create(Rule,
                                        typeDeclaration.Identifier.GetLocation(),
                                        suggestedGenericType));
                        }
                    }

                    issues?.ForEach(d => c.ReportIssue(d));
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKindEx.RecordClassDeclaration,
                SyntaxKindEx.RecordStructDeclaration);
        }

        private static string SuggestGenericCollectionType(ITypeSymbol typeSymbol) =>
            NongenericToGenericMapping.GetValueOrDefault(typeSymbol.ToDisplayString())?.TypeName;
    }
}
