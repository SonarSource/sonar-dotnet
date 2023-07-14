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
using SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks;

public abstract class RestrictDeserializedTypesBase : SymbolicRuleCheck
{
    protected const string DiagnosticId = "S5773";
    protected const string MessageFormat = "{0}";
    private const string RestrictTypesMessage = "Restrict types of objects allowed to be deserialized.";
    private const string VerifyMacMessage = "Serialized data signature (MAC) should be verified.";

    private static KnownType[] formattersWithBinder = new[]
    {
        KnownType.System_Runtime_Serialization_Formatters_Binary_BinaryFormatter,
        KnownType.System_Runtime_Serialization_NetDataContractSerializer,
        KnownType.System_Runtime_Serialization_Formatters_Soap_SoapFormatter
    };

    private static KnownType javaScriptSerializer = KnownType.System_Web_Script_Serialization_JavaScriptSerializer;

    private static KnownType[] typesWithDeserializeMethod = formattersWithBinder.Concat(new[] { javaScriptSerializer }).ToArray();

    protected override ProgramState PostProcessSimple(SymbolicContext context)
    {
        var operation = context.Operation.Instance;
        if (operation.Kind == OperationKindEx.ObjectCreation
            && operation.Type.IsAny(formattersWithBinder))
        {
            return context.State.SetOperationConstraint(context.Operation, SerializationConstraint.Unsafe);
        }
        else if (operation.AsObjectCreation() is { } objectCreation
            && operation.Type.Is(javaScriptSerializer)
            && ResolverIsUnsafe(context, objectCreation, out var resolverDeclaration))
        {
            return context.State.SetOperationConstraint(context.Operation, SerializationConstraint.Unsafe.WithCause(resolverDeclaration?.Identifier.GetLocation()));
        }
        else if (operation.AsAssignment() is { } assignment
            && assignment.Target.AsPropertyReference() is { Property.Name: "Binder" } propertyReference
            && propertyReference.Instance.Type.IsAny(formattersWithBinder))
        {
            if (context.State[assignment.Value]?.HasConstraint(ObjectConstraint.Null) is not true
                && SemanticModel.GetTypeInfo(assignment.Value.Syntax) is { Type.DeclaringSyntaxReferences: { } declaringSyntaxReferences })
            {
                var binderDeclaration = declaringSyntaxReferences
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
            && invocation.TargetMethod.ContainingType.IsAny(typesWithDeserializeMethod)
            && context.State[invocation.Instance]?.Constraint<SerializationConstraint>() is { Kind: ConstraintKind.SerializationUnsafe } constraint)
        {
            var additionalLocations = constraint.Cause is not null
                ? new[] { constraint.Cause }
                : Array.Empty<Location>();
            ReportIssue(operation, additionalLocations, RestrictTypesMessage);
        }
        return context.State;
    }

    private bool ResolverIsUnsafe(SymbolicContext context, IObjectCreationOperationWrapper objectCreation, out MethodDeclarationSyntax resolverDeclaration)
    {
        resolverDeclaration = null;
        foreach (var argument in objectCreation.Arguments.Select(x => x.ToArgument().Value))
        {
            if (context.State.ResolveCaptureAndUnwrapConversion(argument).Type.Is(KnownType.System_Web_Script_Serialization_SimpleTypeResolver))
            {
                return true;
            }
            else if (SemanticModel.GetTypeInfo(argument.Syntax) is { Type.DeclaringSyntaxReferences: { } declaringSyntaxReferences }
                && declaringSyntaxReferences
                    .SelectMany(x => x.GetSyntax().DescendantNodes())
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault(IsResolveType) is { } resolverDeclaration2
                && !ThrowsOrReturnsNull(resolverDeclaration2))
            {
                resolverDeclaration = resolverDeclaration2;
                return true;
            }
        }
        return false;
    }

    private bool IsBindToType(MethodDeclarationSyntax methodDeclaration) =>
        methodDeclaration.Identifier.Text == "BindToType"
        && methodDeclaration.ParameterList.Parameters.Count == 2
        && methodDeclaration.ParameterList.Parameters[0].Type.IsKnownType(KnownType.System_String, SemanticModel)
        && methodDeclaration.ParameterList.Parameters[1].Type.IsKnownType(KnownType.System_String, SemanticModel);

    private bool IsResolveType(MethodDeclarationSyntax methodDeclaration) =>
        methodDeclaration.Identifier.Text == "ResolveType"
        && methodDeclaration.ParameterList.Parameters.Count == 1
        && methodDeclaration.ParameterList.Parameters[0].Type.IsKnownType(KnownType.System_String, SemanticModel);

    protected abstract bool ThrowsOrReturnsNull(MethodDeclarationSyntax methodDeclaration);
}
