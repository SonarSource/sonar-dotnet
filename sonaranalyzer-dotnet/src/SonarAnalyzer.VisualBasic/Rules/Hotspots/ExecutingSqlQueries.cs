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
    public sealed class ExecutingSqlQueries : ExecutingSqlQueriesBase<SyntaxKind>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager)
                .WithNotConfigurable();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(rule);

        public ExecutingSqlQueries()
            : this(AnalyzerConfiguration.Hotspot)
        {
        }

        internal /*for testing*/ ExecutingSqlQueries(IAnalyzerConfiguration analyzerConfiguration)
        {
            InvocationTracker = new VisualBasicInvocationTracker(analyzerConfiguration, rule);
            PropertyAccessTracker = new VisualBasicPropertyAccessTracker(analyzerConfiguration, rule);
            ObjectCreationTracker = new VisualBasicObjectCreationTracker(analyzerConfiguration, rule);
        }

        protected override InvocationCondition OnlyParameterIsConstantOrInterpolatedString() =>
            (context) =>
            {
                var argumentList = ((InvocationExpressionSyntax)context.Invocation).ArgumentList;
                if (argumentList == null ||
                    argumentList.Arguments.Count != 1)
                {
                    return false;
                }

                var onlyArgument = argumentList.Arguments[0].GetExpression().RemoveParentheses();

                return onlyArgument.IsAnyKind(SyntaxKind.InterpolatedStringExpression) ||
                    onlyArgument.IsConstant(context.SemanticModel);
            };

        protected override InvocationCondition ArgumentAtIndexIsConcat(int index)
        {
            throw new System.NotImplementedException();
        }

        protected override InvocationCondition ArgumentAtIndexIsFormat(int index)
        {
            throw new System.NotImplementedException();
        }

        protected override PropertyAccessCondition SetterIsConcat()
        {
            throw new System.NotImplementedException();
        }

        protected override PropertyAccessCondition SetterIsFormat()
        {
            throw new System.NotImplementedException();
        }

        protected override PropertyAccessCondition SetterIsInterpolation()
        {
            throw new System.NotImplementedException();
        }

        protected override ObjectCreationCondition FirstArgumentIsConcat()
        {
            throw new System.NotImplementedException();
        }

        protected override ObjectCreationCondition FirstArgumentIsFormat()
        {
            throw new System.NotImplementedException();
        }

        protected override ObjectCreationCondition FirstArgumentIsInterpolation()
        {
            throw new System.NotImplementedException();
        }
    }
}
