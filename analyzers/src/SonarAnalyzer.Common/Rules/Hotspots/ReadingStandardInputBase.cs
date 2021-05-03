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

using System;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ReadingStandardInputBase<TSyntaxKind> : TrackerHotspotDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S4829";
        private const string MessageFormat = "Make sure that reading the standard input is safe here.";

        protected abstract bool WhenResultIsNotIgnored(InvocationContext context);

        protected ReadingStandardInputBase(IAnalyzerConfiguration configuration) : base(configuration, DiagnosticId, MessageFormat) { }

        protected override void Initialize(TrackerInput input)
        {
            var inv = Language.Tracker.Invocation;
            inv.Track(input, inv.MatchMethod(new MemberDescriptor(KnownType.System_Console, nameof(Console.OpenStandardInput))));

            inv.Track(input,
                new Condition(c => WhenResultIsNotIgnored(c)), // This is syntax-only check and we can execute it first
                inv.MatchMethod(
                    new MemberDescriptor(KnownType.System_Console, nameof(Console.Read)),
                    new MemberDescriptor(KnownType.System_Console, nameof(Console.ReadKey)),
                    new MemberDescriptor(KnownType.System_Console, nameof(Console.ReadLine))));

            var pa = Language.Tracker.PropertyAccess;
            pa.Track(input, pa.MatchProperty(new MemberDescriptor(KnownType.System_Console, nameof(Console.In))));
        }
    }
}
