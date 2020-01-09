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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.Wrappers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class MethodsShouldNotHaveIdenticalImplementations
        : MethodsShouldNotHaveIdenticalImplementationsBase<IMethodDeclaration, SyntaxKind>
    {
        private static readonly DiagnosticDescriptor rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => Helpers.CSharp.CSharpGeneratedCodeRecognizer.Instance;

        protected override SyntaxKind ClassDeclarationSyntaxKind => SyntaxKind.ClassDeclaration;

        protected override IEnumerable<IMethodDeclaration> GetMethodDeclarations(SyntaxNode node)
            => ((ClassDeclarationSyntax)node).GetMethodDeclarations();

        protected override bool AreDuplicates(IMethodDeclaration firstMethod, IMethodDeclaration secondMethod)
        {
            return firstMethod.Body != null &&
                secondMethod.Body != null &&
                firstMethod.Body.Statements.Count >= 2 &&
                firstMethod.Identifier.ValueText != secondMethod.Identifier.ValueText &&
                HaveSameParameters(firstMethod.ParameterList?.Parameters, secondMethod.ParameterList?.Parameters) &&
                firstMethod.Body.IsEquivalentTo(secondMethod.Body, false);

            static bool HaveSameParameters(SeparatedSyntaxList<ParameterSyntax>? leftParameters, SeparatedSyntaxList<ParameterSyntax>? rightParameters)
            {
                if (leftParameters == null && rightParameters == null)
                {
                    return true;
                }

                if (leftParameters == null ||
                    rightParameters == null ||
                    leftParameters.Value.Count != rightParameters.Value.Count)
                {
                    return false;
                }

                return leftParameters.Value
                    .Zip(rightParameters.Value, (p1, p2) => new { p1, p2 })
                    .All(tuple => tuple.p1.IsEquivalentTo(tuple.p2, false));
            }
        }

        protected override SyntaxToken GetMethodIdentifier(IMethodDeclaration method) => method.Identifier;
    }
}
