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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

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

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override CSharpObjectInitializationTracker ObjectInitializationTracker { get; } = new CSharpObjectInitializationTracker(
            isAllowedConstantValue: constantValue => constantValue is int integerValue && !IsUnsafe(integerValue),
            trackedTypes: ImmutableArray.Create(KnownType.System_DirectoryServices_DirectoryEntry),
            isTrackedPropertyName: propertyName => propertyName == "AuthenticationType",
            isAllowedObject: IsAllowedObject,
            trackedConstructorArgumentIndex: 3
        );

        private static bool IsAllowedObject(ISymbol authTypeSymbol, ExpressionSyntax authTypeExpression, SemanticModel semanticModel) =>
            authTypeSymbol.GetSymbolType().Is(KnownType.System_DirectoryServices_AuthenticationTypes)
            && !(authTypeExpression.FindConstantValue(semanticModel) is int authType && IsUnsafe(authType));

        private static bool IsUnsafe(int authType) =>
            authType == AuthenticationTypesNone || authType == AuthenticationTypesAnonymous;
    }
}
