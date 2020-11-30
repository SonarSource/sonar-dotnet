/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class EncryptionAlgorithmsShouldBeSecure : EncryptionAlgorithmsShouldBeSecureBase<SyntaxKind>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        public EncryptionAlgorithmsShouldBeSecure()
        {
            InvocationTracker = new VisualBasicInvocationTracker(AnalyzerConfiguration.AlwaysEnabled, rule);
            PropertyAccessTracker = new VisualBasicPropertyAccessTracker(AnalyzerConfiguration.AlwaysEnabled, rule);
            ObjectCreationTracker = new VisualBasicObjectCreationTracker(AnalyzerConfiguration.AlwaysEnabled, rule);
        }

        protected override PropertyAccessCondition IsInsideObjectInitializer() =>
            (context) =>
                context.Expression.FirstAncestorOrSelf<ObjectMemberInitializerSyntax>() != null;

        protected override InvocationCondition HasPkcs1PaddingArgument() =>
            (context) =>
            {
                var argumentList = ((InvocationExpressionSyntax)context.Invocation).ArgumentList;
                var values = VisualBasicSyntaxHelper.ArgumentValuesForParameter(context.SemanticModel, argumentList, "padding");
                return values.Length == 1 &&
                    values[0] is ExpressionSyntax valueSyntax &&
                    context.SemanticModel.GetSymbolInfo(valueSyntax).Symbol is ISymbol symbol &&
                    symbol.Name == "Pkcs1";
            };
    }
}
