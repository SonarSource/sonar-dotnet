/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.TestFramework.Verification.IssueValidation;

internal sealed class FileContent
{
    public string FileName { get; }
    public SourceText Content { get; }

    public FileContent(string fileName) : this(fileName, SourceText.From(System.IO.File.ReadAllText(fileName))) { }

    public FileContent(SyntaxTree tree) : this(tree.FilePath, tree.GetText()) { }

    private FileContent(string fileName, SourceText content)
    {
        FileName = fileName;
        Content = content;
    }
}
