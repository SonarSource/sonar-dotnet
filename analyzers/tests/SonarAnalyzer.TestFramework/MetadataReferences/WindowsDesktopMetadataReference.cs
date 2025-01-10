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

#if NET

using static SonarAnalyzer.TestFramework.MetadataReferences.MetadataReferenceFactory;

namespace SonarAnalyzer.TestFramework.MetadataReferences;

internal static class WindowsDesktopMetadataReference
{
    internal static MetadataReference PresentationFramework { get; } = CreateReference("PresentationFramework.dll", Sdk.WindowsDesktop);
    internal static MetadataReference PresentationCore { get; } = CreateReference("PresentationCore.dll", Sdk.WindowsDesktop);
    internal static MetadataReference SystemWindowsForms { get; } = CreateReference("System.Windows.Forms.dll", Sdk.WindowsDesktop);
    internal static MetadataReference WindowsBase { get; } = CreateReference("WindowsBase.dll", Sdk.WindowsDesktop);
}

#endif
