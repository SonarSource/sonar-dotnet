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

using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks;

public abstract class RestrictDeserializedTypesBase : SymbolicRuleCheck
{
    protected const string DiagnosticId = "S5773";
    protected const string MessageFormat = "{0}";
    private const string RestrictTypesMessage = "Restrict types of objects allowed to be deserialized.";
    private const string VerifyMacMessage = "Serialized data signature (MAC) should be verified.";

    protected override ProgramState PostProcessSimple(SymbolicContext context)
    {
        var operation = context.Operation.Instance;
        if (operation.Kind == OperationKindEx.ObjectCreation
            && operation.Type.IsAny(KnownType.System_Runtime_Serialization_Formatters_Binary_BinaryFormatter))
        {
            return context.State.SetOperationConstraint(context.Operation, SerializationConstraint.Unsafe);
        }
        else if (operation.AsAssignment() is { } assignment
            && assignment.Target.AsPropertyReference() is { Property.Name: "Binder" } propertyReference
            && propertyReference.Instance.Type.IsAny(KnownType.System_Runtime_Serialization_Formatters_Binary_BinaryFormatter))
        {
            if (context.State[assignment.Value]?.HasConstraint(ObjectConstraint.Null) is not true
                && SemanticModel.GetTypeInfo(assignment.Value.Syntax) is { Type: { } binderType })
            {
                var binderDeclaration = binderType.DeclaringSyntaxReferences
                        .SelectMany(x => x.GetSyntax().DescendantNodes())
                        .OfType<MethodDeclarationSyntax>()
                        .FirstOrDefault(IsBindToType);
                if (binderDeclaration is null || ThrowsOrReturnsNull(binderDeclaration))
                {
                    var state = context.State.SetOperationConstraint(propertyReference.Instance, SerializationConstraint.Safe);
                    if (propertyReference.Instance.TrackedSymbol() is { } targetSymbol)
                    {
                        return state.SetSymbolConstraint(targetSymbol, SerializationConstraint.Safe);
                    }
                    return state;
                }
                else
                {
                    var constraint = SerializationConstraint.Unsafe.WithCause(binderDeclaration.Identifier.GetLocation());
                    var state = context.State.SetOperationConstraint(propertyReference.Instance, constraint);
                    if (propertyReference.Instance.TrackedSymbol() is { } targetSymbol)
                    {
                        return state.SetSymbolConstraint(targetSymbol, constraint);
                    }
                    return state;
                }
            }
        }
        else if (operation.AsInvocation() is { } invocation
            && invocation.TargetMethod.Name == "Deserialize"
            && invocation.TargetMethod.ContainingType.IsAny(KnownType.System_Runtime_Serialization_Formatters_Binary_BinaryFormatter)
            && context.State[invocation.Instance]?.Constraint<SerializationConstraint>() is { Kind: ConstraintKind.SerializationUnsafe } constraint)
        {
            var additionalLocations = constraint.Cause is not null
                ? new[] { constraint.Cause }
                : Array.Empty<Location>();
            ReportIssue(operation, additionalLocations, RestrictTypesMessage);
        }
        return context.State;
    }

    private bool IsBindToType(MethodDeclarationSyntax methodDeclaration) =>
        methodDeclaration.Identifier.Text == "BindToType"
        && methodDeclaration.ParameterList.Parameters.Count == 2
        && methodDeclaration.ParameterList.Parameters[0].Type.IsKnownType(KnownType.System_String, SemanticModel)
        && methodDeclaration.ParameterList.Parameters[1].Type.IsKnownType(KnownType.System_String, SemanticModel);

    protected abstract bool ThrowsOrReturnsNull(MethodDeclarationSyntax methodDeclaration);
}
