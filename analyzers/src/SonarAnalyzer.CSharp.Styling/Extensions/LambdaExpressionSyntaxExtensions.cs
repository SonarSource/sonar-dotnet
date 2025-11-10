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

namespace SonarAnalyzer.CSharp.Styling.Extensions;

public static class LambdaExpressionSyntaxExtensions
{
    public static bool IsAnalysisContextAction(this LambdaExpressionSyntax node, SemanticModel model) =>
        model.GetSymbolInfo(node).Symbol is IMethodSymbol { ReturnsVoid: true } symbol
        && IsAnalysisContextName(symbol.Parameters.Single().Type.Name);

    private static bool IsAnalysisContextName(string name) =>
        name.EndsWith("AnalysisContext") || (name.StartsWith("Sonar") && name.EndsWith("ReportingContext"));
}
