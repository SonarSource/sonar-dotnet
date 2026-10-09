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

using System.Text;

namespace SonarAnalyzer.TestFramework.Build;

public sealed class RazorAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
{
    private const string TargetPathKey = "build_metadata.AdditionalFiles.TargetPath";

    private readonly string rootPath;

    public override AnalyzerConfigOptions GlobalOptions => TargetPathOptions.None;

    public RazorAnalyzerConfigOptionsProvider(string rootPath) =>
        this.rootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) =>
        TargetPathOptions.None;

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) =>
        new TargetPathOptions(Convert.ToBase64String(Encoding.UTF8.GetBytes(TestFiles.GetRelativePath(rootPath, textFile.Path))));

    private sealed class TargetPathOptions : AnalyzerConfigOptions
    {
        public static readonly TargetPathOptions None = new(null);

        private readonly string targetPath;

        public TargetPathOptions(string targetPath) =>
            this.targetPath = targetPath;

        public override bool TryGetValue(string key, out string value)
        {
            if (key == TargetPathKey && targetPath is not null)
            {
                value = targetPath;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
    }
}
