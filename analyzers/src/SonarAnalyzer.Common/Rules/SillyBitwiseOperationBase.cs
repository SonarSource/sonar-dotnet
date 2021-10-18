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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class SillyBitwiseOperationBase : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2437";
        internal const string IsReportingOnLeftKey = "IsReportingOnLeft";
        private const string MessageFormat = "Remove this silly bit operation.";

        protected abstract object FindConstant(SemanticModel semanticModel, SyntaxNode node);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
        protected DiagnosticDescriptor Rule { get; }

        protected SillyBitwiseOperationBase(System.Resources.ResourceManager rspecResources) =>
            Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources, fadeOutCode: true);

        protected void CheckBinary(SyntaxNodeAnalysisContext context, SyntaxNode left, SyntaxToken @operator, SyntaxNode right, int constValueToLookFor)
        {
            Location location;
            bool isReportingOnLeftKey;
            if (FindIntConstant(context.SemanticModel, left) is { } valueLeft && valueLeft == constValueToLookFor)
            {
                location = left.CreateLocation(@operator);
                isReportingOnLeftKey = true;
            }
            else if (FindIntConstant(context.SemanticModel, right) is { } valueRight && valueRight == constValueToLookFor)
            {
                location = @operator.CreateLocation(right);
                isReportingOnLeftKey = false;
            }
            else
            {
                return;
            }

            context.ReportIssue(Diagnostic.Create(Rule,
                                                                 location,
                                                                 ImmutableDictionary<string, string>.Empty.Add(IsReportingOnLeftKey, isReportingOnLeftKey.ToString())));
        }

        protected int? FindIntConstant(SemanticModel semanticModel, SyntaxNode node) =>
            FindConstant(semanticModel, node) is { } value
            && !IsEnum(semanticModel, node)
                ? ConversionHelper.TryConvertToInt(value)
                : null;

        private static bool IsEnum(SemanticModel semanticModel, SyntaxNode node) =>
            semanticModel.GetSymbolInfo(node).Symbol.GetSymbolType() is INamedTypeSymbol {EnumUnderlyingType: { }};
    }
}
