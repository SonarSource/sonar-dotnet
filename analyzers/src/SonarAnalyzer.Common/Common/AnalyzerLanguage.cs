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

using System;
using System.IO;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Common
{
    public sealed class AnalyzerLanguage
    {
        private const string CsLiteral = "cs";
        private const string VbLiteral = "vbnet";

        public static readonly AnalyzerLanguage CSharp = new AnalyzerLanguage(CsLiteral);
        public static readonly AnalyzerLanguage VisualBasic = new AnalyzerLanguage(VbLiteral);

        private readonly string language;

        public string LanguageName
        {
            get
            {
                if (this == CSharp)
                {
                    return LanguageNames.CSharp;
                }
                else if (this == VisualBasic)
                {
                    return LanguageNames.VisualBasic;
                }
                else
                {
                    throw new NotSupportedException($"Can't get language name for '{ToString()}'.");
                }
            }
        }

        public string FileExtension
        {
            get
            {
                if (this == CSharp)
                {
                    return ".cs";
                }
                else if (this == VisualBasic)
                {
                    return ".vb";
                }
                else
                {
                    throw new NotSupportedException($"Can't get file extension for '{ToString()}'.");
                }
            }
        }

        private AnalyzerLanguage(string language) =>
            this.language = language;

        public override string ToString() =>
            language;

        public static AnalyzerLanguage Parse(string language) =>
            language switch
            {
                CsLiteral => CSharp,
                VbLiteral => VisualBasic,
                _ => throw new NotSupportedException($"Argument needs to be '{CsLiteral}' or '{VbLiteral}', but found: '{language}'.")
            };

        public static AnalyzerLanguage FromPath(string path) =>
            Path.GetExtension(path).ToUpperInvariant() switch
            {
                ".CS" => CSharp,
                ".VB" => VisualBasic,
                _ => throw new NotSupportedException("Unsupported file extension: " + Path.GetExtension(path))
            };

        public static AnalyzerLanguage FromName(string name) =>
            name switch
            {
                LanguageNames.CSharp => CSharp,
                LanguageNames.VisualBasic => VisualBasic,
                _ => throw new NotSupportedException("Unsupported language name: " + name)
            };
    }
}
