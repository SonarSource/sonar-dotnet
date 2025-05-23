﻿/*
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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ArgumentSpecifiedForCallerInfoParameter : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S3236";
    private const string MessageFormat = "Remove this argument from the method call; it hides the caller information.";

    private static readonly ImmutableArray<KnownType> CallerInfoAttributesToReportOn =
    ImmutableArray.Create(
        KnownType.System_Runtime_CompilerServices_CallerArgumentExpressionAttribute,
        KnownType.System_Runtime_CompilerServices_CallerFilePathAttribute,
        KnownType.System_Runtime_CompilerServices_CallerLineNumberAttribute);

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
        {
            if (new CSharpMethodParameterLookup((InvocationExpressionSyntax)c.Node, c.Model) is { MethodSymbol: { } } methodParameterLookup
                && !(methodParameterLookup.MethodSymbol.ContainingType.Is(KnownType.System_Diagnostics_Debug) && (methodParameterLookup.MethodSymbol.Name == "Assert"))
                && methodParameterLookup.GetAllArgumentParameterMappings() is { } argumentMappings)
            {
                foreach (var argumentMapping in argumentMappings.Where(x =>
                    x.Symbol.GetAttributes(CallerInfoAttributesToReportOn).Any()
                    && !IsArgumentPassthroughOfParameter(c.Model, x.Node, x.Symbol)))
                {
                    c.ReportIssue(Rule, argumentMapping.Node);
                }
            }
        }, SyntaxKind.InvocationExpression);

    private static bool IsArgumentPassthroughOfParameter(SemanticModel model, ArgumentSyntax argument, IParameterSymbol targetParameter) =>
        model.GetSymbolInfo(argument.Expression).Symbol is IParameterSymbol sourceParameter // the argument passed to the method is itself an parameter.
                                                                                            // Let's check if it has the same attributes.
            && sourceParameter.GetAttributes(CallerInfoAttributesToReportOn).ToList() is var sourceAttributes
            && targetParameter.GetAttributes(CallerInfoAttributesToReportOn).ToList() is var targetAttributes
            && targetAttributes.All(x => sourceAttributes.Any(y => x.AttributeClass.Name == y.AttributeClass.Name));
}
