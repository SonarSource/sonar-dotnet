/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

namespace SonarAnalyzer.Core.Rules;

public abstract class CommandPathBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    protected const string DiagnosticId = "S4036";

    private readonly Regex validPath = new(@"^(\.{0,2}[\\/]|\w+:)", RegexOptions.None, Constants.DefaultRegexTimeout);

    protected abstract string FirstArgument(InvocationContext context);

    protected override string MessageFormat => "Use an absolute path for this command.";

    protected CommandPathBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        var input = new TrackerInput(context, AnalyzerConfiguration.AlwaysEnabled, Rule);
        var inv = Language.Tracker.Invocation;
        inv.Track(
            input,
            inv.MatchMethod(new MemberDescriptor(KnownType.System_Diagnostics_Process, "Start")),
            x => IsInvalid(FirstArgument(x)));

        var pa = Language.Tracker.PropertyAccess;
        pa.Track(
            input,
            pa.MatchProperty(new MemberDescriptor(KnownType.System_Diagnostics_ProcessStartInfo, "FileName")),
            pa.MatchSetter(),
            x => IsInvalid((string)pa.AssignedValue(x)));

        var oc = Language.Tracker.ObjectCreation;
        oc.Track(
            input,
            oc.MatchConstructor(KnownType.System_Diagnostics_ProcessStartInfo),
            x => oc.ConstArgumentForParameter(x, "fileName") is string value && IsInvalid(value));
    }

    private bool IsInvalid(string path) =>
        !string.IsNullOrEmpty(path) && !validPath.SafeIsMatch(path);
}
