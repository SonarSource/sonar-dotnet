/*
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

namespace SonarAnalyzer.Helpers;

internal static class CSharpDebugOnlyCodeHelper
{
    public static bool IsDebugString(string text) =>
        text == "DEBUG";

    public static bool IsInDebugBlock(this SyntaxNode node) =>
        CSharpIfDirectiveHelper.ActiveConditionalCompilationSections(node).Any(IsDebugString);

    public static bool IsCallerInConditionalDebug(SyntaxNode node, SemanticModel model)
    {
        var method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>() is { } containingMethod
            ? model.GetDeclaredSymbol(containingMethod)
            : null;
        return IsConditionalDebugMethod(method);
    }

    public static bool IsConditionalDebugMethod(this IMethodSymbol methodSymbol)
    {
        if (methodSymbol is null)
        {
            return false;
        }

        // Conditional attribute can be applied to a class, but it does nothing unless
        // the class is an attribute class. So we only need to worry about whether the
        // conditional attribute is on the method.
        return methodSymbol.GetAttributes(KnownType.System_Diagnostics_ConditionalAttribute)
            .Any(x => x.ConstructorArguments.Any(constructorArg => constructorArg.Type.Is(KnownType.System_String) && IsDebugString((string)constructorArg.Value)));
    }
}
