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

namespace SonarAnalyzer.Core.Syntax.Extensions;

public static class LocationExtensions
{
    extension(Location location)
    {
        public FileLinePositionSpan MappedLineSpanIfAvailable =>
            GeneratedCodeRecognizer.IsRazorGeneratedFile(location.SourceTree)
                ? location.GetMappedLineSpan()
                : location.GetLineSpan();

        public int StartLine => location.GetLineSpan().StartLinePosition.Line;

        public int StartColumn => location.GetLineSpan().StartLinePosition.Character;

        public int EndLine => location.GetLineSpan().EndLinePosition.Line;

        public int LineNumberToReport => location.GetMappedLineSpan().StartLinePosition.LineNumberToReport;

        public Location EnsureMappedLocation()
        {
            if (location is null || !GeneratedCodeRecognizer.IsRazorGeneratedFile(location.SourceTree))
            {
                return location;
            }

            var lineSpan = location.GetMappedLineSpan();

            return Location.Create(lineSpan.Path, location.SourceSpan, lineSpan.Span);
        }

        public bool IsValid(Compilation compilation) =>
            location.Kind != LocationKind.SourceFile || compilation.ContainsSyntaxTree(location.SourceTree);

        public SecondaryLocation ToSecondary(string message = null, params string[] messageArgs) =>
            message is not null && messageArgs?.Length > 0
                ? new(location, string.Format(message, messageArgs))
                : new(location, message);
    }
}
