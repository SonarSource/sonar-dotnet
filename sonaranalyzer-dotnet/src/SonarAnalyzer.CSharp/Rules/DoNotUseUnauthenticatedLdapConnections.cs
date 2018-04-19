/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
    public sealed class DoNotUseUnauthenticatedLdapConnections : InitializeObjectsWithValueBase
    {
        internal const string DiagnosticId = "S4433";
        private const string MessageFormat = "Set the 'AuthenticationType' property of this DirectoryEntry to 'AuthenticationTypes.Secure'.";
        private const int AuthenticationTypes_Secure = 1;

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override DiagnosticDescriptor Rule => rule;

        internal override KnownType TrackedType => KnownType.System_DirectoryServices_DirectoryEntry;

        protected override string TrackedPropertyName => "AuthenticationType";

        protected override object ExpectedPropertyValue => AuthenticationTypes_Secure;

        protected override bool IsExpectedValue(object constantValue) =>
            constantValue is int integerValue &&
            (integerValue & AuthenticationTypes_Secure) > 0; // The expected value is a bit from a Flags enum

        protected override bool IsInitializedAsExpected(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel)
        {
            return objectCreation.Initializer == null && CtorHasExpectedArguments(objectCreation.ArgumentList) ||
                base.IsInitializedAsExpected(objectCreation, semanticModel);

            bool CtorHasExpectedArguments(ArgumentListSyntax argumentList) =>
                argumentList?.Arguments.Count != 4 || // other than 4 arguments
                (argumentList != null // 4 arguments and the last argument is with expected value
                    && objectCreation.ArgumentList.Arguments.Count == 4
                    && IsExpectedValue(objectCreation.ArgumentList.Arguments[3].Expression, semanticModel));
        }
    }
}
