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

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseInnermostRegistrationContext : StylingAnalyzer
{
    public UseInnermostRegistrationContext() : base("T0005", "Use inner-most registration context '{0}' instead of '{1}'.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStart =>
        {
            const string contextNameSpace = "SonarAnalyzer.Core.AnalysisContext.";
            var allContextTypes = new[]
            {
                nameof(SonarAnalysisContext),
                nameof(SonarCompilationStartAnalysisContext),
                $"{nameof(SonarCodeBlockStartAnalysisContext<int>)}`1",
                nameof(SonarSymbolStartAnalysisContext),
                nameof(SonarParametrizedAnalysisContext),
                nameof(IReport),
            }.Select(x => compilationStart.Compilation.GetTypeByMetadataName($"{contextNameSpace}{x}")).WhereNotNull().ToImmutableArray();
            if (allContextTypes.IsEmpty)
            {
                return;
            }
            var registrationContextTypes = allContextTypes.Where(x => x.Name != nameof(IReport)).ToImmutableArray();
            compilationStart.RegisterNodeAction(c =>
            {
                if (c.Node is ParameterSyntax outterParameter
                    && outterParameter.GetName() is { } outterParameterName
                    && RegistrationContextParameterSymbol(c.Model, registrationContextTypes, outterParameter) is { } outterParameterSymbol)
                {
                    // Find all inner registrations by looking for other parameters of a context type (IReport or one of the registration context types)
                    // Inside the scopes of such parameters, outterParameter isn't allowed to be used.
                    var innerContextParameters = outterParameter.Parent.EnclosingScope()
                        .DescendantNodes(x => !(outterParameter.Parent is ParameterListSyntax && x == outterParameter.Parent)) // Don't consider other parameters of the same method as inner parameters
                        .OfType<ParameterSyntax>()
                        .Where(x => x != outterParameter) // SimpleLambda parameter are not excluded by the DescendantNodes filter above
                        .Select(x => new NodeAndSymbol(x, RegistrationOrReportingContextParameterSymbol(c.Model, allContextTypes, x)))
                        .Where(x => x.Symbol is not null)
                        .ToImmutableArray();
                    var violationNodes = ViolationsInInnerScopes(c.Model, outterParameterSymbol, innerContextParameters);
                    foreach (var violation in violationNodes)
                    {
                        c.ReportIssue(Rule, violation.Node, violation.Symbol.Name, outterParameterName);
                    }
                }
            }, SyntaxKind.Parameter);
        });

    private static IEnumerable<NodeAndSymbol> ViolationsInInnerScopes(SemanticModel model, IParameterSymbol outterParameterSymbol, ImmutableArray<NodeAndSymbol> innerContextParameters)
    {
        Dictionary<SyntaxNode, ISymbol> violations = [];
        foreach (var innerContextParameter in innerContextParameters)
        {
            var innerScope = innerContextParameter.Node.Parent.EnclosingScope(); // outterParameter should not be used inside these scopes
            foreach (var node in innerScope.DescendantNodes().OfType<IdentifierNameSyntax>().Where(x =>
                x.GetName() == outterParameterSymbol.Name
                && model.GetSymbolInfo(x).Symbol is IParameterSymbol usedParameter
                && SymbolEqualityComparer.Default.Equals(usedParameter, outterParameterSymbol)))
            {
                // There might be mutiple inner context parameters we pass until we find a violation,
                // but only the last one we have seen is the most inner one. The dictionaries add or
                // replace logic solves this for us.
                violations[node] = innerContextParameter.Symbol;
            }
        }
        return violations.Select(x => new NodeAndSymbol(x.Key, x.Value));
    }

    private static IParameterSymbol RegistrationContextParameterSymbol(SemanticModel model, ImmutableArray<INamedTypeSymbol> registrationContextTypes, ParameterSyntax parameter)
    {
        // Syntax based check to exclude parameters with a Type node that are known to be not registration context types
        if (parameter.Type is { } type
            && type.GetName() is { } typeName
            && !registrationContextTypes.Any(x => x.Name == typeName))
        {
            return null;
        }
        return model.GetDeclaredSymbol(parameter) is IParameterSymbol parameterSymbol
            && IsRegistrationParameter(registrationContextTypes, parameterSymbol)
            ? parameterSymbol
            : null;
    }

    private static bool IsRegistrationParameter(ImmutableArray<INamedTypeSymbol> registrationContextTypes, IParameterSymbol parameterSymbol) =>
        registrationContextTypes.Any(parameterSymbol.Type.DerivesFrom);

    private static IParameterSymbol RegistrationOrReportingContextParameterSymbol(SemanticModel model,
                                                                                  ImmutableArray<INamedTypeSymbol> allContextTypes,
                                                                                  ParameterSyntax parameter) =>
        model.GetDeclaredSymbol(parameter) is IParameterSymbol parameterSymbol
            && allContextTypes.Any(parameterSymbol.Type.DerivesOrImplements)
            ? parameterSymbol
            : null;
}
