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
    public sealed class DeliveringDebugFeaturesInProduction : DeliveringDebugFeaturesInProductionBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        public DeliveringDebugFeaturesInProduction() : this(AnalyzerConfiguration.Hotspot) { }

        internal /*for testing*/ DeliveringDebugFeaturesInProduction(IAnalyzerConfiguration configuration) : base(configuration) { }

        protected override bool IsInvokedConditionally(SyntaxNode node, SemanticModel semanticModel) =>
            node.FirstAncestorOrSelf<StatementSyntax>() is { } invocationStatement
            && invocationStatement.Ancestors().Any(x => IsDevelopmentCheck(x, semanticModel));

        protected override bool IsInDevelopmentContext(SyntaxNode node) =>
            node.Ancestors()
                .OfType<ClassDeclarationSyntax>()
                .Any(x => x.Identifier.Text == StartupDevelopment);

        private bool IsDevelopmentCheck(SyntaxNode node, SemanticModel semanticModel) =>
            node is IfStatementSyntax ifStatement
            && ifStatement.Condition.RemoveParentheses() is InvocationExpressionSyntax condition
            && IsValidationMethod(semanticModel, condition, condition.Expression.GetIdentifier()?.Identifier.ValueText);
    }
}
