/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.TestFramework.Packaging;

namespace SonarAnalyzer.TestFramework.Extensions;

public static class DiagnosticDescriptorExtensions
{
    public static bool IsSecurityHotspot(this DiagnosticDescriptor diagnostic)
    {
        var type = RuleTypeMappingCS.Rules.GetValueOrDefault(diagnostic.Id) ?? RuleTypeMappingVB.Rules.GetValueOrDefault(diagnostic.Id);
        return type == "SECURITY_HOTSPOT";
    }
}
