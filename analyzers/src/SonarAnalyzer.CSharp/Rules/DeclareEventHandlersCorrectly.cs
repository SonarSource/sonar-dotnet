/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules
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
                && eventSymbol.InterfaceMembers().IsEmpty()
                && analysisContext.Model.GetSymbolInfo(typeSyntax).Symbol is INamedTypeSymbol eventHandlerType
                && eventHandlerType.DelegateInvokeMethod is { } methodSymbol
                && !IsCorrectEventHandlerSignature(methodSymbol))
            {
                analysisContext.ReportIssue(Rule, typeSyntax);
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
