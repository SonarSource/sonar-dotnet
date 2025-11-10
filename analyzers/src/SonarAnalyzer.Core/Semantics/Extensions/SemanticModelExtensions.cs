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

namespace SonarAnalyzer.Core.Semantics.Extensions;

public static class SemanticModelExtensions
{
    public static bool IsExtensionMethod(this SemanticModel model, SyntaxNode expression) =>
        model.GetSymbolInfo(expression).Symbol is IMethodSymbol memberSymbol && memberSymbol.IsExtensionMethod;

    public static SemanticModel SemanticModelOrDefault(this SyntaxTree tree, SemanticModel model)
    {
        // See https://github.com/dotnet/roslyn/issues/18730
        if (tree == model.SyntaxTree)
        {
            return model;
        }

        return model.Compilation.ContainsSyntaxTree(tree)
            ? model.Compilation.GetSemanticModel(tree)
            : null;
    }
}
