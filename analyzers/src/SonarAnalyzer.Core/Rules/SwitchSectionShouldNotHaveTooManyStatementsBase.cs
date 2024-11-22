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

namespace SonarAnalyzer.Rules
{
    public abstract class SwitchSectionShouldNotHaveTooManyStatementsBase : ParametrizedDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S1151";
        protected const string MessageFormat = "Reduce this {0} number of statements from {1} to at most {2}, for example by extracting code into a {3}.";

        private const int ThresholdDefaultValue = 8;
        [RuleParameter("max", PropertyType.Integer, "Maximum number of statements.", ThresholdDefaultValue)]
        public int Threshold { get; set; } = ThresholdDefaultValue;
    }
}
