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
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DeclareEventHandlersCorrectly : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3906";
        private const string MessageFormat = "Change the signature of that event handler to match the specified signature.";
        private const int SenderArgumentPosition = 0;
        private const int EventArgsPosition = 1;
        private const int DelegateEventHandlerArgCount = 2;

        private static readonly DiagnosticDescriptor Rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
               c => AnalyzeEventType(c, ((EventFieldDeclarationSyntax)c.Node).Declaration.Type, c.ContainingSymbol),
               SyntaxKind.EventFieldDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
               c => AnalyzeEventType(c, ((EventDeclarationSyntax)c.Node).Type, c.ContainingSymbol),
               SyntaxKind.EventDeclaration);
        }

        private static void AnalyzeEventType(SyntaxNodeAnalysisContext analysisContext, TypeSyntax typeSyntax, ISymbol eventSymbol)
        {
            if (!eventSymbol.IsOverride
                && eventSymbol.GetInterfaceMember() is null
                && analysisContext.SemanticModel.GetSymbolInfo(typeSyntax).Symbol is INamedTypeSymbol eventHandlerType
                && eventHandlerType.DelegateInvokeMethod is { } methodSymbol
                && !IsCorrectEventHandlerSignature(methodSymbol))
            {
                analysisContext.ReportIssue(Diagnostic.Create(Rule, typeSyntax.GetLocation()));
            }
        }

        private static bool IsCorrectEventHandlerSignature(IMethodSymbol methodSymbol) =>
            methodSymbol.ReturnsVoid
            && methodSymbol.Parameters.Length == DelegateEventHandlerArgCount
            && methodSymbol.Parameters[SenderArgumentPosition].Name == "sender"
            && methodSymbol.Parameters[SenderArgumentPosition].Type.Is(KnownType.System_Object)
            && methodSymbol.Parameters[EventArgsPosition].Name == "e"
            && IsDerivedFromEventArgs(methodSymbol.Parameters[1].Type);

        private static bool IsDerivedFromEventArgs(ITypeSymbol type) =>
            type.DerivesFrom(KnownType.System_EventArgs)
            || (type is ITypeParameterSymbol typeParameterSymbol
                && typeParameterSymbol.ConstraintTypes.Any(IsDerivedFromEventArgs));
    }
}
