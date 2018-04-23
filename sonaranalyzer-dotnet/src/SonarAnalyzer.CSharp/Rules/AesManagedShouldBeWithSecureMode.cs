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

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class AesManagedShouldBeWithSecureMode : ObjectsShouldBeInitializedCorrectlyBase<int>
    {
        internal const string DiagnosticId = "S4432";
        private const string MessageFormat = "Use Galois/Counter (GCM/NoPadding) 'Mode' instead.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        protected override DiagnosticDescriptor Rule => rule;

        protected override string TrackedPropertyName => "Mode";

        protected override int ExpectedPropertyValue => throw new NotSupportedException();

        protected override bool ExpectedValueIsDefault => false;

        protected override int CtorArgumentsCount => -1;

        protected override int CtorArgumentIndex => throw new NotSupportedException();

        internal override KnownType TrackedType => KnownType.System_Security_Cryptography_AesManaged;

        protected override bool IsExpectedValue(object constantValue) =>
            false;
    }
}
