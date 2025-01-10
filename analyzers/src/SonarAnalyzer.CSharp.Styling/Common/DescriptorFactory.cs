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

using SonarAnalyzer.Core.Analyzers;

namespace SonarAnalyzer.CSharp.Styling.Common;

internal static class DescriptorFactory
{
    public static DiagnosticDescriptor Create(string id, string messageFormat, SourceScope scope = SourceScope.All) =>
        DiagnosticDescriptorFactory.Create(
            AnalyzerLanguage.CSharp,
            new RuleDescriptor(id, $"Internal Styling Rule {id}", "CODE_SMELL", "Major", null, scope, true, null),
            messageFormat,
            true,
            false);
}
