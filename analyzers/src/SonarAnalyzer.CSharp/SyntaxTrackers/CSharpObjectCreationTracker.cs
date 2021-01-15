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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.Helpers
{
    public class CSharpObjectCreationTracker : ObjectCreationTracker<SyntaxKind>
    {
        protected override SyntaxKind[] TrackedSyntaxKinds { get; } = { SyntaxKind.ObjectCreationExpression };
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer { get; } = CSharpGeneratedCodeRecognizer.Instance;

        public CSharpObjectCreationTracker(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule) : base(analyzerConfiguration, rule) { }

        internal override Condition ArgumentAtIndexIsConst(int index) =>
            context => ((ObjectCreationExpressionSyntax)context.Node).ArgumentList is { } argumentList
                       && argumentList.Arguments.Count > index
                       && argumentList.Arguments[index].Expression.HasConstantValue(context.SemanticModel);

        internal override object ConstArgumentForParameter(ObjectCreationContext context, string parameterName)
        {
            var argumentList = ((ObjectCreationExpressionSyntax)context.Node).ArgumentList;
            var values = CSharpSyntaxHelper.ArgumentValuesForParameter(context.SemanticModel, argumentList, parameterName);
            return values.Length == 1 && values[0] is ExpressionSyntax valueSyntax
                ? valueSyntax.FindConstantValue(context.SemanticModel)
                : null;
        }
    }
}
