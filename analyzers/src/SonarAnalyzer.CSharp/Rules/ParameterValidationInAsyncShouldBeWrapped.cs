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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ParameterValidationInAsyncShouldBeWrapped : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4457";
        private const string MessageFormat = "Split this method into two, one handling parameters check and the other handling the asynchronous code.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var method = (MethodDeclarationSyntax)c.Node;
                    if (!method.Modifiers.Any(SyntaxKind.AsyncKeyword)
                        || method.ReturnType.ToString() == "void"
                        || (method.Identifier.ValueText == "Main" && c.SemanticModel.GetDeclaredSymbol(method).IsMainMethod()))
                    {
                        return;
                    }

                    var walker = new ParameterValidationInMethodWalker(c.SemanticModel);
                    walker.SafeVisit(method);
                    if (walker.ArgumentExceptionLocations.Any())
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, method.Identifier.GetLocation(), walker.ArgumentExceptionLocations));
                    }
                },
                SyntaxKind.MethodDeclaration);
    }
}
