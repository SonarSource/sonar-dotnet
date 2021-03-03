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
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ExpectedExceptionAttributeShouldNotBeUsed : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3431";
        private const string MessageFormat = "Replace the 'ExpectedException' attribute with a throw assertion or a try/catch block.";
        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)c.Node;
                    if (methodDeclaration.ExpressionBody == null
                        && methodDeclaration.Body?.Statements.Count > 1
                        && c.SemanticModel.GetDeclaredSymbol(methodDeclaration) is { } methodSymbol
                        && methodSymbol.GetAttributes(UnitTestHelper.KnownExpectedExceptionAttributes).FirstOrDefault() is { } firstExpectedExceptionAttributeOrDefault)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, firstExpectedExceptionAttributeOrDefault.ApplicationSyntaxReference.GetSyntax().GetLocation()));
                    }
                },
                SyntaxKind.MethodDeclaration);
    }
}
