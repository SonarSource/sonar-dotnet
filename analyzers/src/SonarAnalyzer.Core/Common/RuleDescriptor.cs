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

namespace SonarAnalyzer.Common
{
    public record RuleDescriptor(string Id, string Title, string Type, string DefaultSeverity, string Status, SourceScope Scope, bool SonarWay, string Description)
    {
        public string Category =>
            $"{DefaultSeverity} {ReadableType}";

        private string ReadableType =>
            Type switch
            {
                "BUG" => "Bug",
                "CODE_SMELL" => "Code Smell",
                "VULNERABILITY" => "Vulnerability",
                "SECURITY_HOTSPOT" => "Security Hotspot",
                _ => throw new UnexpectedValueException(nameof(Type), Type)
            };

        public bool IsHotspot => Type == "SECURITY_HOTSPOT";
    }
}
