﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NormalizeStringsToUppercase : DoNotCallMethodsCSharpBase
    {
        private const string DiagnosticId = "S4040";

        protected override string MessageFormat => "Change this normalization to 'ToUpperInvariant()'.";

        protected override IEnumerable<MemberDescriptor> CheckedMethods { get; } = new List<MemberDescriptor>
        {
            new(KnownType.System_Char, "ToLower"),
            new(KnownType.System_String, "ToLower"),
            new(KnownType.System_Char, "ToLowerInvariant"),
            new(KnownType.System_String, "ToLowerInvariant"),
        };

        public NormalizeStringsToUppercase() : base(DiagnosticId) { }

        protected override bool ShouldReportOnMethodCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, MemberDescriptor memberDescriptor)
        {
            var identifier = invocation.GetMethodCallIdentifier().Value.ValueText; // never null when we get here
            if (identifier == "ToLowerInvariant")
            {
                return true;
            }

            // ToLower and ToLowerInvariant are extension methods for string but not for char
            var isExtensionMethod = memberDescriptor.ContainingType == KnownType.System_String;

            return invocation.ArgumentList != null
                && invocation.ArgumentList.Arguments.Count == (isExtensionMethod ? 1 : 2)
                && invocation.ArgumentList.Arguments[isExtensionMethod ? 0 : 1].Expression.ToString() == "CultureInfo.InvariantCulture";
        }
    }
}
