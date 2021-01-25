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
        internal static readonly ImmutableArray<KnownType> CheckedTypes =
            ImmutableArray.Create(
                KnownType.System_Array,
                KnownType.System_Collections_Generic_IList_T,
                KnownType.System_String,
                KnownType.System_Collections_IList
            );

        protected const string DiagnosticId = "S2692";
        private const string MessageFormat = "0 is a valid index, but this check ignores it.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
        protected abstract TSyntaxKind LessThanExpression { get; }
        protected abstract TSyntaxKind GreaterThanExpression { get; }

        protected abstract bool TryGetConstantIntValue(TExpressionSyntax expression, out int constValue);
        protected abstract bool IsIndexOfCall(TExpressionSyntax call, SemanticModel semanticModel);
        protected abstract TExpressionSyntax Left(TBinaryExpressionSyntax binaryExpression);
        protected abstract TExpressionSyntax Right(TBinaryExpressionSyntax binaryExpression);
        protected abstract SyntaxToken OperatorToken(TBinaryExpressionSyntax binaryExpression);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
        protected DiagnosticDescriptor Rule { get; }

        protected IndexOfCheckAgainstZeroBase(System.Resources.ResourceManager rspecResources) =>
            Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    var lessThan = (TBinaryExpressionSyntax)c.Node;
                    if (TryGetConstantIntValue(Left(lessThan), out var constValue) &&
                        constValue == 0 &&
                        IsIndexOfCall(Right(lessThan), c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule,
                            Left(lessThan).CreateLocation(OperatorToken(lessThan))));
                    }
                },
                LessThanExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    var greaterThan = (TBinaryExpressionSyntax)c.Node;
                    if (TryGetConstantIntValue(Right(greaterThan), out var constValue) &&
                        constValue == 0 &&
                        IsIndexOfCall(Left(greaterThan), c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule,
                            OperatorToken(greaterThan).CreateLocation(Right(greaterThan))));
                    }
                },
                GreaterThanExpression);
        }
    }
}
