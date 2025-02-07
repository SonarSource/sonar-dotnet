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

using System.Text.RegularExpressions;
using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.Rules
{
    public abstract class CommandPathBase<TSyntaxKind> : TrackerHotspotDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S4036";
        private const string MessageFormat = "Make sure the \"PATH\" used to find this command includes only what you intend.";

        private readonly Regex validPath = new Regex(@"^(\.{0,2}[\\/]|\w+:)", RegexOptions.None, Constants.DefaultRegexTimeout);

        protected abstract string FirstArgument(InvocationContext context);

        protected CommandPathBase(IAnalyzerConfiguration configuration) : base(configuration, DiagnosticId, MessageFormat) { }

        protected override void Initialize(TrackerInput input)
        {
            var inv = Language.Tracker.Invocation;
            inv.Track(input,
                inv.MatchMethod(new MemberDescriptor(KnownType.System_Diagnostics_Process, "Start")),
                c => IsInvalid(FirstArgument(c)));

            var pa = Language.Tracker.PropertyAccess;
            pa.Track(input,
                pa.MatchProperty(new MemberDescriptor(KnownType.System_Diagnostics_ProcessStartInfo, "FileName")),
                pa.MatchSetter(),
                c => IsInvalid((string)pa.AssignedValue(c)));

            var oc = Language.Tracker.ObjectCreation;
            oc.Track(input,
                oc.MatchConstructor(KnownType.System_Diagnostics_ProcessStartInfo),
                c => oc.ConstArgumentForParameter(c, "fileName") is string value && IsInvalid(value));
        }

        private bool IsInvalid(string path) =>
            !string.IsNullOrEmpty(path) && !validPath.SafeIsMatch(path);
    }
}
