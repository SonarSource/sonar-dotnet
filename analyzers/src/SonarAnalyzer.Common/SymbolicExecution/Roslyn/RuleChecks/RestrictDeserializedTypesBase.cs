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

    private static KnownType losFormatter = KnownType.System_Web_UI_LosFormatter;

    private static KnownType[] typesWithDeserializeMethod = formattersWithBinder.Concat(new[] { javaScriptSerializer }).ToArray();

    private Dictionary<ISymbol, SyntaxNode> AdditionalLocationsForSymbols = new Dictionary<ISymbol, SyntaxNode>();
    private Dictionary<IOperation, SyntaxNode> AdditionalLocationsForOperations = new Dictionary<IOperation, SyntaxNode>();

    protected override ProgramState PostProcessSimple(SymbolicContext context)
    {
        var operation = context.Operation.Instance;
        if (operation.Kind == OperationKindEx.ObjectCreation)
        {
            if (operation.Type.IsAny(formattersWithBinder))
            {
                return context.State.SetOperationConstraint(context.Operation, SerializationConstraint.Unsafe);
            }
            else
            {
                var objectCreation = operation.ToObjectCreation();
                if (objectCreation.Type.Is(javaScriptSerializer)
                    && ResolverIsUnsafe(context, objectCreation, out var resolverDeclaration))
                {
                    AdditionalLocationsForOperations[operation] = resolverDeclaration;
                    return context.State.SetOperationConstraint(operation, SerializationConstraint.Unsafe);
                }
                else if (objectCreation.Type.Is(losFormatter)
                    && objectCreation.ArgumentValue("enableMac") is var enableMacArgument
                    && (enableMacArgument is null || context.State[context.State.ResolveCaptureAndUnwrapConversion(enableMacArgument)]?.HasConstraint(BoolConstraint.False) is true))
                {
                    ReportIssue(operation, VerifyMacMessage);
                }
            }
        }
        else if (operation.AsAssignment() is { } assignment)
        {
            if (assignment.Target.AsPropertyReference() is { Property.Name: "Binder", Instance: { } propertyInstance }
            && propertyInstance.Type.IsAny(formattersWithBinder))
            {
                var constraint = BinderIsSafe(context.State, assignment, out var bindToTypeDeclaration)
                    ? SerializationConstraint.Safe
                    : SerializationConstraint.Unsafe;

                if (constraint == SerializationConstraint.Unsafe)
                {
                    AdditionalLocationsForOperations[context.State.ResolveCaptureAndUnwrapConversion(propertyInstance)] = bindToTypeDeclaration;
                }

                var state = context.State.SetOperationConstraint(propertyInstance, constraint);
                if (propertyInstance.TrackedSymbol() is { } symbol)   // TODO ResolveCapture?
                {
                    if (constraint == SerializationConstraint.Unsafe)
                    {
                        AdditionalLocationsForSymbols[symbol] = bindToTypeDeclaration;
                    }
                    return state.SetSymbolConstraint(symbol, constraint);
                }
                else
                {
                    return state;
                }
            }
            else if (AdditionalLocation(context, assignment.Value) is { } methodDeclaration
                && assignment.Target.TrackedSymbol() is { } symbol)
            {
                AdditionalLocationsForSymbols[symbol] = methodDeclaration;
            }
        }
        else if (operation.AsInvocation() is { } invocation
            && invocation.TargetMethod.Name == "Deserialize"
            && invocation.TargetMethod.ContainingType.IsAny(typesWithDeserializeMethod)
            && context.State[invocation.Instance]?.Constraint<SerializationConstraint>() is { Kind: ConstraintKind.SerializationUnsafe } constraint)
        {
            var methodDeclaration = AdditionalLocation(context, invocation.Instance);

            var additionalLocations = methodDeclaration is not null
                ? new[] { GetIdentifier(methodDeclaration).GetLocation() }
                : Array.Empty<Location>();
            ReportIssue(operation, additionalLocations, RestrictTypesMessage);
        }
        return context.State;
    }

    private SyntaxNode AdditionalLocation(SymbolicContext context, IOperation operation)
    {
        operation = context.State.ResolveCaptureAndUnwrapConversion(operation);
        return AdditionalLocationsForOperations.TryGetValue(operation, out var methodDeclaration)
            || (operation.TrackedSymbol() is { } symbol && AdditionalLocationsForSymbols.TryGetValue(symbol, out methodDeclaration))
            ? methodDeclaration
            : null;
    }

    private bool BinderIsSafe(ProgramState state, IAssignmentOperationWrapper assignment, out SyntaxNode bindToTypeDeclaration)
    {
        if (state[assignment.Value]?.HasConstraint(ObjectConstraint.Null) is true)
        {
            bindToTypeDeclaration = null;
            return false;
        }
        bindToTypeDeclaration = SemanticModel.GetTypeInfo(assignment.Value.Syntax).Type?.DeclaringSyntaxReferences
            .SelectMany(x => x.GetSyntax().DescendantNodes())
            .FirstOrDefault(IsBindToType);
        return bindToTypeDeclaration is null || ThrowsOrReturnsNull(bindToTypeDeclaration);
    }

    private bool ResolverIsUnsafe(SymbolicContext context, IObjectCreationOperationWrapper objectCreation, out SyntaxNode resolverDeclaration)
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
                    .FirstOrDefault(IsResolveType) is { } resolverDeclaration2
                && !ThrowsOrReturnsNull(resolverDeclaration2))
            {
                resolverDeclaration = resolverDeclaration2;
                return true;
            }
        }
        return false;
    }

    protected abstract bool IsBindToType(SyntaxNode methodDeclaration);

    protected abstract bool IsResolveType(SyntaxNode methodDeclaration);

    protected abstract bool ThrowsOrReturnsNull(SyntaxNode methodDeclaration);

    protected abstract SyntaxToken GetIdentifier(SyntaxNode methodDeclaration);
}
