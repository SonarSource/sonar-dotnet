/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class SwitchSectionShouldNotHaveTooManyStatementsBase : ParameterLoadingDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S1151";
        protected const string MessageFormat = "Reduce this {0} number of statements from {1} to at most {2}, for example by extracting code into a {3}.";

        private const int ThresholdDefaultValue = 8;
        [RuleParameter("max", PropertyType.Integer, "Maximum number of statements.", ThresholdDefaultValue)]
        public int Threshold { get; set; } = ThresholdDefaultValue;
    }
}
