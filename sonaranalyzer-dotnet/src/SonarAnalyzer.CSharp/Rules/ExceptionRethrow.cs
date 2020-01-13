/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
    public sealed class ExceptionRethrow : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3445";
        private const string MessageFormat = "Consider using 'throw;' to preserve the stack trace.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var catchClause = (CatchClauseSyntax)c.Node;
                    if (catchClause.Declaration == null ||
                        catchClause.Declaration.Identifier.IsKind(SyntaxKind.None))
                    {
                        return;
                    }

                    var exceptionIdentifier = c.SemanticModel.GetDeclaredSymbol(catchClause.Declaration);
                    if (exceptionIdentifier == null)
                    {
                        return;
                    }

                    var throws = catchClause.DescendantNodes(n =>
                            n == catchClause ||
                            !n.IsKind(SyntaxKind.CatchClause))
                        .OfType<ThrowStatementSyntax>()
                        .Where(t => t.Expression != null);

                    foreach (var @throw in throws)
                    {
                        var thrown = c.SemanticModel.GetSymbolInfo(@throw.Expression).Symbol as ILocalSymbol;
                        if (Equals(thrown, exceptionIdentifier))
                        {
                            c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, @throw.GetLocation()));
                        }
                    }
                },
                SyntaxKind.CatchClause);
        }
    }
}
