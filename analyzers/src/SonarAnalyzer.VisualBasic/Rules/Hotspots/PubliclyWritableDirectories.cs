﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class PubliclyWritableDirectories : PubliclyWritableDirectoriesBase<SyntaxKind, InvocationExpressionSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        public PubliclyWritableDirectories() : this(AnalyzerConfiguration.Hotspot) { }

        internal PubliclyWritableDirectories(IAnalyzerConfiguration configuration) : base(configuration) { }

        private protected override bool IsGetTempPathAssignment(InvocationExpressionSyntax invocationExpression, KnownType type, string methodName, SemanticModel semanticModel) =>
            invocationExpression.IsMethodInvocation(type, methodName, semanticModel)
            && invocationExpression.Parent.IsAnyKind(SyntaxKind.EqualsValue, SyntaxKind.SimpleAssignmentStatement, SyntaxKind.ReturnStatement);

        private protected override bool IsInsecureEnvironmentVariableRetrieval(InvocationExpressionSyntax invocation, KnownType type, string methodName, SemanticModel semanticModel) =>
            invocation.IsMethodInvocation(type, methodName, semanticModel)
            && invocation.ArgumentList?.Arguments.First() is var firstArgument
            && InsecureEnvironmentVariables.Any(x => x.Equals(firstArgument.GetExpression().StringValue(semanticModel), StringComparison.OrdinalIgnoreCase));
    }
}
