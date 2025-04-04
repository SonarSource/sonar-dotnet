﻿/*
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

using SonarAnalyzer.CSharp.Core.Rspec;

namespace SonarAnalyzer.CSharp.Core.Common;

public static class DescriptorFactory
{
    public static DiagnosticDescriptor Create(string id, string messageFormat, bool? isEnabledByDefault = null, bool fadeOutCode = false, bool isCompilationEnd = false) =>
        // RuleCatalog class is created from SonarAnalyzer.SourceGenerator
        DiagnosticDescriptorFactory.Create(AnalyzerLanguage.CSharp, RuleCatalog.Rules[id], messageFormat, isEnabledByDefault, fadeOutCode, isCompilationEnd);
}
