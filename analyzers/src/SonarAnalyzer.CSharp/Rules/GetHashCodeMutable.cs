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
    public sealed class GetHashCodeMutable : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2328";
        private const string IssueMessage = "Refactor 'GetHashCode' to not reference mutable fields.";
        private const string SecondaryMessageFormat = "Remove this use of '{0}' or make it 'readonly'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, IssueMessage);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var methodSyntax = (MethodDeclarationSyntax)c.Node;
                    var methodSymbol = c.SemanticModel.GetDeclaredSymbol(methodSyntax);

                    if (!methodSymbol.IsObjectGetHashCode())
                    {
                        return;
                    }

                    ImmutableArray<ISymbol> baseMembers;
                    try
                    {
                        baseMembers = c.SemanticModel.LookupBaseMembers(methodSyntax.SpanStart);
                    }
                    catch (System.ArgumentException)
                    {
                        // this is expected on invalid code
                        return;
                    }

                    var fieldsOfClass = baseMembers
                                        .Concat(methodSymbol.ContainingType.GetMembers())
                                        .Select(symbol => symbol as IFieldSymbol)
                                        .WhereNotNull()
                                        .ToHashSet();

                    var identifiers = methodSyntax.DescendantNodes().OfType<IdentifierNameSyntax>();

                    var secondaryLocations = GetAllFirstMutableFieldsUsed(c, fieldsOfClass, identifiers).Select(CreateSecondaryLocation).ToList();
                    if (secondaryLocations.Count == 0)
                    {
                        return;
                    }

                    c.ReportIssue(CreateDiagnostic(
                        Rule,
                        methodSyntax.Identifier.GetLocation(),
                        secondaryLocations.ToAdditionalLocations(),
                        secondaryLocations.ToProperties()));
                },
                SyntaxKind.MethodDeclaration);

        private static SecondaryLocation CreateSecondaryLocation(SimpleNameSyntax identifierSyntax) =>
            new SecondaryLocation(identifierSyntax.GetLocation(), string.Format(SecondaryMessageFormat, identifierSyntax.Identifier.Text));

        private static IEnumerable<IdentifierNameSyntax> GetAllFirstMutableFieldsUsed(SonarSyntaxNodeReportingContext context,
                                                                                      ICollection<IFieldSymbol> fieldsOfClass,
                                                                                      IEnumerable<IdentifierNameSyntax> identifiers)
        {
            var syntaxNodes = new Dictionary<IFieldSymbol, List<IdentifierNameSyntax>>();

            foreach (var identifier in identifiers)
            {
                if (!(context.SemanticModel.GetSymbolInfo(identifier).Symbol is IFieldSymbol identifierSymbol))
                {
                    continue;
                }

                if (!syntaxNodes.ContainsKey(identifierSymbol))
                {
                    if (!IsFieldRelevant(identifierSymbol, fieldsOfClass))
                    {
                        continue;
                    }

                    syntaxNodes.Add(identifierSymbol, new List<IdentifierNameSyntax>());
                }

                syntaxNodes[identifierSymbol].Add(identifier);
            }

            return syntaxNodes.Values
                .Select(identifierReferences => identifierReferences.OrderBy(id => id.SpanStart).FirstOrDefault())
                .WhereNotNull();
        }

        private static bool IsFieldRelevant(IFieldSymbol fieldSymbol, ICollection<IFieldSymbol> fieldsOfClass) =>
            fieldSymbol is { IsConst: false, IsReadOnly: false }
            && fieldsOfClass.Contains(fieldSymbol);
    }
}
