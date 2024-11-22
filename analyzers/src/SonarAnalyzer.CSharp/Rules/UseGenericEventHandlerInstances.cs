/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseGenericEventHandlerInstances : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3908";
        private const string MessageFormat = "Refactor this delegate to use 'System.EventHandler<TEventArgs>'.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ImmutableArray<KnownType> allowedTypes =
            ImmutableArray.Create(
                KnownType.System_EventHandler,
                KnownType.System_EventHandler_TEventArgs
            );

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
               c =>
               {
                   var eventField = (EventFieldDeclarationSyntax)c.Node;
                   var eventFirstVariable = eventField.Declaration.Variables.FirstOrDefault();

                   if (eventFirstVariable != null)
                   {
                       AnalyzeEventType(c, eventFirstVariable, eventField.Declaration.Type.GetLocation);
                   }
               }, SyntaxKind.EventFieldDeclaration);

            context.RegisterNodeAction(
               c =>
               {
                   var eventDeclaration = (EventDeclarationSyntax)c.Node;
                   AnalyzeEventType(c, eventDeclaration, eventDeclaration.Type.GetLocation);
               }, SyntaxKind.EventDeclaration);
        }

        private static void AnalyzeEventType(SonarSyntaxNodeReportingContext analysisContext, SyntaxNode eventNode,
            Func<Location> getLocationToReportOn)
        {

            if (analysisContext.SemanticModel.GetDeclaredSymbol(eventNode) is IEventSymbol eventSymbol &&
                !eventSymbol.IsOverride &&
                eventSymbol.GetInterfaceMember() == null &&
                (eventSymbol.Type as INamedTypeSymbol)?.ConstructedFrom.IsAny(allowedTypes) == false)
            {
                analysisContext.ReportIssue(rule, getLocationToReportOn());
            }
        }
    }
}
