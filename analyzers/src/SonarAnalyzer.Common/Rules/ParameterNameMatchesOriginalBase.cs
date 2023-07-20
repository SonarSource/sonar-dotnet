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

namespace SonarAnalyzer.Rules
{
    public abstract class ParameterNameMatchesOriginalBase<TSyntaxKind, TMethodDeclarationSyntax> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
        where TMethodDeclarationSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S927";

        protected abstract TSyntaxKind[] SyntaxKinds { get; }
        protected abstract IEnumerable<SyntaxToken> ParameterIdentifiers(TMethodDeclarationSyntax method);

        protected override string MessageFormat => "Rename parameter '{0}' to '{1}' to match the {2} declaration.";

        protected ParameterNameMatchesOriginalBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
                {
                    var methodSyntax = (TMethodDeclarationSyntax)c.Node;
                    if (c.SemanticModel.GetDeclaredSymbol(methodSyntax) is IMethodSymbol methodSymbol && methodSymbol.Parameters.Any())
                    {
                        if (methodSymbol.PartialImplementationPart != null)
                        {
                            if (methodSymbol.PartialImplementationPart.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is TMethodDeclarationSyntax methodImplementationSyntax)
                            {
                                VerifyParameters(c, methodImplementationSyntax, methodSymbol.Parameters, "partial class");
                            }
                        }
                        else if (methodSymbol.OverriddenMethod != null)
                        {
                            VerifyGenericParameters(c, methodSyntax, methodSymbol.Parameters, methodSymbol.OverriddenMethod.OriginalDefinition.Parameters, "base class");
                        }
                        else if (methodSymbol.GetInterfaceMember() is { } interfaceMember)
                        {
                            VerifyGenericParameters(c, methodSyntax, methodSymbol.Parameters, interfaceMember.OriginalDefinition.Parameters, "interface");
                        }
                    }
                },
                SyntaxKinds);

        private void VerifyParameters(SonarSyntaxNodeReportingContext context, TMethodDeclarationSyntax methodSyntax, IList<IParameterSymbol> expectedParameters, string expectedLocation)
        {
            foreach (var item in ParameterIdentifiers(methodSyntax)
                                    .Zip(expectedParameters, (actual, expected) => new { actual, expected })
                                    .Where(x => !x.actual.ValueText.Equals(x.expected.Name, Language.NameComparison)))
            {
                context.ReportIssue(CreateDiagnostic(Rule, item.actual.GetLocation(), item.actual.ValueText, item.expected.Name, expectedLocation));
            }
        }

        private void VerifyGenericParameters(SonarSyntaxNodeReportingContext context,
                                             TMethodDeclarationSyntax methodSyntax,
                                             IList<IParameterSymbol> actualParameters,
                                             IList<IParameterSymbol> expectedParameters,
                                             string expectedLocation)
        {
            var parameters = ParameterIdentifiers(methodSyntax).ToList();
            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                var expectedParameter = expectedParameters[i];
                if (!parameter.ValueText.Equals(expectedParameter.Name, Language.NameComparison)
                    && !AreGenericTypeParametersWithDifferentTypes(actualParameters[i].Type, expectedParameter.Type)
                    && (expectedParameter.Type.Kind != SymbolKind.TypeParameter
                        || actualParameters[i].Type.Kind == SymbolKind.TypeParameter))
                {
                    context.ReportIssue(CreateDiagnostic(Rule, parameter.GetLocation(), parameter.ValueText, expectedParameter.Name, expectedLocation));
                }
            }
        }

        private static bool AreGenericTypeParametersWithDifferentTypes(ITypeSymbol actualType, ITypeSymbol expectedType) =>
            actualType is INamedTypeSymbol actualNamedType
            && expectedType is INamedTypeSymbol expectedNamedType
            && AreTypeArgumentsDifferent(actualNamedType, expectedNamedType);

        private static bool AreTypeArgumentsDifferent(INamedTypeSymbol namedType, INamedTypeSymbol otherNamedType)
        {
            for (var i = 0; i < namedType.TypeArguments.Count(); i++)
            {
                if (namedType.TypeArguments[i].TypeKind != otherNamedType.TypeArguments[i].TypeKind)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
