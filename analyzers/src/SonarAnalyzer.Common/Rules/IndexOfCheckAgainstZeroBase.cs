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
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class IndexOfCheckAgainstZeroBase<TSyntaxKind, TExpressionSyntax, TBinaryExpressionSyntax> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TExpressionSyntax : SyntaxNode
        where TBinaryExpressionSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S2692";
        private const string MessageFormat = "0 is a valid index, but this check ignores it.";

        private protected static readonly string[] InvalidStringMethods =
            {
                "IndexOfAny",
                "LastIndexOf",
                "LastIndexOfAny"
            };

        private protected static readonly ImmutableArray<KnownType> CheckedTypes =
            ImmutableArray.Create(
                KnownType.System_Array,
                KnownType.System_Collections_Generic_IList_T,
                KnownType.System_String,
                KnownType.System_Collections_IList
            );

        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade LanguageFacade { get; }
        protected abstract TSyntaxKind LessThanExpression { get; }
        protected abstract TSyntaxKind GreaterThanExpression { get; }

        protected abstract bool TryGetConstantIntValue(TExpressionSyntax expression, out int constValue);
        protected abstract bool IsSensitiveCall(TExpressionSyntax call, SemanticModel semanticModel);
        protected abstract TExpressionSyntax Left(TBinaryExpressionSyntax binaryExpression);
        protected abstract TExpressionSyntax Right(TBinaryExpressionSyntax binaryExpression);
        protected abstract SyntaxToken OperatorToken(TBinaryExpressionSyntax binaryExpression);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected IndexOfCheckAgainstZeroBase(System.Resources.ResourceManager rspecResources) =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                LanguageFacade.GeneratedCodeRecognizer,
                c =>
                {
                    var lessThan = (TBinaryExpressionSyntax)c.Node;
                    if (IsInvalidComparision(Left(lessThan), Right(lessThan), c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, Left(lessThan).CreateLocation(OperatorToken(lessThan))));
                    }
                },
                LessThanExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                LanguageFacade.GeneratedCodeRecognizer,
                c =>
                {
                    var greaterThan = (TBinaryExpressionSyntax)c.Node;
                    if (IsInvalidComparision(Right(greaterThan), Left(greaterThan), c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, OperatorToken(greaterThan).CreateLocation(Right(greaterThan))));
                    }
                },
                GreaterThanExpression);
        }

        private bool IsInvalidComparision(TExpressionSyntax conxtantExpression, TExpressionSyntax methodInvocationExpression, SemanticModel semanticModel) =>
            TryGetConstantIntValue(conxtantExpression, out var constValue)
            && constValue == 0
            && IsSensitiveCall(methodInvocationExpression, semanticModel);
    }
}
