/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SyntaxTrackers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class LdapConnectionShouldBeSecure : ObjectShouldBeInitializedCorrectlyBase
    {
        private const string DiagnosticId = "S4433";
        private const string MessageFormat = "Set the 'AuthenticationType' property of this DirectoryEntry to 'AuthenticationTypes.Secure'.";

        private const int AuthenticationTypesNone = 0;
        private const int AuthenticationTypesAnonymous = 16;

        private static readonly DiagnosticDescriptor rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override CSharpObjectInitializationTracker objectInitializationTracker { get; } = new CSharpObjectInitializationTracker(
            isAllowedConstantValue: IsAllowedValue,
            trackedTypes: TrackedTypes,
            isTrackedPropertyName: propertyName => "AuthenticationType" == propertyName,
            isAllowedObject: IsAllowedObject,
            trackedConstructorArgumentIndex: 3
        );

        private static bool IsAllowedValue(object constantValue) =>
            constantValue is int integerValue &&
            !IsUnsafe(integerValue);

        private static readonly ImmutableArray<KnownType> TrackedTypes = ImmutableArray.Create(KnownType.System_DirectoryServices_DirectoryEntry);

        /// <summary>
        /// Verifies if the AuthenticationType parameter used on constructor invocation is secure.
        /// </summary>
        /// <param name="authTypeSymbol">The symbol associated with the expression used to populate the AuthenticationType parameter</param>
        /// <param name="authTypeExpression">Expression used to populate the AuthenticationType parameter</param>
        private static bool IsAllowedObject(ISymbol authTypeSymbol, ExpressionSyntax authTypeExpression, SemanticModel semanticModel)
        {
            if (!authTypeSymbol.GetSymbolType().Is(KnownType.System_DirectoryServices_AuthenticationTypes))
            {
                return false;
            }

            // In order to correctly detect all the cases we will need to refactor the rule to use symbolic execution.
            // Until this is done, we can reduce the number of false positives by checking if the variable has one of the following
            // values assigned in the parent scope: AuthenticationTypes.None, AuthenticationTypes.Anonymous, default or default(AuthenticationTypes).
            var root = authTypeExpression.FirstAncestorOrSelf<MethodDeclarationSyntax>() ??
                       authTypeExpression.FirstAncestorOrSelf<ClassDeclarationSyntax>() ??
                       authTypeExpression.FirstAncestorOrSelf<StructDeclarationSyntax>() ??
                       authTypeExpression.FirstAncestorOrSelf<InterfaceDeclarationSyntax>() ??
                       authTypeExpression.SyntaxTree.GetRoot();

            // The CSharpObjectInitializationTracker is verifying only if the AuthenticationType property or DirectoryEntry ctor
            // was invoked with a valid value, which can be evaluated as a constant.
            // When the initialization is done with a variable, we check if an unsafe value was set during
            // variable declaration or later by assignment.
            return !HasUnsafeDeclaration(root, authTypeSymbol, semanticModel) &&
                   !HasUnsafeAssignment(root, authTypeSymbol, semanticModel);
        }

        private static bool HasUnsafeAssignment(SyntaxNode root, ISymbol symbol, SemanticModel semanticModel) =>
            root.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Where(assignment => AssignmentLeftHasSameSymbol(assignment, symbol, semanticModel))
                .Any(assignment => HasUnsafeConstantValue(assignment.Right, semanticModel));

        private static bool AssignmentLeftHasSameSymbol(AssignmentExpressionSyntax assignment, ISymbol symbol, SemanticModel semanticModel) =>
            assignment.Left is IdentifierNameSyntax identifierNameSyntax &&
            identifierNameSyntax.NameIs(symbol.Name) &&
            symbol.Equals(semanticModel.GetSymbolInfo(identifierNameSyntax).Symbol);

        private static bool HasUnsafeDeclaration(SyntaxNode root, ISymbol symbol, SemanticModel semanticModel) =>
            root.DescendantNodes()
                .OfType<VariableDeclaratorSyntax>()
                .Where(variableDeclarator => VariableDeclaratorHasSymbol(variableDeclarator, symbol, semanticModel))
                .Any(variableDeclarator => HasUnsafeConstantValue(variableDeclarator.Initializer.Value, semanticModel));

        private static bool VariableDeclaratorHasSymbol(VariableDeclaratorSyntax variableDeclarator, ISymbol symbol, SemanticModel semanticModel) =>
            variableDeclarator.Identifier.ValueText == symbol.Name &&
            symbol.Equals(semanticModel.GetDeclaredSymbol(variableDeclarator));

        private static bool HasUnsafeConstantValue(SyntaxNode node, SemanticModel semanticModel) =>
            semanticModel.GetConstantValue(node) is {} constantValue &&
            constantValue.HasValue &&
            constantValue.Value is int authType &&
            IsUnsafe(authType);

        private static bool IsUnsafe(int authType) =>
            authType == AuthenticationTypesNone || authType == AuthenticationTypesAnonymous;
    }
}
