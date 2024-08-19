/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
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
