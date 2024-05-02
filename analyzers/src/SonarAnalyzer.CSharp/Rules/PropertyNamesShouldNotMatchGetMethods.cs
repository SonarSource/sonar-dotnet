/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
    public sealed class PropertyNamesShouldNotMatchGetMethods : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4059";
        private const string MessageFormat = "Change either the name of property '{0}' or the name of method '{1}' to make them distinguishable.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    if (c.SemanticModel.GetDeclaredSymbol(c.Node) is not INamedTypeSymbol typeSymbol)
                    {
                        return;
                    }
                    var typeMembers = typeSymbol.GetMembers().Where(x => x.IsPubliclyAccessible());
                    var properties = typeMembers.OfType<IPropertySymbol>().Where(property => !property.IsOverride).ToArray();
                    var methods = typeMembers.OfType<IMethodSymbol>().ToArray();

                    foreach (var collidingMembers in CollidingMembers(properties, methods))
                    {
                        var propertyIdentifier = collidingMembers.Item1;
                        var methodIdentifier = collidingMembers.Item2;
                        c.ReportIssue(Rule.CreateDiagnostic(c.Compilation,
                            propertyIdentifier.GetLocation(),
                            new[] { methodIdentifier.GetLocation() },
                            properties: null,
                            propertyIdentifier.ValueText,
                            methodIdentifier.ValueText));
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKindEx.RecordClassDeclaration,
                SyntaxKindEx.RecordStructDeclaration,
                SyntaxKind.StructDeclaration);

        private static IEnumerable<Tuple<SyntaxToken, SyntaxToken>> CollidingMembers(IPropertySymbol[] properties, IMethodSymbol[] methods)
        {
            foreach (var property in properties)
            {
                if (methods.FirstOrDefault(x => AreCollidingNames(property.Name, x.Name)) is { } collidingMethod
                    && collidingMethod.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is MethodDeclarationSyntax methodSyntax
                    && property.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is PropertyDeclarationSyntax propertySyntax)
                {
                    yield return new Tuple<SyntaxToken, SyntaxToken>(propertySyntax.Identifier, methodSyntax.Identifier);
                }
            }
        }

        private static bool AreCollidingNames(string propertyName, string methodName) =>
            methodName.Equals(propertyName, StringComparison.OrdinalIgnoreCase) || methodName.Equals("Get" + propertyName, StringComparison.OrdinalIgnoreCase);
    }
}
