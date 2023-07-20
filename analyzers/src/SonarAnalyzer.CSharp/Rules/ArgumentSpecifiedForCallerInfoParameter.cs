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
    public sealed class ArgumentSpecifiedForCallerInfoParameter : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3236";
        private const string MessageFormat = "Remove this argument from the method call; it hides the caller information.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<KnownType> CallerInfoAttributesToReportOn =
            ImmutableArray.Create(
                KnownType.System_Runtime_CompilerServices_CallerArgumentExpressionAttribute,
                KnownType.System_Runtime_CompilerServices_CallerFilePathAttribute,
                KnownType.System_Runtime_CompilerServices_CallerLineNumberAttribute);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
            {
                if (new CSharpMethodParameterLookup((InvocationExpressionSyntax)c.Node, c.SemanticModel) is { MethodSymbol: { } } methodParameterLookup
                    && methodParameterLookup.GetAllArgumentParameterMappings() is { } argumentMappings)
                {
                    foreach (var argumentMapping in argumentMappings.Where(x =>
                        x.Symbol.GetAttributes(CallerInfoAttributesToReportOn).Any()
                        && !IsArgumentPassthroughOfParameter(c.SemanticModel, x.Node, x.Symbol)))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, argumentMapping.Node.GetLocation()));
                    }
                }
            }, SyntaxKind.InvocationExpression);

        private static bool IsArgumentPassthroughOfParameter(SemanticModel semanticModel, ArgumentSyntax argument, IParameterSymbol targetParameter) =>
            semanticModel.GetSymbolInfo(argument.Expression).Symbol is IParameterSymbol sourceParameter // the argument passed to the method is itself an parameter.
                                                                                                        // Let's check if it has the same attributes.
                && sourceParameter.GetAttributes(CallerInfoAttributesToReportOn).ToList() is var sourceAttributes
                && targetParameter.GetAttributes(CallerInfoAttributesToReportOn).ToList() is var targetAttributes
                && targetAttributes.All(target => sourceAttributes.Any(source => target.AttributeClass.Name == source.AttributeClass.Name));
    }
}
