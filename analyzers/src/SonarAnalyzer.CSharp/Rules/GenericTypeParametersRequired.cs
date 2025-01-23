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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GenericTypeParametersRequired : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4018";
        private const string MessageFormat = "Refactor this method to use all type parameters in the parameter list to enable type inference.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)c.Node;

                    var typeParameters = methodDeclaration
                            .TypeParameterList
                            ?.Parameters
                            .Select(p => c.Model.GetDeclaredSymbol(p));

                    if (typeParameters == null)
                    {
                        return;
                    }

                    var argumentTypes = methodDeclaration
                            .ParameterList
                            .Parameters
                            .Select(p => c.Model.GetDeclaredSymbol(p)?.Type);

                    var typeParametersInArguments = new HashSet<ITypeParameterSymbol>();
                    foreach (var argumentType in argumentTypes)
                    {
                        AddTypeParameters(argumentType, typeParametersInArguments);
                    }

                    if (typeParameters.Except(typeParametersInArguments).Any())
                    {
                        c.ReportIssue(Rule, methodDeclaration.Identifier);
                    }
                },
                SyntaxKind.MethodDeclaration);

        private static void AddTypeParameters(ITypeSymbol argumentSymbol, ISet<ITypeParameterSymbol> set)
        {
            var localArgumentSymbol = argumentSymbol;

            if (localArgumentSymbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                localArgumentSymbol = arrayTypeSymbol.ElementType;
            }

            if (localArgumentSymbol.Is(TypeKind.TypeParameter))
            {
                set.Add(localArgumentSymbol as ITypeParameterSymbol);
            }

            if (localArgumentSymbol is INamedTypeSymbol namedSymbol)
            {
                foreach (var typeParam in namedSymbol.TypeArguments)
                {
                    AddTypeParameters(typeParam, set);
                }
            }
        }
    }
}
