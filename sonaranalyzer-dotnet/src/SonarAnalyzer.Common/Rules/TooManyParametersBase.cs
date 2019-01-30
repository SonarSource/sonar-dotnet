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

using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class TooManyParametersBase<TSyntaxKind, TParameterListSyntax> : ParameterLoadingDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TParameterListSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S107";
        protected const string MessageFormat = "{0} has {1} parameters, which is greater than the {2} authorized.";

        protected abstract TSyntaxKind[] SyntaxKinds { get; }
        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

        protected abstract string UserFriendlyNameForNode(SyntaxNode node);

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
                        CanBeChanged(parameterListNode.Parent, c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], parameterListNode.GetLocation(),
                            UserFriendlyNameForNode(parameterListNode.Parent), parametersCount, Maximum));
                    }
                },
                SyntaxKinds);
        }

        protected bool VerifyCanBeChangedBySymbol(SyntaxNode node, SemanticModel semanticModel)
        {
            var declaredSymbol = semanticModel.GetDeclaredSymbol(node);
            var symbol = semanticModel.GetSymbolInfo(node).Symbol;
            if (declaredSymbol == null && symbol == null)
            {
                // no information
                return false;
            }

            if (symbol != null)
            {
                // Not a declaration, such as Action
                return true;
            }

            if (declaredSymbol.IsExtern &&
                declaredSymbol.IsStatic &&
                declaredSymbol.GetAttributes(KnownType.System_Runtime_InteropServices_DllImportAttribute).Any())
            {
                // P/Invoke method is defined externally.
                // Do not raise
                return false;
            }

            return declaredSymbol.GetOverriddenMember() == null &&
                   declaredSymbol.GetInterfaceMember() == null;
        }
    }
}
