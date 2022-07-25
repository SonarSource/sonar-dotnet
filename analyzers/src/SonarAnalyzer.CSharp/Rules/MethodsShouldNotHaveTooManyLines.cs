/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MethodsShouldNotHaveTooManyLines
        : MethodsShouldNotHaveTooManyLinesBase<SyntaxKind, BaseMethodDeclarationSyntax>
    {
        private const string LocalFunctionMessageFormat = "{0} local function has {1} lines, which is greater than the {2} lines authorized.";

        private static readonly DiagnosticDescriptor DefaultRule = DescriptorFactory.Create(DiagnosticId, MessageFormat, false);
        private static readonly DiagnosticDescriptor LocalFunctionRule = DescriptorFactory.Create(DiagnosticId, LocalFunctionMessageFormat, false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(DefaultRule, LocalFunctionRule);

        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer =>
            CSharpGeneratedCodeRecognizer.Instance;

        protected override SyntaxKind[] SyntaxKinds { get; } =
            {
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DestructorDeclaration
            };

        protected override string MethodKeyword => "methods";

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    if ((c.Node.IsInTopLevelMain()
                         && !c.Node.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().Any())
                        || ((LocalFunctionStatementSyntaxWrapper)c.Node).Modifiers.Any(SyntaxKind.StaticKeyword))
                    {
                        var wrapper = (LocalFunctionStatementSyntaxWrapper)c.Node;
                        var linesCount = CountLines(wrapper);
                        if (linesCount > Max)
                        {
                            c.ReportIssue(
                                Diagnostic.Create(
                                    LocalFunctionRule,
                                    wrapper.Identifier.GetLocation(),
                                    wrapper.Modifiers.Any(SyntaxKind.StaticKeyword) ? "This static" : "This",
                                    linesCount,
                                    Max,
                                    MethodKeyword));
                        }
                    }
                },
                SyntaxKindEx.LocalFunctionStatement);

            base.Initialize(context);
        }

        protected override IEnumerable<SyntaxToken> GetMethodTokens(BaseMethodDeclarationSyntax baseMethodDeclaration) =>
            baseMethodDeclaration.ExpressionBody()?.Expression?.DescendantTokens()
                ?? baseMethodDeclaration.Body?.Statements.Where(s => !IsStaticLocalFunction(s)).SelectMany(s => s.DescendantTokens())
                ?? Enumerable.Empty<SyntaxToken>();

        protected override SyntaxToken? GetMethodIdentifierToken(BaseMethodDeclarationSyntax baseMethodDeclaration) =>
            baseMethodDeclaration.GetIdentifierOrDefault();

        protected override string GetMethodKindAndName(SyntaxToken identifierToken)
        {
            var identifierName = identifierToken.ValueText;
            if (string.IsNullOrEmpty(identifierName))
            {
                return "method";
            }

            var declaration = identifierToken.Parent;
            if (declaration.IsKind(SyntaxKind.ConstructorDeclaration))
            {
                return $"constructor '{identifierName}'";
            }

            if (declaration.IsKind(SyntaxKind.DestructorDeclaration))
            {
                return $"finalizer '~{identifierName}'";
            }

            if (declaration is MethodDeclarationSyntax)
            {
                return $"method '{identifierName}'";
            }

            return "method";
        }

        private static IEnumerable<SyntaxToken> GetMethodTokens(LocalFunctionStatementSyntaxWrapper wrapper) =>
            wrapper.ExpressionBody?.Expression.DescendantTokens()
            ?? wrapper.Body.Statements.SelectMany(s => s.DescendantTokens());

        private static long CountLines(LocalFunctionStatementSyntaxWrapper wrapper) =>
            GetMethodTokens(wrapper).SelectMany(x => x.GetLineNumbers())
                                    .Distinct()
                                    .LongCount();

        private static bool IsStaticLocalFunction(SyntaxNode node) =>
            node.IsKind(SyntaxKindEx.LocalFunctionStatement)
            && ((LocalFunctionStatementSyntaxWrapper)node).Modifiers.Any(SyntaxKind.StaticKeyword);
    }
}
