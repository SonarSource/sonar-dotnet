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
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using System.Collections.Immutable;

namespace SonarAnalyzer.Rules
{
    public abstract class TooManyParametersBase<TSyntaxKind, TParameterListSyntax> : ParameterLoadingDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TParameterListSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S107";
        protected const string MessageFormat = "{2} has {1} parameters, which is greater than the {0} authorized.";

        protected abstract DiagnosticDescriptor Rule { get; }
        protected abstract TSyntaxKind[] SyntaxKinds { get; }
        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
        protected abstract Dictionary<TSyntaxKind, string> Mapping { get; }
        protected abstract TSyntaxKind ParentType(TParameterListSyntax parameterList);
        protected abstract int CountParameters(TParameterListSyntax parameterList);
        protected abstract bool CanBeChanged(SyntaxNode node, SemanticModel semanticModel);

        private const int DefaultValueMaximum = 7;
        [RuleParameter("max", PropertyType.Integer, "Maximum authorized number of parameters", DefaultValueMaximum)]
        public int Maximum { get; set; } = DefaultValueMaximum;

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    var parameterListNode = (TParameterListSyntax)c.Node;
                    var parametersCount = CountParameters(parameterListNode);

                    if (parametersCount > Maximum &&
                        parameterListNode.Parent != null &&
                        CanBeChanged(parameterListNode.Parent, c.SemanticModel) &&
                        Mapping.TryGetValue(ParentType(parameterListNode), out var declarationName))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, parameterListNode.GetLocation(),
                            Maximum, parametersCount, declarationName));
                    }
                },
                SyntaxKinds);
        }
    }
}
