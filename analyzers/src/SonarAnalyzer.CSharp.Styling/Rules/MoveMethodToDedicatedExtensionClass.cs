/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MoveMethodToDedicatedExtensionClass : StylingAnalyzer
{
    public MoveMethodToDedicatedExtensionClass() : base("T0046", "Move this extension method to the {0} class.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var method = (IMethodSymbol)c.ContainingSymbol;
                if (method.IsExtensionMethod && ExpectedContainingType(method) is { } expected)
                {
                    c.ReportIssue(Rule, ((MethodDeclarationSyntax)c.Node).Identifier, expected);
                }
            },
            SyntaxKind.MethodDeclaration);

    private static string ExpectedContainingType(IMethodSymbol method)
    {
        var expectedPrefixes = method.Parameters[0].Type switch
        {
            ITypeParameterSymbol typeParameter => typeParameter.ConstraintTypes.Where(x => x.IsType).Select(x => x.Name),
            INamedTypeSymbol namedType => ValidTypeArguments(namedType).Select(x => x.Name),
            _ => [method.Parameters[0].Type.Name]
        };

        if (!expectedPrefixes.Any() || expectedPrefixes.All(string.IsNullOrWhiteSpace))
        {
            return "ObjectExtensions";
        }
        else if (expectedPrefixes.Any(x => method.ContainingType.Name.Equals($"{x}Extensions")))
        {
            return null;
        }
        else
        {
            return expectedPrefixes.Select(x => $"{x}Extensions").JoinOr();
        }
    }

    private static IEnumerable<INamedTypeSymbol> ValidTypeArguments(INamedTypeSymbol namedType)
    {
        var validTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default)
        {
            namedType
        };
        foreach (var type in namedType.TypeArguments.OfType<INamedTypeSymbol>())
        {
            validTypes.AddRange(ValidTypeArguments(type));
        }
        return validTypes;
    }
}
