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
    public sealed class OverrideGetHashCodeOnOverridingEquals : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1206";
        private const string MessageFormat = "This {0} overrides '{1}' and should therefore also override '{2}'.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var declaration = (TypeDeclarationSyntax)c.Node;
                    var typeSymbol = c.SemanticModel.GetDeclaredSymbol(declaration);
                    if (typeSymbol == null)
                    {
                        return;
                    }

                    var overridenMethods = typeSymbol.GetMembers()
                        .OfType<IMethodSymbol>()
                        .Where(method => method.IsObjectEquals() || method.IsObjectGetHashCode())
                        .Select(method => method.Name)
                        .ToList();
                    if (overridenMethods.Count == 0 || overridenMethods.Count == 2)
                    {
                        return;
                    }

                    c.ReportIssue(CreateDiagnostic(rule, declaration.Identifier.GetLocation(),
                        declaration.Keyword.ValueText, overridenMethods[0], GetMissingMethodName(overridenMethods[0])));
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration);
        }

        private static string GetMissingMethodName(string overridenMethodName)
        {
            return overridenMethodName == nameof(object.Equals)
                ? nameof(object.GetHashCode)
                : nameof(object.Equals);
        }
    }
}
