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
public sealed class LambdaParameterName : StylingAnalyzer
{
    public LambdaParameterName() : base("T0010", "Use 'x' for the lambda parameter name.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                if (c.Node is SimpleLambdaExpressionSyntax { Parameter: { Identifier.ValueText: not ("x" or "_") } parameter, Parent: { } parent } lambda
                    && parent.FirstAncestorOrSelf<LambdaExpressionSyntax>() is null
                    && !IsSonarContextAction(c)
                    && !MatchesDelegateParameterName(c.Model, lambda))
                {
                    c.ReportIssue(Rule, parameter);
                }
            },
            SyntaxKind.SimpleLambdaExpression);

    private static bool MatchesDelegateParameterName(SemanticModel model, SimpleLambdaExpressionSyntax lambda) =>
        model.GetTypeInfo(lambda).ConvertedType is INamedTypeSymbol
        {
            TypeKind: TypeKind.Delegate,
            DelegateInvokeMethod.Parameters: { Length: 1 } parameters
        } delegateType
        && !IsFuncOrAction(delegateType)
        && parameters[0] is { Name: { } parameterName }
        && parameterName == lambda.Parameter.Identifier.ValueText;

    private static bool IsFuncOrAction(INamedTypeSymbol delegateType) =>
        delegateType.IsAny(KnownType.System_Func_T_TResult, KnownType.System_Action_T);

    private static bool IsSonarContextAction(SonarSyntaxNodeReportingContext context) =>
        context.Model.GetSymbolInfo(context.Node).Symbol is IMethodSymbol lambda
        && lambda.ReturnsVoid
        && IsSonarContextName(lambda.Parameters.Single().Type.Name);

    private static bool IsSonarContextName(string name) =>
        name.StartsWith("Sonar") && (name.EndsWith("AnalysisContext") || name.EndsWith("ReportingContext"));
}
