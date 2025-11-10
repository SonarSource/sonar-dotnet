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

#if CS
namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;
#else
namespace SonarAnalyzer.VisualBasic.Core.Syntax.Extensions;
#endif

public static class SyntaxNodeExtensionsShared
{
    public static bool ContainsGetOrSetOnDependencyProperty(this SyntaxNode node, Compilation compilation)
    {
        var model = compilation.GetSemanticModel(node.SyntaxTree);
        // Ignore the accessor if it calls System.Windows.DependencyObject.GetValue or System.Windows.DependencyObject.SetValue
        return node
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(x => x.Expression.NameIs("GetValue") || x.Expression.NameIs("SetValue"))
            .Any(x => model.GetSymbolInfo(x).Symbol.ContainingType.DerivesFrom(KnownType.System_Windows_DependencyObject));
    }

    public static IEnumerable<StatementSyntax> GetPreviousStatementsCurrentBlock(this SyntaxNode expression)
    {
        var statement = expression.FirstAncestorOrSelf<StatementSyntax>();
        return statement is null ? [] : statement.Parent.ChildNodes().OfType<StatementSyntax>().TakeWhile(x => x != statement).Reverse();
    }
}
