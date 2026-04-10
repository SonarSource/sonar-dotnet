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
