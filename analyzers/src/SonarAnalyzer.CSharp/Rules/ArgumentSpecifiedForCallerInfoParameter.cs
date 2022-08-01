/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ArgumentSpecifiedForCallerInfoParameter : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3236";
        private const string MessageFormat = "Remove this argument from the method call; it hides the caller information.";

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<KnownType> CallerInfoAttributesToReportOn =
            ImmutableArray.Create(
                KnownType.System_Runtime_CompilerServices_CallerArgumentExpressionAttribute,
                KnownType.System_Runtime_CompilerServices_CallerFilePathAttribute,
                KnownType.System_Runtime_CompilerServices_CallerLineNumberAttribute);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                if (new CSharpMethodParameterLookup((InvocationExpressionSyntax)c.Node, c.SemanticModel) is { MethodSymbol: { } } methodParameterLookup
                    && methodParameterLookup.GetAllArgumentParameterMappings() is { } argumentMappings)
                {
                    foreach (var argumentMapping in argumentMappings.Where(x => x.Symbol.GetAttributes(CallerInfoAttributesToReportOn).Any()))
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, argumentMapping.Node.GetLocation()));
                    }
                }
            }, SyntaxKind.InvocationExpression);
    }
}
