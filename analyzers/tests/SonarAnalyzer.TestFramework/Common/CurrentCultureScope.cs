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

using System.Globalization;

namespace SonarAnalyzer.TestFramework.Common;

public sealed class CurrentCultureScope : IDisposable
{
    private readonly CultureInfo oldCulture;
    private readonly CultureInfo oldUiCulture;

    public CurrentCultureScope() : this(CultureInfo.InvariantCulture) { }

    public CurrentCultureScope(CultureInfo culture)
    {
        var thread = Thread.CurrentThread;
        oldCulture = thread.CurrentCulture;
        oldUiCulture = thread.CurrentUICulture;
        thread.CurrentCulture = culture;
        thread.CurrentUICulture = culture;
    }

    public void Dispose()
    {
        var thread = Thread.CurrentThread;
        thread.CurrentCulture = oldCulture;
        thread.CurrentUICulture = oldUiCulture;
    }
}
