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
    public sealed class LiteralsShouldNotBePassedAsLocalizedParameters : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4055";
        private const string MessageFormat = "Replace this string literal with a string retrieved through an instance of the 'ResourceManager' class.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly ISet<string> LocalizableSymbolNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TEXT",
            "CAPTION",
            "MESSAGE"
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(AnalyzeInvocations, SyntaxKind.InvocationExpression);
            context.RegisterNodeAction(AnalyzeAssignments, SyntaxKind.SimpleAssignmentExpression);
        }

        private static void AnalyzeInvocations(SonarSyntaxNodeReportingContext context)
        {
            var invocationSyntax = (InvocationExpressionSyntax)context.Node;
            if (!(context.SemanticModel.GetSymbolInfo(invocationSyntax).Symbol is IMethodSymbol methodSymbol)
                || invocationSyntax.ArgumentList == null)
            {
                return;
            }

            // Calling to/from debug-only code
            if (methodSymbol.IsDiagnosticDebugMethod()
                || methodSymbol.IsConditionalDebugMethod()
                || CSharpDebugOnlyCodeHelper.IsCallerInConditionalDebug(invocationSyntax, context.SemanticModel))
            {
                return;
            }

            if (methodSymbol.IsConsoleWrite() || methodSymbol.IsConsoleWriteLine())
            {
                var firstArgument = invocationSyntax.ArgumentList.Arguments.FirstOrDefault();
                if (IsStringLiteral(firstArgument?.Expression, context.SemanticModel))
                {
                    context.ReportIssue(CreateDiagnostic(Rule, firstArgument.GetLocation()));
                }
                return;
            }

            var nonCompliantParameters = methodSymbol.Parameters
                                                     .Merge(invocationSyntax.ArgumentList.Arguments, (parameter, syntax) => new { parameter, syntax })
                                                     .Where(x => IsLocalizableStringLiteral(x.parameter, x.syntax, context.SemanticModel));

            foreach (var nonCompliantParameter in nonCompliantParameters)
            {
                context.ReportIssue(CreateDiagnostic(Rule, nonCompliantParameter.syntax.GetLocation()));
            }
        }

        private static void AnalyzeAssignments(SonarSyntaxNodeReportingContext context)
        {
            var assignmentSyntax = (AssignmentExpressionSyntax)context.Node;
            if (CSharpDebugOnlyCodeHelper.IsCallerInConditionalDebug(assignmentSyntax, context.SemanticModel))
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
                    context.ReportIssue(CreateDiagnostic(Rule, assignmentMapping.Right.GetLocation()));
                }
            }
        }

        private static bool IsStringLiteral(SyntaxNode expression, SemanticModel semanticModel) =>
            expression != null
            && semanticModel.GetConstantValue(expression) is { HasValue: true, Value: string _ };

        private static bool IsLocalizable(ISymbol symbol) =>
            symbol?.Name != null
            && symbol.GetAttributes(KnownType.System_ComponentModel_LocalizableAttribute) is var localizableAttributes
            && IsLocalizable(symbol.Name, new List<AttributeData>(localizableAttributes));

        private static bool IsLocalizable(string symbolName, IReadOnlyCollection<AttributeData> localizableAttributes) =>
            localizableAttributes.Any(x => HasConstructorWitValue(x, true))
            || (symbolName.SplitCamelCaseToWords().Any(LocalizableSymbolNames.Contains)
               && (!localizableAttributes.Any(x => HasConstructorWitValue(x, false))));

        private static bool IsLocalizableStringLiteral(ISymbol symbol, ArgumentSyntax argumentSyntax, SemanticModel semanticModel) =>
            symbol != null
            && argumentSyntax != null
            && IsLocalizable(symbol)
            && IsStringLiteral(argumentSyntax.Expression, semanticModel);

        private static bool HasConstructorWitValue(AttributeData attribute, bool expectedValue) =>
            attribute.ConstructorArguments.Any(c => c.Value is bool boolValue && boolValue == expectedValue);
    }
}
