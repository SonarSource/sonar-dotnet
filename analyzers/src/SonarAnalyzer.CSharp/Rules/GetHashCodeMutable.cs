/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules
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
                    var methodSymbol = c.Model.GetDeclaredSymbol(methodSyntax);

                    if (methodSymbol.ContainingType.IsValueType || !methodSymbol.IsObjectGetHashCode())
                    {
                        return;
                    }

                    ImmutableArray<ISymbol> baseMembers;
                    try
                    {
                        baseMembers = c.Model.LookupBaseMembers(methodSyntax.SpanStart);
                    }
                    catch (ArgumentException)
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

                    var secondaryLocations = GetAllFirstMutableFieldsUsed(c, fieldsOfClass, identifiers).Select(CreateSecondaryLocation).ToArray();
                    if (secondaryLocations.Any())
                    {
                        c.ReportIssue(Rule, methodSyntax.Identifier, secondaryLocations);
                    }
                },
                SyntaxKind.MethodDeclaration);

        private static SecondaryLocation CreateSecondaryLocation(SimpleNameSyntax identifierSyntax) =>
            new(identifierSyntax.GetLocation(), string.Format(SecondaryMessageFormat, identifierSyntax.Identifier.Text));

        private static IEnumerable<IdentifierNameSyntax> GetAllFirstMutableFieldsUsed(SonarSyntaxNodeReportingContext context,
                                                                                      ICollection<IFieldSymbol> fieldsOfClass,
                                                                                      IEnumerable<IdentifierNameSyntax> identifiers)
        {
            var syntaxNodes = new Dictionary<IFieldSymbol, List<IdentifierNameSyntax>>();

            foreach (var identifier in identifiers)
            {
                if (context.Model.GetSymbolInfo(identifier).Symbol is not IFieldSymbol identifierSymbol)
                {
                    continue;
                }
                if (!syntaxNodes.ContainsKey(identifierSymbol))
                {
                    if (!IsFieldRelevant(identifierSymbol, fieldsOfClass))
                    {
                        continue;
                    }
                    syntaxNodes.Add(identifierSymbol, []);
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
