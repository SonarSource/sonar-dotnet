/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
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
    public sealed class MagicNumberShouldNotBeUsed : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S109";
        private const string MessageFormat = "Assign this magic number '{0}' to a well-named (variable|constant), " +
            "and use the (variable|constant) instead.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ISet<string> NotConsideredAsMagicNumbers = new HashSet<string> { "-1", "0", "1" };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var literalExpression = (LiteralExpressionSyntax)c.Node;

                    if (!IsExceptionToTheRule(literalExpression))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, literalExpression.GetLocation(),
                            literalExpression.Token.ValueText));
                    }
                },
                SyntaxKind.NumericLiteralExpression);
        }

        private bool IsExceptionToTheRule(LiteralExpressionSyntax literalExpression) =>
            NotConsideredAsMagicNumbers.Contains(literalExpression.Token.ValueText) ||
            // It's ok to use magic numbers as part of a variable declaration
            literalExpression.FirstAncestorOrSelf<VariableDeclarationSyntax>() != null ||
            // It's ok to use magic numbers as part of a parameter declaration
            literalExpression.FirstAncestorOrSelf<ParameterSyntax>() != null ||
            // It's ok to use magic numbers as part of an enum declaration
            literalExpression.FirstAncestorOrSelf<EnumMemberDeclarationSyntax>() != null ||
            // It's ok to use magic numbers in the GetHashCode method. Note that I am only checking the method name of the sake of simplicity
            literalExpression.FirstAncestorOrSelf<MethodDeclarationSyntax>()?.Identifier.ValueText == nameof(object.GetHashCode) ||
            // It's ok to use magic numbers in pragma directives
            literalExpression.FirstAncestorOrSelf<PragmaWarningDirectiveTriviaSyntax>() != null;
    }
}
