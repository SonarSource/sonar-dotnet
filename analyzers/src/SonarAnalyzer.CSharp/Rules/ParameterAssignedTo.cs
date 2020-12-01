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
    public sealed class ParameterAssignedTo : ParameterAssignedToBase<SyntaxKind, AssignmentExpressionSyntax, IdentifierNameSyntax>
    {

        public ParameterAssignedTo() : base(RspecStrings.ResourceManager) { }

        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => Helpers.CSharp.CSharpGeneratedCodeRecognizer.Instance;

        protected override SyntaxKind SyntaxKindOfInterest => SyntaxKind.SimpleAssignmentExpression;

        protected override SyntaxNode AssignmentLeft(AssignmentExpressionSyntax assignment) => assignment.Left;

        protected override SyntaxNode AssignmentRight(AssignmentExpressionSyntax assignment) => assignment.Right;

        protected override bool IsAssignmentToCatchVariable(ISymbol symbol, SyntaxNode node)
        {
            if (!(symbol is ILocalSymbol localSymbol))
            {
                return false;
            }

            var result = localSymbol.DeclaringSyntaxReferences
                .Select(declaringSyntaxReference => declaringSyntaxReference.GetSyntax())
                .Any(syntaxNode =>
                    syntaxNode.Parent is CatchClauseSyntax &&
                    ((CatchClauseSyntax)syntaxNode.Parent).Declaration == syntaxNode);

            return result;
        }

        protected override bool IsAssignmentToParameter(ISymbol symbol)
        {
            var parameterSymbol = symbol as IParameterSymbol;
            var result = parameterSymbol?.RefKind == RefKind.None;
            return result;
        }


    }
}
