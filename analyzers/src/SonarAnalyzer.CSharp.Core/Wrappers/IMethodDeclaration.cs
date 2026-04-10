/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Core.Wrappers;

public interface IMethodDeclaration
{
    BlockSyntax Body { get; }

    ArrowExpressionClauseSyntax ExpressionBody { get; }

    SyntaxToken Identifier { get; }

    ParameterListSyntax ParameterList { get; }

    public TypeParameterListSyntax TypeParameterList { get; }

    bool HasImplementation { get; }

    bool IsLocal { get; }

    TypeSyntax ReturnType { get; }
}
