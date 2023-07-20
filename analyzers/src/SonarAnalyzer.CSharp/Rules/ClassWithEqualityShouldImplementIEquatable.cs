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
    public sealed class ClassWithEqualityShouldImplementIEquatable : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3897";
        private const string MessageFormat = "Implement 'IEquatable<{0}>'.";
        private const string EqualsMethodName = nameof(object.Equals);

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)c.Node;
                    var classSymbol = c.SemanticModel.GetDeclaredSymbol(classDeclaration);
                    if (classSymbol == null ||
                        ImplementsIEquatableInterface(classSymbol) ||
                        classDeclaration.Identifier.IsMissing)
                    {
                        return;
                    }

                    classSymbol.GetMembers(EqualsMethodName)
                        .OfType<IMethodSymbol>()
                        .Where(IsIEquatableEqualsMethodCandidate)
                        .ToList()
                        .ForEach(ms => c.ReportIssue(CreateDiagnostic(rule,
                            classDeclaration.Identifier.GetLocation(),
                            ms.Parameters[0].Type.Name)));
                }, SyntaxKind.ClassDeclaration);
        }

        private bool ImplementsIEquatableInterface(ITypeSymbol classSymbol) =>
            classSymbol.AllInterfaces.Any(@interface =>
              @interface.ConstructedFrom.Is(KnownType.System_IEquatable_T) &&
              @interface.TypeArguments.Length == 1 &&
              @interface.TypeArguments[0].Equals(classSymbol));

        private static bool IsIEquatableEqualsMethodCandidate(IMethodSymbol methodSymbol)
        {
            return methodSymbol.MethodKind == MethodKind.Ordinary &&
                methodSymbol.Name == EqualsMethodName &&
                !methodSymbol.IsOverride &&
                methodSymbol.DeclaredAccessibility == Accessibility.Public &&
                methodSymbol.ReturnType.Is(KnownType.System_Boolean) &&
                methodSymbol.Parameters.Length == 1 &&
                methodSymbol.Parameters[0].Type.Equals(methodSymbol.ContainingType);

        }
    }
}
