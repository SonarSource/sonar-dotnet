/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
               c => AnalyzeEventType(c, ((EventFieldDeclarationSyntax)c.Node).Declaration.Type, c.ContainingSymbol),
               SyntaxKind.EventFieldDeclaration);

            context.RegisterNodeAction(
               c => AnalyzeEventType(c, ((EventDeclarationSyntax)c.Node).Type, c.ContainingSymbol),
               SyntaxKind.EventDeclaration);
        }

        private static void AnalyzeEventType(SonarSyntaxNodeReportingContext analysisContext, TypeSyntax typeSyntax, ISymbol eventSymbol)
        {
            if (!eventSymbol.IsOverride
                && eventSymbol.GetInterfaceMember() is null
                && analysisContext.SemanticModel.GetSymbolInfo(typeSyntax).Symbol is INamedTypeSymbol eventHandlerType
                && eventHandlerType.DelegateInvokeMethod is { } methodSymbol
                && !IsCorrectEventHandlerSignature(methodSymbol))
            {
                analysisContext.ReportIssue(CreateDiagnostic(Rule, typeSyntax.GetLocation()));
            }
        }

        private static bool IsCorrectEventHandlerSignature(IMethodSymbol methodSymbol) =>
            methodSymbol is
            {
                ReturnsVoid: true,
                Parameters: { Length: DelegateEventHandlerArgCount } parameters,
            }
            && parameters[SenderArgumentPosition] is
            {
                Name: "sender",
                Type: { } senderType,
            }
            && senderType.Is(KnownType.System_Object)
            && parameters[EventArgsPosition] is
            {
                Name: "e",
                Type: { } eventArgsType,
            }
            && IsDerivedFromEventArgs(eventArgsType);

        private static bool IsDerivedFromEventArgs(ITypeSymbol type) =>
            type.DerivesFrom(KnownType.System_EventArgs)
            || (type is ITypeParameterSymbol typeParameterSymbol
                && typeParameterSymbol.ConstraintTypes.Any(IsDerivedFromEventArgs));
    }
}
