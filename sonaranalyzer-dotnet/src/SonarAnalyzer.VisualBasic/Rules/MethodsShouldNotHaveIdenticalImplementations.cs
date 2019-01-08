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
using System.Linq;
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
    public sealed class MethodsShouldNotHaveIdenticalImplementations
        : MethodsShouldNotHaveIdenticalImplementationsBase<MethodBlockSyntax, SyntaxKind>
    {
        private static readonly DiagnosticDescriptor rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);
        protected override Helpers.GeneratedCodeRecognizer GeneratedCodeRecognizer => Helpers.VisualBasic.VisualBasicGeneratedCodeRecognizer.Instance;

        protected override SyntaxKind ClassDeclarationSyntaxKind => SyntaxKind.ClassBlock;

        protected override IEnumerable<MethodBlockSyntax> GetMethodDeclarations(SyntaxNode node)
        {
            var classDeclaration = (ClassBlockSyntax)node;
            return classDeclaration.Members.OfType<MethodBlockSyntax>().ToList();
        }

        protected override bool AreDuplicates(MethodBlockSyntax firstMethod, MethodBlockSyntax secondMethod)
        {
            return firstMethod.Statements.Count >= 2 &&
                firstMethod.SubOrFunctionStatement.Identifier.ValueText != secondMethod.SubOrFunctionStatement.Identifier.ValueText &&
                HaveSameParameters(firstMethod?.BlockStatement?.ParameterList.Parameters, secondMethod?.BlockStatement?.ParameterList.Parameters) &&
                VisualBasicEquivalenceChecker.AreEquivalent(firstMethod.Statements, secondMethod.Statements);

            bool HaveSameParameters(SeparatedSyntaxList<ParameterSyntax>? leftParameters, SeparatedSyntaxList<ParameterSyntax>? rightParameters)
            {
                if ((leftParameters == null && rightParameters != null) ||
                    (leftParameters != null && rightParameters == null) ||
                    leftParameters.Value.Count != rightParameters.Value.Count)
                {
                    return false;
                }

                return leftParameters.Value.Zip(rightParameters.Value, (p1, p2) => new { p1, p2 })
                    .All(tuple => tuple.p1.IsEquivalentTo(tuple.p2, false));
            }
        }

        protected override SyntaxToken GetMethodIdentifier(MethodBlockSyntax method) => method.SubOrFunctionStatement.Identifier;
    }
}
