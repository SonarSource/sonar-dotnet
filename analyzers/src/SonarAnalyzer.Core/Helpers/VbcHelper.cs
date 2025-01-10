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

namespace SonarAnalyzer.Helpers;

public static class VbcHelper
{
    public static readonly Regex VbNetErrorPattern = new Regex(@"\s+error(\s+\S+)?\s*:",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, RegexConstants.DefaultTimeout);

    /// <summary>
    /// VB.Net Complier (VBC) post-process issues and will fail if the line contains the <see cref="VbNetErrorPattern"/>.
    /// </summary>
    /// <remarks>
    /// This helper method is intended to be used only while waiting for the bug to be fixed on Microsoft side.
    /// <see href="https://github.com/dotnet/roslyn/issues/5724"/>.
    /// </remarks>
    /// <param name="diagnostic">
    /// The diagnostic to test.
    /// </param>
    /// <returns>
    /// Returns <c>true</c> when reporting the diagnostic will trigger a VBC post-process error and <c>false</c> otherwise.
    /// </returns>
    public static bool IsTriggeringVbcError(Diagnostic diagnostic)
    {
        if (diagnostic.Location == null ||
            diagnostic.Location.SourceTree?.GetRoot().Language != LanguageNames.VisualBasic)
        {
            return false;
        }

        var text = diagnostic.Location.SourceTree.GetText();
        var lineNumber = diagnostic.Location.GetLineNumberToReport();

        return IsTextMatchingVbcErrorPattern(text.Lines[lineNumber - 1].ToString());
    }

    public static bool IsTextMatchingVbcErrorPattern(string text) =>
        text != null &&
        VbNetErrorPattern.SafeIsMatch(text);
}
