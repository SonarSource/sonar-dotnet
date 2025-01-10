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

using SonarAnalyzer.CSharp.Core.Trackers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class LdapConnectionShouldBeSecure : ObjectShouldBeInitializedCorrectlyBase
    {
        private const string DiagnosticId = "S4433";
        private const string MessageFormat = "Set the 'AuthenticationType' property of this DirectoryEntry to 'AuthenticationTypes.Secure'.";

        private const int AuthenticationTypesNone = 0;
        private const int AuthenticationTypesAnonymous = 16;

        public LdapConnectionShouldBeSecure() : base(AnalyzerConfiguration.AlwaysEnabled, DiagnosticId, MessageFormat) { }

        protected override CSharpObjectInitializationTracker ObjectInitializationTracker { get; } = new CSharpObjectInitializationTracker(
            isAllowedConstantValue: constantValue => constantValue is int integerValue && !IsUnsafe(integerValue),
            trackedTypes: ImmutableArray.Create(KnownType.System_DirectoryServices_DirectoryEntry),
            isTrackedPropertyName: propertyName => propertyName == "AuthenticationType",
            isAllowedObject: IsAllowedObject,
            trackedConstructorArgumentIndex: 3
        );

        private static bool IsAllowedObject(ISymbol authTypeSymbol, SyntaxNode authTypeExpression, SemanticModel semanticModel) =>
            authTypeSymbol.GetSymbolType().Is(KnownType.System_DirectoryServices_AuthenticationTypes)
            && !(authTypeExpression.FindConstantValue(semanticModel) is int authType && IsUnsafe(authType));

        private static bool IsUnsafe(int authType) =>
            authType == AuthenticationTypesNone || authType == AuthenticationTypesAnonymous;
    }
}
