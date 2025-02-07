/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Core.Rules
{
    public abstract class DoNotLockOnSharedResourceBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S2551";
        protected const string MessageFormat = "Lock on a dedicated object instance instead.";

        private static readonly ImmutableArray<KnownType> _invalidLockTypes =
           ImmutableArray.Create(
               KnownType.System_String,
               KnownType.System_Type
           );

        protected static bool IsLockOnForbiddenKnownType(SyntaxNode expression, SemanticModel semanticModel) =>
            semanticModel.GetTypeInfo(expression).Type.IsAny(_invalidLockTypes);
    }
}
