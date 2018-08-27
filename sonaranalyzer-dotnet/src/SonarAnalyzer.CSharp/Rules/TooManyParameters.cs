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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class TooManyParameters : ParameterLoadingDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S107";
        private const string MessageFormat = "{2} has {1} parameters, which is greater than the {0} authorized.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager,
                isEnabledByDefault: false);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private const int DefaultValueMaximum = 7;
        [RuleParameter("max", PropertyType.Integer, "Maximum authorized number of parameters", DefaultValueMaximum)]
        public int Maximum { get; set; } = DefaultValueMaximum;

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var parameterListNode = (ParameterListSyntax)c.Node;
                    var parameters = parameterListNode.Parameters.Count;

                    if (parameters > Maximum &&
                        parameterListNode.Parent != null &&
                        CanBeChanged(parameterListNode.Parent, c.SemanticModel) &&
                        Mapping.TryGetValue(parameterListNode.Parent.Kind(), out var declarationName))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, parameterListNode.GetLocation(),
                            Maximum, parameters, declarationName));
                    }
                },
                SyntaxKind.ParameterList);
        }

        private bool CanBeChanged(SyntaxNode node, SemanticModel semanticModel)
        {
            var declaredSymbol = semanticModel.GetDeclaredSymbol(node);
            var symbol = semanticModel.GetSymbolInfo(node).Symbol;

            if (declaredSymbol == null && symbol == null)
            {
                // No information
                return false;
            }

            if (symbol != null)
            {
                // Not a declaration, such as Action
                return true;
            }

            if ((node as ConstructorDeclarationSyntax)?.Initializer?.ArgumentList?.Arguments.Count > Maximum)
            {
                // Base class is already not compliant so let's ignore current constructor.
                // Another option could be to substract current number of parameters from base count and raise only if greater
                // than threshold.
                return false;
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

        private static readonly Dictionary<SyntaxKind, string> Mapping = new Dictionary<SyntaxKind, string>
        {
            { SyntaxKind.ConstructorDeclaration, "Constructor" },
            { SyntaxKind.MethodDeclaration, "Method" },
            { SyntaxKind.DelegateDeclaration, "Delegate" },
            { SyntaxKind.AnonymousMethodExpression, "Delegate" },
            { SyntaxKind.ParenthesizedLambdaExpression, "Lambda" },
            { SyntaxKind.SimpleLambdaExpression, "Lambda" }
        };
    }
}
