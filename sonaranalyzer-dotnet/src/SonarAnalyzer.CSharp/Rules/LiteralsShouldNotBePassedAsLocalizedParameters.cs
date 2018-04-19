/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
        internal const string DiagnosticId = "S4055";
        private const string MessageFormat = "Replace this string literal with a string retrieved through an instance of the 'ResourceManager' class.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<string> localizableSymbolNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "text",
            "caption",
            "message"
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var invocationSyntax = (InvocationExpressionSyntax)c.Node;
                    var methodSymbol = c.SemanticModel.GetSymbolInfo(invocationSyntax).Symbol as IMethodSymbol;
                    if (methodSymbol == null ||
                        invocationSyntax.ArgumentList == null)
                    {
                        return;
                    }

                    if (methodSymbol.IsConsoleWrite() || methodSymbol.IsConsoleWriteLine())
                    {
                        var firstArgument = invocationSyntax.ArgumentList.Arguments.FirstOrDefault();
                        if (IsStringLiteral(firstArgument?.Expression, c.SemanticModel))
                        {
                            c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, firstArgument.GetLocation()));
                        }
                    }
                    else
                    {
                        methodSymbol.Parameters
                            .Merge(invocationSyntax.ArgumentList.Arguments, (parameter, syntax) => new { parameter, syntax })
                            .Where(x => x.parameter != null && x.syntax != null)
                            .Where(x => IsLocalizable(x.parameter))
                            .Where(x => IsStringLiteral(x.syntax.Expression, c.SemanticModel))
                            .ToList()
                            .ForEach(x =>
                            {
                                c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, x.syntax.GetLocation()));
                            });
                    }
                },
                SyntaxKind.InvocationExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var assignmentSyntax = (AssignmentExpressionSyntax)c.Node;
                    var propertySymbol = c.SemanticModel.GetSymbolInfo(assignmentSyntax.Left).Symbol as IPropertySymbol;

                    if (IsLocalizable(propertySymbol) &&
                        IsStringLiteral(assignmentSyntax.Right, c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, assignmentSyntax.GetLocation()));
                    }
                },
                SyntaxKind.SimpleAssignmentExpression);
        }

        private static bool IsStringLiteral(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            if (expression == null)
            {
                return false;
            }

            var constant = semanticModel.GetConstantValue(expression);
            return constant.HasValue && constant.Value is string;
        }

        private static bool IsLocalizable(ISymbol symbol)
        {
            if (symbol?.Name == null)
            {
                return false;
            }

            return symbol.Name.SplitCamelCaseToWords().Any(localizableSymbolNames.Contains) ||
                symbol.GetAttributes().Any(IsLocalizableAttribute);
        }

        private static bool IsLocalizableAttribute(AttributeData attributeData)
        {
            return attributeData.AttributeClass.Is(KnownType.System_ComponentModel_LocalizableAttribute) &&
                attributeData.ConstructorArguments.Any(c => (c.Value as bool?) ?? false);
        }
    }
}
