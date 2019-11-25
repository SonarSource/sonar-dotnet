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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ParameterAssignedToBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S1226";
        protected const string MessageFormat = "Introduce a new variable instead of reusing the parameter '{0}'.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
    }

    public abstract class ParameterAssignedToBase<TLanguageKindEnum, TAssignmentStatementSyntax> : ParameterAssignedToBase
        where TLanguageKindEnum : struct
        where TAssignmentStatementSyntax : SyntaxNode
    {
        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    var assignment = (TAssignmentStatementSyntax)c.Node;
                    var left = GetAssignedNode(assignment);
                    var symbol = c.SemanticModel.GetSymbolInfo(left).Symbol;

                    if (symbol != null
                        && (IsAssignmentToParameter(symbol) || IsAssignmentToCatchVariable(symbol, left))
                        && (!IsReadBefore(c.SemanticModel, symbol, assignment)))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], left.GetLocation(), left.ToString()));
                    }
                },
                SyntaxKindsOfInterest.ToArray());
        }


        protected abstract bool IsAssignmentToCatchVariable(ISymbol symbol, SyntaxNode node);

        protected abstract bool IsAssignmentToParameter(ISymbol symbol);

        protected abstract bool IsReadBefore(SemanticModel semanticModel, ISymbol parameterSymbol, TAssignmentStatementSyntax assignment);

        protected abstract SyntaxNode GetAssignedNode(TAssignmentStatementSyntax assignment);

        public abstract ImmutableArray<TLanguageKindEnum> SyntaxKindsOfInterest { get; }
    }
}
