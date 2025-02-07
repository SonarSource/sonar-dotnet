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

namespace SonarAnalyzer.Common;

/// <summary>
/// sonar-plugin-api / RuleParamType also supports lists with single/multi selection.
/// We don't have a way how to annotate those on .NET side.
/// See sonar-plugin-api / RuleParamTypeTest.java for format an usages.
/// </summary>
public enum PropertyType
{
    String,
    Text,
    Boolean,
    Integer,
    Float,
    RegularExpression,  // This will be translated to String by RuleParamType.parse() on the API side
}
