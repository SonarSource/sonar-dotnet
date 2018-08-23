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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.Common
{
    public abstract class UseShortCircuitingOperatorBase : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2178";
        internal const string MessageFormat = "Correct this '{0}' to '{1}'.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

        protected static bool IsBool(SyntaxNode node, SemanticModel semanticModel)
        {
            if (node == null)
            {
                return false;
            }

            var type = semanticModel.GetTypeInfo(node).Type;
            return type.Is(KnownType.System_Boolean);
        }
    }

    public abstract class UseShortCircuitingOperatorBase<TLanguageKindEnum, TBinaryExpression> : UseShortCircuitingOperatorBase
        where TLanguageKindEnum : struct
        where TBinaryExpression : SyntaxNode
    {
        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    var node = (TBinaryExpression)c.Node;

                    if (GetOperands(node).All(o => IsBool(o, c.SemanticModel)))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, GetOperator(node).GetLocation(),
                            GetCurrentOpName(node), GetSuggestedOpName(node)));
                    }
                },
                SyntaxKindsOfInterest.ToArray());
        }

        protected abstract string GetSuggestedOpName(TBinaryExpression node);

        protected abstract string GetCurrentOpName(TBinaryExpression node);

        protected abstract IEnumerable<SyntaxNode> GetOperands(TBinaryExpression expression);

        protected abstract SyntaxToken GetOperator(TBinaryExpression expression);

        protected abstract ImmutableArray<TLanguageKindEnum> SyntaxKindsOfInterest { get; }

        protected abstract DiagnosticDescriptor Rule { get; }

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    }
}
