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

[Obsolete("This rule has been deprecated since 10.27")]
public abstract class ExpandingArchivesBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S5042";

    protected override string MessageFormat => "Make sure that decompressing this archive file is safe.";

    protected ExpandingArchivesBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        var input = new TrackerInput(context, AnalyzerConfiguration.AlwaysEnabled, Rule);
        var t = Language.Tracker.Invocation;
        t.Track(
            input,
            t.MatchMethod(
                new MemberDescriptor(KnownType.System_IO_Compression_ZipFileExtensions, "ExtractToFile"),
                new MemberDescriptor(KnownType.System_IO_Compression_ZipFileExtensions, "ExtractToDirectory"),
                new MemberDescriptor(KnownType.System_IO_Compression_ZipFile, "ExtractToDirectory")));
    }
}
