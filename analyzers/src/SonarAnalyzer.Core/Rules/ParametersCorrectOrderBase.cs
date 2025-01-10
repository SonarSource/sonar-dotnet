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

namespace SonarAnalyzer.Rules
{
    public abstract class ParametersCorrectOrderBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S2234";
        protected abstract TSyntaxKind[] InvocationKinds { get; }
        protected override string MessageFormat => "Parameters to '{0}' have the same names but not the same order as the method arguments.";

        protected ParametersCorrectOrderBase() : base(DiagnosticId) { }

        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer,
                c =>
                {
                    const int MinNumberOfNameableArguments = 2;
                    if (!c.IsRedundantPrimaryConstructorBaseTypeContext()
                        && Language.Syntax.ArgumentList(c.Node) is { Count: >= MinNumberOfNameableArguments } argumentList  // there must be at least two arguments to be able to swap, and further
                        && argumentList.Select(ArgumentName).WhereNotNull().Take(MinNumberOfNameableArguments).Count() == MinNumberOfNameableArguments // at least two arguments with a "name"
                        && Language.MethodParameterLookup(c.Node, c.SemanticModel) is var methodParameterLookup)
                    {
                        foreach (var argument in argumentList)
                        {
                            // Example void M(int x, int y) <- p_x and p_y are the parameter
                            // M(y, x); <- a_y and a_x are the arguments
                            if (methodParameterLookup.TryGetSymbol(argument, out var parameterSymbol) // argument = a_x and parameterSymbol = p_y
                                && parameterSymbol is { IsParams: false }
                                && ArgumentName(argument) is { } argumentName // "x"
                                && !MatchingNames(parameterSymbol, argumentName)  // "x" != "y"
                                && Language.Syntax.NodeExpression(argument) is { } argumentExpression
                                && c.Context.SemanticModel.GetTypeInfo(argumentExpression).ConvertedType is { } argumentType
                                // is there another parameter that seems to be a better fit (name and type match): p_x
                                && methodParameterLookup.MethodSymbol.Parameters.FirstOrDefault(p => MatchingNames(p, argumentName)) is { IsParams: false } otherParameter
                                && argumentType.DerivesOrImplements(otherParameter.Type)
                                // is there an argument that matches the parameter p_y by name: a_y
                                && Language.Syntax.ArgumentList(c.Node).FirstOrDefault(x => MatchingNames(parameterSymbol, ArgumentName(x))) is { })
                            {
                                var secondaryLocations = methodParameterLookup.MethodSymbol.DeclaringSyntaxReferences
                                    .Select(x => Language.Syntax.NodeIdentifier(x.GetSyntax())?.ToSecondaryLocation())
                                    .WhereNotNull();
                                c.ReportIssue(Rule, PrimaryLocation(c.Node), secondaryLocations, methodParameterLookup.MethodSymbol.Name);
                                return;
                            }
                        }
                    }
                }, InvocationKinds);

        protected virtual Location PrimaryLocation(SyntaxNode node) =>
            Language.Syntax.NodeIdentifier(node)?.GetLocation() ?? node.GetLocation();

        private bool MatchingNames(IParameterSymbol parameter, string argumentName) =>
            Language.NameComparer.Equals(parameter.Name, argumentName);

        private string ArgumentName(SyntaxNode argument) =>
            Language.Syntax.NodeIdentifier(Language.Syntax.NodeExpression(argument))?.ValueText;
    }
}
