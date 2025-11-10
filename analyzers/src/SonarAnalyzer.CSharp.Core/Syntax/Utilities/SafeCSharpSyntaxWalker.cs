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

namespace SonarAnalyzer.CSharp.Core.Syntax.Utilities;

public class SafeCSharpSyntaxWalker : CSharpSyntaxWalker, ISafeSyntaxWalker
{
    protected SafeCSharpSyntaxWalker() { }

    protected SafeCSharpSyntaxWalker(SyntaxWalkerDepth depth) : base(depth) { }

    public bool SafeVisit(SyntaxNode syntaxNode)
    {
        try
        {
            Visit(syntaxNode);
            return true;
        }
        catch (InsufficientExecutionStackException)
        {
            // Roslyn walker overflows the stack when the depth of the call is around 2050.
            // See https://github.com/SonarSource/sonar-dotnet/issues/2115
            return false;
        }
    }
}
