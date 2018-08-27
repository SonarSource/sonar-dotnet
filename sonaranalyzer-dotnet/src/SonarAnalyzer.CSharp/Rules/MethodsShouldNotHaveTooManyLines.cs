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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class MethodsShouldNotHaveTooManyLines : ParameterLoadingDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S138";
        private const string MessageFormat = "This {0} has {1} lines, which is greater than the {2} lines authorized. Split it into smaller methods.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager,
                isEnabledByDefault: false);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private const int DefaultMaxMethodLines = 80;

        [RuleParameter("max", PropertyType.Integer, "Maximum authorized lines of code in a method", DefaultMaxMethodLines)]
        public int Max { get; set; } = DefaultMaxMethodLines;

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    if (Max < 2)
                    {
                        throw new ArgumentOutOfRangeException(
                            $"Invalid rule parameter: maximum number of lines = {Max}. Must be at least 2.");
                    }

                    var baseMethodSyntax = (BaseMethodDeclarationSyntax)c.Node;

                    var identifierLocation = baseMethodSyntax?.FindIdentifierLocation();
                    if (identifierLocation == null)
                    {
                        return;
                    }

                    var linesCount = GetBodyTokens(baseMethodSyntax)
                        .SelectMany(token => token.GetLineNumbers())
                        .Distinct()
                        .LongCount();

                    if (linesCount > Max)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, identifierLocation,
                            GetDescription(baseMethodSyntax), linesCount, Max));
                    }
                },
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DestructorDeclaration);
        }

        private static IEnumerable<SyntaxToken> GetBodyTokens(BaseMethodDeclarationSyntax baseMethodSyntax) =>
            baseMethodSyntax?.ExpressionBody()?.Expression?.DescendantTokens()
            ?? baseMethodSyntax?.Body?.Statements.SelectMany(s => s.DescendantTokens())
            ?? Enumerable.Empty<SyntaxToken>();

        private static string GetDescription(BaseMethodDeclarationSyntax baseMethodDeclaration)
        {
            var identifierName = baseMethodDeclaration.GetIdentifierOrDefault()?.ValueText;
            if (identifierName == null)
            {
                return "method";
            }

            if (baseMethodDeclaration is ConstructorDeclarationSyntax)
            {
                return $"constructor '{identifierName}'";
            }

            if (baseMethodDeclaration is DestructorDeclarationSyntax)
            {
                return $"finalizer '~{identifierName}'";
            }

            if (baseMethodDeclaration is MethodDeclarationSyntax)
            {
                return $"method '{identifierName}'";
            }

            return "method";
        }

    }
}
