/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System.Collections.Generic;
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
    public class ClassWithEqualityShouldImplementIEquatable : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3897";
        private const string MessageFormat = "{0}";
        private const string ImplementAndOverrideMessage = "Implement 'IEquatable<{0}>' and override 'Equals(object)'.";
        private const string ImplementIEquatableMessage = "Implement 'IEquatable<{0}>'.";
        private const string OverrideEqualsMessage = "Override 'Equals(object)'.";
        private const string EqualsTSecondaryMessage = "Call this method from 'Equals(object)'.";
        private const string EqualsObjectSecondaryMessage = "Call 'Equals(T)' from this method.";
        private const string EqualsMethodName = nameof(object.Equals);

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        protected sealed override DiagnosticDescriptor Rule => rule;

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)c.Node;
                    var classSymbol = c.SemanticModel.GetDeclaredSymbol(classDeclaration);
                    if (classSymbol == null)
                    {
                        return;
                    }

                    var equalsMethodSymbols = classDeclaration.Members
                        .OfType<MethodDeclarationSyntax>()
                        .Select(mds => c.SemanticModel.GetDeclaredSymbol(mds).ToSymbolWithSyntax(mds))
                        .Where(node => IsValidEqualsMethodSymbol(node.Symbol))
                        .ToList();

                    var objectEqualsMethod = equalsMethodSymbols.FirstOrDefault(ms => ms.Symbol.Parameters[0].Type.Is(KnownType.System_Object))?.Syntax;
                    var equatableInterface = classSymbol.AllInterfaces.FirstOrDefault(nts => nts.ConstructedFrom.Is(KnownType.System_IEquatable_T));
                    var typeSpecificEqualsMethod = FindTypeSpecificEqualsMethod(equalsMethodSymbols, equatableInterface, classSymbol);

                    if (HasNoTriggeringEqualsMethod(objectEqualsMethod, typeSpecificEqualsMethod) ||
                        IsEquatableInterfaceCorrectlyImplemented(objectEqualsMethod, typeSpecificEqualsMethod, equatableInterface))
                    {
                        return;
                    }

                    c.ReportDiagnostic(CreateReportDiagnostic(equatableInterface != null, objectEqualsMethod,
                        typeSpecificEqualsMethod, classDeclaration.Identifier));
                }, SyntaxKind.ClassDeclaration);
        }

        private static bool IsValidEqualsMethodSymbol(IMethodSymbol methodSymbol)
        {
            return methodSymbol.MethodKind == MethodKind.Ordinary &&
                methodSymbol.Name == EqualsMethodName &&
                methodSymbol.Parameters.Length == 1 &&
                methodSymbol.ReturnType.Is(KnownType.System_Boolean);
        }

        private static MethodDeclarationSyntax FindTypeSpecificEqualsMethod(
            IList<SyntaxNodeWithSymbol<MethodDeclarationSyntax, IMethodSymbol>> equalsMethodSymbols,
            INamedTypeSymbol equatableInterface, INamedTypeSymbol classSymbol)
        {
            var equalsParameterType = equatableInterface == null ?
                classSymbol :
                equatableInterface.TypeArguments[0];

            return equalsMethodSymbols.FirstOrDefault(
                    ms => !ms.Symbol.IsOverride && ms.Symbol.Parameters[0].Type.Equals(equalsParameterType))
                ?.Syntax;
        }

        private static bool HasNoTriggeringEqualsMethod(MethodDeclarationSyntax objectEqualsMethod,
            MethodDeclarationSyntax typeSpecificEqualsMethod)
        {
            return objectEqualsMethod == null && typeSpecificEqualsMethod == null;
        }

        private static bool IsEquatableInterfaceCorrectlyImplemented(MethodDeclarationSyntax objectEqualsMethod,
            MethodDeclarationSyntax typeSpecificEqualsMethod, INamedTypeSymbol equatableInterface)
        {
            return objectEqualsMethod != null && typeSpecificEqualsMethod != null && equatableInterface != null;
        }

        private Diagnostic CreateReportDiagnostic(bool implementsIEquatable, MethodDeclarationSyntax objectEqualsMethod,
            MethodDeclarationSyntax typeSpecificEqualsMethod, SyntaxToken classIdentifier)
        {
            if (objectEqualsMethod != null && typeSpecificEqualsMethod != null)
            {
                return Diagnostic.Create(Rule, classIdentifier.GetLocation(),
                    string.Format(ImplementIEquatableMessage, classIdentifier.ValueText));
            }

            if (objectEqualsMethod != null)
            {
                return Diagnostic.Create(Rule, classIdentifier.GetLocation(),
                    additionalLocations: new[] { objectEqualsMethod.Identifier.GetLocation() },
                    properties: new Dictionary<string, string> { ["0"] = EqualsObjectSecondaryMessage }.ToImmutableDictionary(),
                    messageArgs: string.Format(ImplementIEquatableMessage, classIdentifier.ValueText));
            }

            if (!implementsIEquatable)
            {
                return Diagnostic.Create(Rule, classIdentifier.GetLocation(),
                    additionalLocations: new[] { typeSpecificEqualsMethod.Identifier.GetLocation() },
                    properties: new Dictionary<string, string> { ["0"] = EqualsTSecondaryMessage }.ToImmutableDictionary(),
                    messageArgs: string.Format(ImplementAndOverrideMessage, classIdentifier.ValueText));
            }

            return Diagnostic.Create(Rule, classIdentifier.GetLocation(),
                additionalLocations: new[] { typeSpecificEqualsMethod.Identifier.GetLocation() },
                properties: new Dictionary<string, string> { ["0"] = EqualsTSecondaryMessage }.ToImmutableDictionary(),
                messageArgs: OverrideEqualsMessage);
        }
    }
}
