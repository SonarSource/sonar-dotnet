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

namespace SonarAnalyzer.CSharp.Styling.Extensions;

internal static class MemberDeclarationSyntaxExtensions
{
    public static OrderDescriptor ComputeOrder(this MemberDeclarationSyntax member)
    {
        if (member.Modifiers.Any(SyntaxKind.ProtectedKeyword))  // protected, protected internal or private protected
        {
            return new(3, "protected");
        }
        else if (member.Modifiers.Any(SyntaxKind.PublicKeyword))
        {
            return new(1, "public");
        }
        else if (member.Modifiers.Any(SyntaxKind.InternalKeyword))
        {
            return new(2, "internal");
        }
        else // private or unspecified
        {
            return new(4, "private");
        }
    }
}
