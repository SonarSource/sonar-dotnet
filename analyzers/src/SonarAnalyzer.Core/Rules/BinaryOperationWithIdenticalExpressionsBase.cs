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
    public abstract class BinaryOperationWithIdenticalExpressionsBase : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1764";

        protected const string OperatorMessageFormat = "Correct one of the identical expressions on both sides of operator '{0}'.";
        protected const string EqualsMessage = "Change one instance of '{0}' to a different value; comparing '{0}' to itself always returns true.";
    }
}
