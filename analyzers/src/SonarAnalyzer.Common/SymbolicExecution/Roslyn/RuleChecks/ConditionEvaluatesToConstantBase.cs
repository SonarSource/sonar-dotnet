/*
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

using System.Net;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks;

public abstract class ConditionEvaluatesToConstantBase : SymbolicRuleCheck
{
    protected const string DiagnosticIdCodeSmell = "S2589"; // Code smell
    protected const string DiagnosticIdBug = "S2583"; // Bug
    protected const string MessageFormat = "{0}";
    protected const string MessageFormatBool = "Change this condition so that it does not always evaluate to '{0}'.";
    protected const string MessageNull = "Change this expression which always evaluates to 'null'.";

    private HashSet<IOperation> trueOperations = new();
    private HashSet<IOperation> falseOperations = new();
    private HashSet<IOperation> unknownOperations = new();

    protected override ProgramState PostProcessSimple(SymbolicContext context)
    {
        var operation = context.Operation.Instance;
        if (operation.Kind == OperationKindEx.Binary)
        {
            int i = 2;
        }
        return base.PostProcessSimple(context);
    }

    public override ProgramState ConditionEvaluated(SymbolicContext context)
    {
        var operation = context.Operation.Instance;
        switch (context.State[operation].Constraint<BoolConstraint>().Kind)
        {
            case ConstraintKind.True:
                trueOperations.Add(operation);
                break;
            case ConstraintKind.False:
                falseOperations.Add(operation);
                break;
            default:
                unknownOperations.Add(operation);
                break;
        }
        return base.ConditionEvaluated(context);
    }

    public override void ExecutionCompleted()
    {
        var alwaysTrueOps = trueOperations.Except(falseOperations);
        var alwaysFalseOps = falseOperations.Except(trueOperations);

        foreach (var constantTrue in alwaysTrueOps)
        {
            ReportIssue(constantTrue);
        }
        foreach (var constantFalse in alwaysFalseOps)
        {
            ReportIssue(constantFalse);
        }

        base.ExecutionCompleted();
    }
}
