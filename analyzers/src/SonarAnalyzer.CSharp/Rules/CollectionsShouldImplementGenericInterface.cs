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
    public sealed class CollectionsShouldImplementGenericInterface : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3909";
        private const string MessageFormat = "Refactor this collection to implement '{0}'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        private static readonly Dictionary<KnownType, string> NonGenericToGenericMapping = new()
        {
            { KnownType.System_Collections_ICollection, "System.Collections.Generic.ICollection<T>" },
            { KnownType.System_Collections_IList, "System.Collections.Generic.IList<T>" },
            { KnownType.System_Collections_IEnumerable, "System.Collections.Generic.IEnumerable<T>" },
            { KnownType.System_Collections_CollectionBase, "System.Collections.ObjectModel.Collection<T>" }
        };

        private static readonly ImmutableArray<KnownType> GenericTypes = ImmutableArray.Create(
            KnownType.System_Collections_Generic_ICollection_T,
            KnownType.System_Collections_Generic_IList_T,
            KnownType.System_Collections_Generic_IEnumerable_T,
            KnownType.System_Collections_ObjectModel_Collection_T);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    var typeDeclaration = (BaseTypeDeclarationSyntax)c.Node;
                    var implementedTypes = typeDeclaration.BaseList?.Types;
                    if (implementedTypes == null || c.IsRedundantPositionalRecordContext())
                    {
                        return;
                    }

                    List<Diagnostic> issues = null;
                    var containingType = (INamedTypeSymbol)c.ContainingSymbol;
                    foreach (var typeSymbol in containingType.Interfaces.Concat(new[] { containingType.BaseType }).WhereNotNull())
                    {
                        if (typeSymbol.OriginalDefinition.IsAny(GenericTypes))
                        {
                            return;
                        }

                        if (SuggestGenericCollectionType(typeSymbol) is { } suggestedGenericType)
                        {
                            issues ??= new();
                            issues.Add(CreateDiagnostic(Rule, typeDeclaration.Identifier.GetLocation(), suggestedGenericType));
                        }
                    }

                    issues?.ForEach(d => c.ReportIssue(d));
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKindEx.RecordClassDeclaration,
                SyntaxKindEx.RecordStructDeclaration);

        private static string SuggestGenericCollectionType(ITypeSymbol typeSymbol) =>
            NonGenericToGenericMapping.FirstOrDefault(pair => pair.Key.Matches(typeSymbol)).Value;
    }
}
