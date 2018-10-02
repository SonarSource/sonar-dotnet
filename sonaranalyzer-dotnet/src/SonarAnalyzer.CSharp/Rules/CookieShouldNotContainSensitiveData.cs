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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class CookieShouldNotContainSensitiveData : ObjectShouldBeInitializedCorrectlyBase
    {
        internal const string DiagnosticId = "S2255";
        private const string MessageFormat = "Make sure storing this data in this cookie is safe here.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager)
                .WithNotConfigurable();

        public CookieShouldNotContainSensitiveData()
            : base(new DefaultAnalyzerConfiguration())
        {
        }

        public CookieShouldNotContainSensitiveData(IAnalyzerConfiguration analysisConfiguration)
            : base(analysisConfiguration)
        {
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override string TrackedPropertyName => "Value";

        internal override KnownType TrackedType => KnownType.System_Web_HttpCookie;

        protected override bool CtorInitializesTrackedPropertyWithAllowedValue(ArgumentListSyntax argumentList, SemanticModel semanticModel) =>
            argumentList == null ||
            argumentList.Arguments.Count != 2 ||
            IsAllowedValue(argumentList.Arguments[1].Expression, semanticModel);

        protected override bool IsTrackedPropertyName(ExpressionSyntax expression) =>
            expression is ElementAccessExpressionSyntax ||
            base.IsTrackedPropertyName(expression);

        protected override bool IsPropertyOnTrackedType(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            var isElementAccess = expression is ElementAccessExpressionSyntax elementAccess &&
                elementAccess.Expression != null &&
                IsTrackedType(elementAccess.Expression, semanticModel);

            return isElementAccess
                || base.IsPropertyOnTrackedType(expression, semanticModel);
        }

        protected override bool IsAllowedValue(object constantValue) =>
            false;
    }
}
