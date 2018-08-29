/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

namespace SonarAnalyzer.Common
{
    public sealed class AnalyzerLanguage
    {
        private const string CsLiteral = "cs";
        private const string VbNetLiteral = "vbnet";

        public static readonly AnalyzerLanguage None = new AnalyzerLanguage("none");
        public static readonly AnalyzerLanguage CSharp = new AnalyzerLanguage(CsLiteral);
        public static readonly AnalyzerLanguage VisualBasic = new AnalyzerLanguage(VbNetLiteral);
        public static readonly AnalyzerLanguage Both = new AnalyzerLanguage("both");
        private readonly string language;

        private AnalyzerLanguage(string language)
        {
            this.language = language;
        }

        public override string ToString()
        {
            return this.language;
        }

        public AnalyzerLanguage AddLanguage(AnalyzerLanguage other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (this == None ||
                this == other)
            {
                return other;
            }

            return Both;
        }

        public bool IsAlso(AnalyzerLanguage other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (other == None)
            {
                throw new NotSupportedException("IsAlso doesn't support AnalyzerLanguage.None.");
            }

            return this == other || this == Both;
        }

        public static AnalyzerLanguage Parse(string language)
        {
            if (language == CsLiteral)
            {
                return CSharp;
            }
            if (language == VbNetLiteral)
            {
                return VisualBasic;
            }

            throw new NotSupportedException($"Supplied language needs to be '{CsLiteral}' or '{VbNetLiteral}', but found: '{language}'.");
        }

        public string RepositoryKey
        {
            get
            {
                if (this == CSharp)
                {
                    return "csharpsquid";
                }

                if (this == VisualBasic)
                {
                    return "vbnet";
                }

                throw new NotSupportedException($"Quality profile can only be queried for a single language. But was called on '{ToString()}'.");
            }
        }

        public string DirectoryName
        {
            get
            {
                if (this == CSharp)
                {
                    return "CSharp";
                }

                if (this == VisualBasic)
                {
                    return "VisualBasic";
                }

                throw new NotSupportedException($"Can't get folder name for '{ToString()}'.");
            }
        }

        public string FriendlyName
        {
            get
            {
                if (this == CSharp)
                {
                    return "C#";
                }

                if (this == VisualBasic)
                {
                    return "VB.NET";
                }

                throw new NotSupportedException($"Can't get friendly name for '{ToString()}'.");
            }
        }

        public string FileExtension
        {
            get
            {
                if (this == CSharp)
                {
                    return "cs";
                }

                if (this == VisualBasic)
                {
                    return "vb";
                }

                throw new NotSupportedException($"Can't get file extension for '{ToString()}'.");
            }
        }

        public static AnalyzerLanguage FromPath(string path)
        {
            switch (System.IO.Path.GetExtension(path).ToUpperInvariant())
            {
                case ".CS":
                    return CSharp;
                case ".VB":
                    return VisualBasic;
                default:
                    return None;
            }
        }
    }
}
