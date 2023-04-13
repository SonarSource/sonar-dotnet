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

using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks;

public abstract class PublicMethodArgumentsShouldBeCheckedForNullBase : SymbolicRuleCheck
{
    protected const string DiagnosticId = "S3900";
    protected const string MessageFormat = "{0}";

    protected abstract bool IsInConstructorInitializer(SyntaxNode node);
    protected abstract string NullName { get; }

    protected override ProgramState PreProcessSimple(SymbolicContext context)
    {
        if (NullDereferenceCandidate(context.Operation.Instance) is { } candidate
            && candidate.Kind == OperationKindEx.ParameterReference
            && candidate.ToParameterReference() is { Parameter: { Type.IsValueType: false } parameter } parameterReference
            && !HasObjectConstraint(parameter)
            && !context.LiveVariableAnalysis.CapturedVariables.Contains(parameter) // Workaround to avoid FPs. Can be removed once captures are properly handled by lva.LiveOut()
            && !parameter.HasAttribute(KnownType.Microsoft_AspNetCore_Mvc_FromServicesAttribute))
        {
            var message = IsInConstructorInitializer(context.Operation.Instance.Syntax)
                ? "Refactor this constructor to avoid using members of parameter '{0}' because it could be {1}."
                : "Refactor this method to add validation of parameter '{0}' before using it.";
            ReportIssue(parameterReference.WrappedOperation, string.Format(message, parameterReference.WrappedOperation.Syntax, NullName), context);
        }

        return context.State;

        bool HasObjectConstraint(IParameterSymbol symbol) =>
            context.State[symbol]?.HasConstraint<ObjectConstraint>() is true;
    }

    private static IOperation NullDereferenceCandidate(IOperation operation)
    {
        var candidate = operation.Kind switch
        {
            OperationKindEx.Invocation => operation.ToInvocation().Instance,
            OperationKindEx.FieldReference => operation.ToFieldReference().Instance,
            OperationKindEx.PropertyReference => operation.ToPropertyReference().Instance,
            OperationKindEx.EventReference => operation.ToEventReference().Instance,
            OperationKindEx.Await => operation.ToAwait().Operation,
            OperationKindEx.ArrayElementReference => operation.ToArrayElementReference().ArrayReference,
            OperationKindEx.MethodReference => operation.ToMethodReference().Instance,
            _ => null,
        };
        return candidate?.UnwrapConversion();
    }
}
