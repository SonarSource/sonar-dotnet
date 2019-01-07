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

using System;
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
    public sealed class InterfaceMethodsShouldBeCallableByChildTypes : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4039";
        private const string MessageFormat = "Make '{0}' sealed, change to a non-explicit declaration or provide a " +
            "new method exposing the functionality of '{1}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => ReportOnIssue<MethodDeclarationSyntax>(c, m => m.ExplicitInterfaceSpecifier, m => m.Identifier,
                    AreMethodsEquivalent),
                SyntaxKind.MethodDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => ReportOnIssue<PropertyDeclarationSyntax>(c, m => m.ExplicitInterfaceSpecifier, m => m.Identifier,
                    ArePropertiesEquivalent),
                SyntaxKind.PropertyDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => ReportOnIssue<EventDeclarationSyntax>(c, m => m.ExplicitInterfaceSpecifier, m => m.Identifier,
                    AreEventsEquivalent),
                SyntaxKind.EventDeclaration);
        }

        private static void ReportOnIssue<TMemberSyntax>(SyntaxNodeAnalysisContext analysisContext,
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

            var classDeclaration = memberDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration == null ||
                classDeclaration.Identifier.IsMissing ||
                !IsClassTracked(classDeclaration, analysisContext.SemanticModel))
            {
                return;
            }

            var hasPublicEquivalentMethod = classDeclaration.Members
                .OfType<TMemberSyntax>()
                .Any(member => areMembersEquivalent(member, memberDeclaration));
            if (!hasPublicEquivalentMethod)
            {
                var identifierName = getIdentifierName(memberDeclaration);

                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(rule, identifierName.GetLocation(),
                    classDeclaration.Identifier.ValueText,
                    string.Concat(explicitInterfaceSpecifier.Name, ".", identifierName.ValueText)));
            }
        }

        private static bool IsClassTracked(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

            return classSymbol != null &&
                !classSymbol.IsSealed &&
                !classSymbol.IsStatic &&
                classSymbol.IsPubliclyAccessible();
        }

        private static bool AreMethodsEquivalent(MethodDeclarationSyntax currentMethod,
            MethodDeclarationSyntax targetedMethod)
        {
            return currentMethod != targetedMethod &&
                currentMethod.Modifiers.Any(modifier => IsPublicOrProtected(modifier)) &&
                (currentMethod.Identifier.ValueText == targetedMethod.Identifier.ValueText ||
                    (targetedMethod.Identifier.ValueText == nameof(IDisposable.Dispose) &&
                    currentMethod.Identifier.ValueText == "Close")); // Allows to replace IDisposable.Dispose() with Close()
        }

        private static bool ArePropertiesEquivalent(PropertyDeclarationSyntax currentProperty,
            PropertyDeclarationSyntax targetedProperty)
        {
            return currentProperty != targetedProperty &&
                currentProperty.Identifier.ValueText == targetedProperty.Identifier.ValueText &&
                currentProperty.Modifiers.Any(modifier => IsPublicOrProtected(modifier));
        }

        private static bool AreEventsEquivalent(EventDeclarationSyntax currentEvent,
            EventDeclarationSyntax targetedEvent)
        {
            return currentEvent != targetedEvent &&
                currentEvent.Identifier.ValueText == targetedEvent.Identifier.ValueText &&
                currentEvent.Modifiers.Any(modifier => IsPublicOrProtected(modifier));
        }

        private static bool IsPublicOrProtected(SyntaxToken modifier) =>
            modifier.IsKind(SyntaxKind.PublicKeyword) ||
            modifier.IsKind(SyntaxKind.ProtectedKeyword);
    }
}
