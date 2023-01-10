/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

#nullable enable

using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Formatting;

namespace SonarAnalyzer.ShimLayer;

public static class ClassificationHelpersWrapper
{
    private static Func<SyntaxToken, string?>? getCSharpClassification;
    private static Func<SyntaxToken, string?>? getVBClassification;

    public static string? GetCSharpClassification(SyntaxToken token)
    {
        getCSharpClassification ??= typeof(CSharpFormattingOptions) is { Assembly: { } assembly } // Microsoft.CodeAnalysis.CSharp.Workspaces.dll
            && assembly.GetType("Microsoft.CodeAnalysis.CSharp.Classification.ClassificationHelpers") is { } classificationHelperType
            && classificationHelperType.GetMethod("GetClassification", BindingFlags.Static | BindingFlags.Public) is { } getClassificationMethod
            ? token => getClassificationMethod.Invoke(null, new object[] { token }) as string
            : token => (string?)null;
        return getCSharpClassification(token);
    }

    public static string? GetVBClassification(SyntaxToken token)
    {
        getVBClassification ??= Assembly.Load(new AssemblyName("Microsoft.CodeAnalysis.VisualBasic.Workspaces")) is { } assembly
            && assembly.GetType("Microsoft.CodeAnalysis.VisualBasic.Classification.ClassificationHelpers") is { } classificationHelperType
            && classificationHelperType.GetMethod("GetClassification", BindingFlags.Static | BindingFlags.Public) is { } getClassificationMethod
            ? token => getClassificationMethod.Invoke(null, new object[] { token }) as string
            : token => (string?)null;
        return getVBClassification(token);
    }
}
