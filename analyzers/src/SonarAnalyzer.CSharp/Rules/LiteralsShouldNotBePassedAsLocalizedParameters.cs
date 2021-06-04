/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class LiteralsShouldNotBePassedAsLocalizedParameters : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4055";
        private const string MessageFormat = "Replace this string literal with a string retrieved through an instance of the 'ResourceManager' class.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly ISet<string> LocalizableSymbolNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TEXT",
            "CAPTION",
            "MESSAGE"
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(AnalyzeInvocations, SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(AnalyzeAssignments, SyntaxKind.SimpleAssignmentExpression);
        }

        private static void AnalyzeInvocations(SyntaxNodeAnalysisContext context)
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
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, firstArgument.GetLocation()));
                }
                return;
            }

            var nonCompliantParameters = methodSymbol.Parameters
                                                     .Merge(invocationSyntax.ArgumentList.Arguments, (parameter, syntax) => new { parameter, syntax })
                                                     .Where(x => IsLocalizableStringLiteral(x.parameter, x.syntax, context.SemanticModel));

            foreach (var nonCompliantParameter in nonCompliantParameters)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, nonCompliantParameter.syntax.GetLocation()));
            }
        }

        private static void AnalyzeAssignments(SyntaxNodeAnalysisContext context)
        {
            var assignmentSyntax = (AssignmentExpressionSyntax)context.Node;
            if (context.SemanticModel.GetSymbolInfo(assignmentSyntax.Left).Symbol is IPropertySymbol propertySymbol
                && IsLocalizable(propertySymbol)
                && IsStringLiteral(assignmentSyntax.Right, context.SemanticModel)
                && !CSharpDebugOnlyCodeHelper.IsCallerInConditionalDebug(assignmentSyntax, context.SemanticModel))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, assignmentSyntax.GetLocation()));
            }
        }

        private static bool IsStringLiteral(SyntaxNode expression, SemanticModel semanticModel) =>
            expression != null
            && semanticModel.GetConstantValue(expression) is { HasValue: true, Value: string _ };

        private static bool IsLocalizable(ISymbol symbol) =>
            symbol?.Name != null
            && symbol.GetAttributes(KnownType.System_ComponentModel_LocalizableAttribute) is var localizableAttributes
            && IsLocalizable(symbol.Name, new List<AttributeData>(localizableAttributes).AsReadOnly());

        private static bool IsLocalizable(string symbolName, IReadOnlyCollection<AttributeData> localizableAttributes) =>
            localizableAttributes.Any(x => HasConstructorWithBoolValue(x, true))
            || (symbolName.SplitCamelCaseToWords().Any(LocalizableSymbolNames.Contains)
               && (!localizableAttributes.Any(x => HasConstructorWithBoolValue(x, false))));

        private static bool IsLocalizableStringLiteral(IParameterSymbol parameter, ArgumentSyntax argumentSyntax, SemanticModel semanticModel) =>
            parameter != null
            && argumentSyntax != null
            && IsLocalizable(parameter)
            && IsStringLiteral(argumentSyntax.Expression, semanticModel);

        private static bool HasConstructorWithBoolValue(AttributeData attribute, bool expectedValue) =>
            attribute.ConstructorArguments.Any(c => c.Value is bool boolValue && boolValue == expectedValue);
    }
}
