﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using System.IO;

namespace SonarAnalyzer.Core.Syntax.Utilities;

public abstract class GeneratedCodeRecognizer
{
    private static readonly ImmutableArray<string> GeneratedFileParts = ImmutableArray.Create(
        ".g.",
        ".generated.",
        ".designer.",
        "_generated.",
        "temporarygeneratedfile_",
        ".assemblyattributes.vb"); // The C# version of this file can already be detected because it contains special comments

    private static readonly ImmutableArray<string> AutoGeneratedCommentParts = ImmutableArray.Create(
        "<auto-generated",
        "<autogenerated",
        "generated by");

    private static readonly ImmutableArray<string> GeneratedCodeAttributes = ImmutableArray.Create(
        "DebuggerNonUserCode",
        "DebuggerNonUserCodeAttribute",
        "GeneratedCode",
        "GeneratedCodeAttribute",
        "CompilerGenerated",
        "CompilerGeneratedAttribute");

    protected abstract bool IsTriviaComment(SyntaxTrivia trivia);
    protected abstract string GetAttributeName(SyntaxNode node);

    public bool IsGenerated(SyntaxTree tree) =>
         !string.IsNullOrEmpty(tree.FilePath)
         && (HasGeneratedFileName(tree) || HasGeneratedCommentOrAttribute(tree));

    public static bool IsRazorGeneratedFile(SyntaxTree tree) =>
        tree is not null && (IsRazor(tree) || IsCshtml(tree));

    public static bool IsRazor(SyntaxTree tree) =>
        // razor.ide.g.cs is the extension for razor-generated files in the context of design-time builds.
        // However, it is not considered here because of https://github.com/dotnet/razor/issues/9108
        tree.FilePath.EndsWith("razor.g.cs", StringComparison.OrdinalIgnoreCase);

    public static bool IsCshtml(SyntaxTree tree) =>
        // cshtml.ide.g.cs is the extension for razor-generated files in the context of design-time builds.
        // However, it is not considered here because of https://github.com/dotnet/razor/issues/9108
        tree.FilePath.EndsWith("cshtml.g.cs", StringComparison.OrdinalIgnoreCase);

    private bool HasGeneratedCommentOrAttribute(SyntaxTree tree)
    {
        var root = tree.GetRoot();
        if (root is null)
        {
            return false;
        }
        return HasAutoGeneratedComment(root) || HasGeneratedCodeAttribute(root);
    }

    private bool HasAutoGeneratedComment(SyntaxNode root)
    {
        var firstToken = root.GetFirstToken(true);

        if (!firstToken.HasLeadingTrivia)
        {
            return false;
        }

        return firstToken.LeadingTrivia
            .Where(IsTriviaComment)
            .Any(x => AutoGeneratedCommentParts.Any(part => x.ToString().IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0));
    }

    private bool HasGeneratedCodeAttribute(SyntaxNode root)
    {
        var attributeNames = root
            .DescendantNodesAndSelf()
            .Select(GetAttributeName)
            .Where(x => !string.IsNullOrEmpty(x));

        return attributeNames.Any(x => GeneratedCodeAttributes.Any(attribute => x.EndsWith(attribute, StringComparison.Ordinal)));
    }

    private static bool HasGeneratedFileName(SyntaxTree tree)
    {
        var fileName = Path.GetFileName(tree.FilePath);
        return GeneratedFileParts.Any(x => fileName.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0);
    }
}
