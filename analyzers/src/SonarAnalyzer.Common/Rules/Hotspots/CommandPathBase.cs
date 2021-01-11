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
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class CommandPathBase<TSyntaxKind> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S4036";
        private const string MessageFormat = "Make sure the \"PATH\" used to find this command includes only what you intend.";

        private static readonly Regex ValidPath = new Regex(@"^(\.{0,2}[\\/]|[a-zA-Z]:)");

        protected abstract string FirstArgument(InvocationContext context);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
        protected DiagnosticDescriptor Rule { get; }
        protected InvocationTracker<TSyntaxKind> InvocationTracker { get; set; }
        protected PropertyAccessTracker<TSyntaxKind> PropertyAccessTracker { get; set; }
        protected ObjectCreationTracker<TSyntaxKind> ObjectCreationTracker { get; set; }

        protected CommandPathBase(System.Resources.ResourceManager rspecStrings) =>
            Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecStrings).WithNotConfigurable();

        protected override void Initialize(SonarAnalysisContext context)
        {
            InvocationTracker.Track(context,
                InvocationTracker.MatchMethod(new MemberDescriptor(KnownType.System_Diagnostics_Process, "Start")),
                c => IsInvalid(FirstArgument(c)));

            PropertyAccessTracker.Track(context,
                PropertyAccessTracker.MatchProperty(new MemberDescriptor(KnownType.System_Diagnostics_ProcessStartInfo, "FileName")),
                PropertyAccessTracker.MatchSetter(),
                c => IsInvalid((string)PropertyAccessTracker.AssignedValue(c)));

            ObjectCreationTracker.Track(context,
                ObjectCreationTracker.MatchConstructor(KnownType.System_Diagnostics_ProcessStartInfo),
                c => ObjectCreationTracker.ConstArgumentForParameter(c, "fileName") is string value && IsInvalid(value));
        }

        private static bool IsInvalid(string path) =>
            !string.IsNullOrEmpty(path) && !ValidPath.IsMatch(path);
    }
}
