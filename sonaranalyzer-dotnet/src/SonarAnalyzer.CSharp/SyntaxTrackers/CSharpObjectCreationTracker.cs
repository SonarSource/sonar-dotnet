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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers
{
    public class CSharpObjectCreationTracker : ObjectCreationTracker<SyntaxKind>
    {
        public CSharpObjectCreationTracker(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule)
            : base(analyzerConfiguration, rule)
        {
        }

        protected override SyntaxKind[] TrackedSyntaxKinds { get; } =
            new[] { SyntaxKind.ObjectCreationExpression };

        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer { get; } =
            CSharp.CSharpGeneratedCodeRecognizer.Instance;

        internal override ObjectCreationCondition ArgumentAtIndexIsConst(int index) =>
            (context) =>
            {
                var argumentList = ((ObjectCreationExpressionSyntax)context.Expression).ArgumentList;
                return argumentList != null &&
                    argumentList.Arguments.Count > index &&
                    argumentList.Arguments[index].Expression.IsConstant(context.SemanticModel);
            };
    }
}
