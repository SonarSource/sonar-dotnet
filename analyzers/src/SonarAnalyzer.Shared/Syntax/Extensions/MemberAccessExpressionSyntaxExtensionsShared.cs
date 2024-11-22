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

#if CS
namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;
#else
namespace SonarAnalyzer.VisualBasic.Core.Syntax.Extensions;
#endif

internal static class MemberAccessExpressionSyntaxExtensionsShared
{
    public static bool IsPtrZero(this MemberAccessExpressionSyntax memberAccess, SemanticModel model) =>
        memberAccess.Name.Identifier.Text == "Zero"
        && model.GetTypeInfo(memberAccess).Type is var type
        && type.IsAny(KnownType.System_IntPtr, KnownType.System_UIntPtr);
}
