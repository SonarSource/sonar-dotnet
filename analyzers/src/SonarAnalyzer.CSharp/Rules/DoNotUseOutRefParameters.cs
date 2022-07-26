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

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotUseOutRefParameters : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3874";
        private const string MessageFormat = "Consider refactoring this method in order to remove the need for this '{0}' modifier.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var parameter = (ParameterSyntax)c.Node;

                    if (!parameter.Modifiers.Any(IsRefOrOut)
                        || (parameter is { Parent: ParameterListSyntax { Parent: MethodDeclarationSyntax method } } && IsDeconstructor(method)))
                    {
                        return;
                    }

                    var modifier = parameter.Modifiers.First(IsRefOrOut);

                    var parameterSymbol = c.SemanticModel.GetDeclaredSymbol(parameter);

                    if (parameterSymbol?.ContainingSymbol is not IMethodSymbol containingMethod
                        || containingMethod.IsOverride
                        || !containingMethod.IsPubliclyAccessible()
                        || IsTryPattern(containingMethod, modifier)
                        || containingMethod.GetInterfaceMember() != null)
                    {
                        return;
                    }

                    c.ReportIssue(Diagnostic.Create(Rule, modifier.GetLocation(), modifier.ValueText));
                },
                SyntaxKind.Parameter);

        private static bool IsTryPattern(IMethodSymbol method, SyntaxToken modifier) =>
            method.Name.StartsWith("Try", StringComparison.Ordinal)
            && method.ReturnType.Is(KnownType.System_Boolean)
            && modifier.IsKind(SyntaxKind.OutKeyword);

        private static bool IsRefOrOut(SyntaxToken token) =>
            token.IsKind(SyntaxKind.RefKeyword)
            || token.IsKind(SyntaxKind.OutKeyword);

        private static bool IsDeconstructor(MethodDeclarationSyntax node) =>
            node.HasReturnTypeVoid()
            && node.Identifier.Value.Equals("Deconstruct")
            && ((node.Modifiers.Count == 1 && node.Modifiers.Any(SyntaxKind.PublicKeyword))
                 || node.IsExtensionMethod());
    }
}
