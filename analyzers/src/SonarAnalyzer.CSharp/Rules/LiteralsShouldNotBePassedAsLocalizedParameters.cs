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
    public sealed class LiteralsShouldNotBePassedAsLocalizedParameters : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4055";
        private const string MessageFormat = "Replace this string literal with a string retrieved through an instance of the 'ResourceManager' class.";

        private static readonly HashSet<string> LocalizableSymbolNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "TEXT",
            "CAPTION",
            "MESSAGE"
        };

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(AnalyzeInvocations, SyntaxKind.InvocationExpression);
            context.RegisterNodeAction(AnalyzeAssignments, SyntaxKind.SimpleAssignmentExpression);
        }

        private static void AnalyzeInvocations(SonarSyntaxNodeReportingContext context)
        {
            var invocationSyntax = (InvocationExpressionSyntax)context.Node;
            if (!(context.SemanticModel.GetSymbolInfo(invocationSyntax).Symbol is IMethodSymbol methodSymbol) || invocationSyntax.ArgumentList is null)
            {
                return;
            }

            // Calling to/from debug-only code
            if (methodSymbol.IsDiagnosticDebugMethod()
                || methodSymbol.IsConditionalDebugMethod()
                || invocationSyntax.IsInConditionalDebug(context.SemanticModel))
            {
                return;
            }

            if (methodSymbol.IsConsoleWrite() || methodSymbol.IsConsoleWriteLine())
            {
                var firstArgument = invocationSyntax.ArgumentList.Arguments.FirstOrDefault();
                if (IsStringLiteral(firstArgument?.Expression, context.SemanticModel))
                {
                    context.ReportIssue(Rule, firstArgument);
                }
                return;
            }

            var nonCompliantParameters = methodSymbol.Parameters
                .Merge(invocationSyntax.ArgumentList.Arguments, (parameter, syntax) => new { parameter, syntax })
                .Where(x => IsLocalizableStringLiteral(x.parameter, x.syntax, context.SemanticModel));

            foreach (var nonCompliantParameter in nonCompliantParameters)
            {
                context.ReportIssue(Rule, nonCompliantParameter.syntax);
            }
        }

        private static void AnalyzeAssignments(SonarSyntaxNodeReportingContext context)
        {
            var assignmentSyntax = (AssignmentExpressionSyntax)context.Node;
            if (assignmentSyntax.IsInConditionalDebug(context.SemanticModel))
            {
                return;
            }

            var assignmentMappings = assignmentSyntax.MapAssignmentArguments();
            foreach (var assignmentMapping in assignmentMappings)
            {
                if (context.SemanticModel.GetSymbolInfo(assignmentMapping.Left).Symbol is IPropertySymbol propertySymbol
                    && IsLocalizable(propertySymbol)
                    && IsStringLiteral(assignmentMapping.Right, context.SemanticModel))
                {
                    context.ReportIssue(Rule, assignmentMapping.Right);
                }
            }
        }

        private static bool IsStringLiteral(SyntaxNode expression, SemanticModel model) =>
            expression is not null && model.GetConstantValue(expression) is { HasValue: true, Value: string _ };

        private static bool IsLocalizable(ISymbol symbol) =>
            symbol?.Name is not null
            && symbol.GetAttributes(KnownType.System_ComponentModel_LocalizableAttribute) is var localizableAttributes
            && IsLocalizable(symbol.Name, new List<AttributeData>(localizableAttributes));

        private static bool IsLocalizable(string symbolName, IReadOnlyCollection<AttributeData> localizableAttributes) =>
            localizableAttributes.Any(x => HasConstructorWitValue(x, true))
            || (symbolName.SplitCamelCaseToWords().Any(LocalizableSymbolNames.Contains)
               && (!localizableAttributes.Any(x => HasConstructorWitValue(x, false))));

        private static bool IsLocalizableStringLiteral(ISymbol symbol, ArgumentSyntax argumentSyntax, SemanticModel model) =>
            symbol is not null
            && argumentSyntax is not null
            && IsLocalizable(symbol)
            && IsStringLiteral(argumentSyntax.Expression, model);

        private static bool HasConstructorWitValue(AttributeData attribute, bool expectedValue) =>
            attribute.ConstructorArguments.Any(x => x.Value is bool boolValue && boolValue == expectedValue);
    }
}
