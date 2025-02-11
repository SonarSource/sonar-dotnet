/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class InterfaceMethodsShouldBeCallableByChildTypes : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4039";
        private const string MessageFormat = "Make '{0}' sealed, change to a non-explicit declaration or provide a " +
            "new method exposing the functionality of '{1}'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c => ReportOnIssue<MethodDeclarationSyntax>(c, m => m.ExplicitInterfaceSpecifier, m => m.Identifier, AreMethodsEquivalent),
                SyntaxKind.MethodDeclaration);

            context.RegisterNodeAction(
                c => ReportOnIssue<PropertyDeclarationSyntax>(c, m => m.ExplicitInterfaceSpecifier, m => m.Identifier, ArePropertiesEquivalent),
                SyntaxKind.PropertyDeclaration);

            context.RegisterNodeAction(
                c => ReportOnIssue<EventDeclarationSyntax>(c, m => m.ExplicitInterfaceSpecifier, m => m.Identifier, AreEventsEquivalent),
                SyntaxKind.EventDeclaration);
        }

        private static void ReportOnIssue<TMemberSyntax>(SonarSyntaxNodeReportingContext analysisContext,
                                                         Func<TMemberSyntax, ExplicitInterfaceSpecifierSyntax> getExplicitInterfaceSpecifier,
                                                         Func<TMemberSyntax, SyntaxToken> getIdentifierName,
                                                         Func<TMemberSyntax, TMemberSyntax, bool> areMembersEquivalent)
            where TMemberSyntax : MemberDeclarationSyntax
        {
            var memberDeclaration = (TMemberSyntax)analysisContext.Node;

            var explicitInterfaceSpecifier = getExplicitInterfaceSpecifier(memberDeclaration);
            if (explicitInterfaceSpecifier == null)
            {
                return;
            }

            var declaration = (TypeDeclarationSyntax)memberDeclaration.FirstAncestorOrSelf<SyntaxNode>(node => node is TypeDeclarationSyntax);
            if (declaration == null
                || declaration.Identifier.IsMissing
                || !IsDeclarationTracked(declaration, analysisContext.Model))
            {
                return;
            }

            var hasPublicEquivalentMethod = declaration.Members
                                                       .OfType<TMemberSyntax>()
                                                       .Any(member => areMembersEquivalent(member, memberDeclaration));
            if (!hasPublicEquivalentMethod)
            {
                var identifierName = getIdentifierName(memberDeclaration);

                analysisContext.ReportIssue(Rule, identifierName, declaration.Identifier.ValueText, string.Concat(explicitInterfaceSpecifier.Name, ".", identifierName.ValueText));
            }
        }

        private static bool IsDeclarationTracked(BaseTypeDeclarationSyntax declaration, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetDeclaredSymbol(declaration);

            return symbol is { IsSealed: false }
                   && symbol.IsPubliclyAccessible();
        }

        private static bool AreMethodsEquivalent(MethodDeclarationSyntax currentMethod, MethodDeclarationSyntax targetedMethod) =>
            currentMethod != targetedMethod
            && currentMethod.Modifiers.Any(IsPublicOrProtected)
            && (currentMethod.Identifier.ValueText == targetedMethod.Identifier.ValueText
                || (targetedMethod.Identifier.ValueText == nameof(IDisposable.Dispose) && currentMethod.Identifier.ValueText == "Close")); // Allows to replace IDisposable.Dispose() with Close()

        private static bool ArePropertiesEquivalent(PropertyDeclarationSyntax currentProperty, PropertyDeclarationSyntax targetedProperty) =>
            currentProperty != targetedProperty
            && currentProperty.Identifier.ValueText == targetedProperty.Identifier.ValueText
            && currentProperty.Modifiers.Any(IsPublicOrProtected);

        private static bool AreEventsEquivalent(EventDeclarationSyntax currentEvent, EventDeclarationSyntax targetedEvent) =>
            currentEvent != targetedEvent
            && currentEvent.Identifier.ValueText == targetedEvent.Identifier.ValueText
            && currentEvent.Modifiers.Any(IsPublicOrProtected);

        private static bool IsPublicOrProtected(SyntaxToken modifier) =>
            modifier.IsKind(SyntaxKind.PublicKeyword)
            || modifier.IsKind(SyntaxKind.ProtectedKeyword);
    }
}
