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
    public sealed class NumberPatternShouldBeRegular : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3937";
        private const string MessageFormat = "Review this number; its irregular pattern indicates an error.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    if ((c.Compilation as CSharpCompilation)?.LanguageVersion.CompareTo(LanguageVersionEx.CSharp7) < 0)
                    {
                        return;
                    }

                    var literal = (LiteralExpressionSyntax)c.Node;

                    var parts = literal.Token.Text.Split('_');

                    var previousPartLength = -1;
                    for (var i = 1; i < parts.Length; i++)
                    {
                        var currentPartLength = GetCurrentPartLength(parts[i], i == parts.Length - 1);
                        if (previousPartLength == -1)
                        {
                            previousPartLength = currentPartLength;
                        }
                        else if (currentPartLength != previousPartLength)
                        {
                            c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, literal.GetLocation()));
                            return;
                        }
                        else
                        {
                            // continue
                        }
                    }
                },
                SyntaxKind.NumericLiteralExpression);
        }

        private int GetCurrentPartLength(string s, bool removeDecimalPart)
        {
            if (!removeDecimalPart)
            {
                return s.Length;
            }

            var dotIndex = s.IndexOf('.');
            return dotIndex > 0
                ? s.Remove(dotIndex).Length
                : s.Length;
        }
    }
}
