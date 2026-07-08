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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.Core.Rules;

public abstract class BypassingAccessibilityBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    protected const string DiagnosticId = "S3011";

    protected override string MessageFormat => "Make sure that this accessibility bypass is safe here.";

    protected BypassingAccessibilityBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        Initialize(new TrackerInput(context, AnalyzerConfiguration.AlwaysEnabled, Rule));

    private void Initialize(TrackerInput input)
    {
        var t = Language.Tracker.FieldAccess;
        t.Track(
            input,
            t.WhenRead(),
            t.MatchField(new MemberDescriptor(KnownType.System_Reflection_BindingFlags, "NonPublic")));
    }
}
