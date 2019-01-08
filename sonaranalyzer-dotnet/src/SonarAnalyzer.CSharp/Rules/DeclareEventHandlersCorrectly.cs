/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DeclareEventHandlersCorrectly : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3906";
        private const string MessageFormat = "Change the signature of that event handler to match the specified signature.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
               c => AnalyzeEventType(c, ((EventFieldDeclarationSyntax)c.Node).Declaration.Type),
               SyntaxKind.EventFieldDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
               c => AnalyzeEventType(c, ((EventDeclarationSyntax)c.Node).Type),
               SyntaxKind.EventDeclaration);
        }

        private void AnalyzeEventType(SyntaxNodeAnalysisContext analysisContext, TypeSyntax typeSyntax)
        {
            var eventHandlerType = analysisContext.SemanticModel.GetSymbolInfo(typeSyntax).Symbol
                        as INamedTypeSymbol;
            var methodSymbol = eventHandlerType?.DelegateInvokeMethod;
            if (methodSymbol == null)
            {
                return;
            }

            if (!IsCorrectEventHandlerSignature(methodSymbol))
            {
                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(rule, typeSyntax.GetLocation()));
            }
        }

        private bool IsCorrectEventHandlerSignature(IMethodSymbol methodSymbol)
        {
            return methodSymbol.ReturnsVoid &&
                methodSymbol.Parameters.Length == 2 &&
                methodSymbol.Parameters[0].Name == "sender" &&
                methodSymbol.Parameters[0].Type.Is(KnownType.System_Object) &&
                methodSymbol.Parameters[1].Name == "e" &&
                methodSymbol.Parameters[1].Type.DerivesFrom(KnownType.System_EventArgs);
        }
    }
}
