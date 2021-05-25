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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(S2278DiagnosticId)]
    [Rule(DiagnosticId)]
    public sealed class InsecureEncryptionAlgorithm : InsecureEncryptionAlgorithmBase<SyntaxKind, InvocationExpressionSyntax, ArgumentListSyntax, ArgumentSyntax>
    {
        // S2278 was deprecated in favor of S5547. Technically, there is no difference in the C# analyzer between
        // the 2 rules, but to be coherent with all the other languages, we still replace it with the new one
        private const string S2278DiagnosticId = "S2278";
        private const string S2278MessageFormat = "Use the recommended AES (Advanced Encryption Standard) instead.";

        private static readonly DiagnosticDescriptor S2278 = DiagnosticDescriptorBuilder.GetDescriptor(S2278DiagnosticId, S2278MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(S2278, Rule);

        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;
        protected override SyntaxKind[] ObjectCreationExpressionKinds => new[] { SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression };

        protected override ArgumentListSyntax ArgumentList(InvocationExpressionSyntax invocationExpression) =>
            invocationExpression.ArgumentList;

        protected override SeparatedSyntaxList<ArgumentSyntax> Arguments(ArgumentListSyntax argumentList) =>
            argumentList.Arguments;

        protected override bool IsStringLiteralArgument(ArgumentSyntax argument) =>
            argument.Expression.IsKind(SyntaxKind.StringLiteralExpression);

        protected override string StringLiteralValue(ArgumentSyntax argument) =>
            ((LiteralExpressionSyntax)argument.Expression).Token.ValueText;

        protected override Location Location(SyntaxNode objectCreation) =>
            objectCreation is ObjectCreationExpressionSyntax objectCreationExpression
                ? objectCreationExpression.Type.GetLocation()
                : ((ImplicitObjectCreationExpressionSyntaxWrapper)objectCreation).SyntaxNode.GetLocation();
    }
}
