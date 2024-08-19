/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
            && candidate.ToParameterReference() is { Parameter: { Type.IsValueType: false } parameter }
            && MissesObjectConstraint(context.State[parameter])
            && !context.CapturedVariables.Contains(parameter) // Workaround to avoid FPs. Can be removed once captures are properly handled by lva.LiveOut()
            && !parameter.HasAttribute(KnownType.Microsoft_AspNetCore_Mvc_FromServicesAttribute)
            && !IsInsideRazorGeneratedCode(candidate))
        {
            var message = IsInConstructorInitializer(candidate.Syntax)
                ? "Refactor this constructor to avoid using members of parameter '{0}' because it could be {1}."
                : "Refactor this method to add validation of parameter '{0}' before using it.";
            ReportIssue(candidate, string.Format(message, candidate.Syntax, NullName));
        }

        return context.State;

        // Checks whether the null state of the parameter symbol is determined or if was assigned a new value after it was passed to the method.
        // In either of those cases the rule will not raise an issue for the parameter.
        static bool MissesObjectConstraint(SymbolicValue symbolState) =>
            symbolState is null
            || (!symbolState.HasConstraint<ObjectConstraint>() && !symbolState.HasConstraint<ParameterReassignedConstraint>());
    }

    protected override ProgramState PostProcessSimple(SymbolicContext context)
    {
        if (AssignmentTarget(context.Operation.Instance) is { Kind: OperationKindEx.ParameterReference } assignedParameter)
        {
            return context.SetSymbolConstraint(assignedParameter.ToParameterReference().Parameter, ParameterReassignedConstraint.Instance);
        }

        return context.State;
    }

    protected bool IsAccessibleFromOtherAssemblies() =>
        SemanticModel.GetDeclaredSymbol(Node).GetEffectiveAccessibility() is Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal;

    private static IOperation NullDereferenceCandidate(IOperation operation)
    {
        var candidate = operation.Kind switch
        {
            // C# extensions have Instance=Null, while VB extensions have it set.
            OperationKindEx.Invocation when operation.ToInvocation() is var invocation && !invocation.TargetMethod.IsExtensionMethod => invocation.Instance,
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

    private static IOperation AssignmentTarget(IOperation operation) =>
        // Missing operation types: DeconstructionAssignment, CoalesceAssignment, ThrowExpression, assignment with flow capture
        operation.Kind == OperationKindEx.SimpleAssignment
            ? operation.ToAssignment().Target
            : null;

    private static bool IsInsideRazorGeneratedCode(IOperation candidate) =>
        GeneratedCodeRecognizer.IsRazorGeneratedFile(candidate.Syntax.SyntaxTree)
        && candidate.Syntax.TryGetInferredMemberName().Equals("__builder", StringComparison.OrdinalIgnoreCase);
}
