﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks;

public abstract class InvalidCastToInterfaceBase : SymbolicRuleCheck
{
    protected const string DiagnosticId = "S1944";
    protected const string MessageFormat = "{0}";
    private const string MessageNullable = "Nullable is known to be empty, this cast throws an exception.";

    protected override ProgramState PreProcessSimple(SymbolicContext context)
    {
        if (context.Operation.Instance.AsConversion() is { } conversion
            && conversion.IsBuildIn()
            && conversion.Operand.Type.IsNullableValueType()
            && !conversion.Type.IsNullableValueType()
            && !conversion.Type.Is(KnownType.System_Object)
            && context.State[conversion.Operand]?.HasConstraint(ObjectConstraint.Null) is true)
        {
            ReportIssue(context.Operation, MessageNullable);
        }
        return context.State;
    }
}
