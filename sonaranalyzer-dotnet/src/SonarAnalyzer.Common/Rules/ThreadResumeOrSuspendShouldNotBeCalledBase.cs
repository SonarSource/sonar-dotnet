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

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ThreadResumeOrSuspendShouldNotBeCalledBase<TInvocation> : DoNotCallMethodsBase<TInvocation>
        where TInvocation : SyntaxNode
    {
        internal const string DiagnosticId = "S3889";
        protected const string MessageFormat = "Refactor the code to remove this use of '{0}'.";

        private readonly IEnumerable<MemberDescriptor> invalidMethods = new List<MemberDescriptor>
        {
            new MemberDescriptor(KnownType.System_Threading_Thread, "Suspend"),
            new MemberDescriptor(KnownType.System_Threading_Thread, "Resume")
        };

        internal override IEnumerable<MemberDescriptor> CheckedMethods => invalidMethods;
    }
}
