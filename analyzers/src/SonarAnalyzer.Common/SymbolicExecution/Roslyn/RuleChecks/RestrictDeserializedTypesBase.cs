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
using SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks;

public abstract class RestrictDeserializedTypesBase : SymbolicRuleCheck
{
    protected const string DiagnosticId = "S5773";
    protected const string MessageFormat = "{0}";
    private const string RestrictTypesMessage = "Restrict types of objects allowed to be deserialized.";
    private const string VerifyMacMessage = "Serialized data signature (MAC) should be verified.";

    private static readonly KnownType[] FormattersWithBinder = new[]
    {
        KnownType.System_Runtime_Serialization_Formatters_Binary_BinaryFormatter,
        KnownType.System_Runtime_Serialization_NetDataContractSerializer,
        KnownType.System_Runtime_Serialization_Formatters_Soap_SoapFormatter
    };
    private static readonly KnownType JavaScriptSerializer = KnownType.System_Web_Script_Serialization_JavaScriptSerializer;
    private static readonly KnownType LosFormatter = KnownType.System_Web_UI_LosFormatter;
    private static readonly KnownType[] TypesWithDeserializeMethod = FormattersWithBinder.Append(JavaScriptSerializer).ToArray();

    private readonly Dictionary<ISymbol, SyntaxNode> additionalLocationsForSymbols = new();
    private readonly Dictionary<IOperation, SyntaxNode> additionalLocationsForOperations = new();

    protected abstract bool IsBindToType(SyntaxNode methodDeclaration);
    protected abstract bool IsResolveType(SyntaxNode methodDeclaration);
    protected abstract bool ThrowsOrReturnsNull(SyntaxNode methodDeclaration);
    protected abstract SyntaxToken GetIdentifier(SyntaxNode methodDeclaration);

    protected override ProgramState PostProcessSimple(SymbolicContext context)
    {
        var state = context.State;
        var operation = context.Operation.Instance;
        if (operation.Kind == OperationKindEx.ObjectCreation)
        {
            return operation.Type.IsAny(FormattersWithBinder)
                ? state.SetOperationConstraint(operation, SerializationConstraint.Unsafe)
                : ProcessOtherSerializerCreations(state, operation.ToObjectCreation());
        }
        else if (operation.AsAssignment() is { } assignment)
        {
            if (ProcessBinderAssignment(state, assignment) is { } binderProcessedState)
            {
                return binderProcessedState;
            }
            else if (AdditionalLocation(state, assignment.Value) is { } methodDeclaration
                && assignment.Target.TrackedSymbol() is { } symbol)
            {
                additionalLocationsForSymbols[symbol] = methodDeclaration;
            }
        }
        else if (UnsafeDeserialization(state, operation) is { } invocation)
        {
            var methodDeclaration = AdditionalLocation(state, invocation.Instance);
            var additionalLocations = methodDeclaration is not null
                ? new[] { GetIdentifier(methodDeclaration).GetLocation() }
                : Array.Empty<Location>();
            ReportIssue(operation, additionalLocations, RestrictTypesMessage);
        }
        return state;
    }

    private ProgramState ProcessOtherSerializerCreations(ProgramState state, IObjectCreationOperationWrapper objectCreation)
    {
        if (UnsafeJavaScriptSerializer(state, objectCreation, out var resolveTypeDeclaration))
        {
            additionalLocationsForOperations[objectCreation.WrappedOperation] = resolveTypeDeclaration;
            return state.SetOperationConstraint(objectCreation.WrappedOperation, SerializationConstraint.Unsafe);
        }
        else if (UnsafeLosFormatter(state, objectCreation))
        {
            ReportIssue(objectCreation.WrappedOperation, VerifyMacMessage);
        }
        return state;
    }

    private bool UnsafeJavaScriptSerializer(ProgramState state, IObjectCreationOperationWrapper objectCreation, out SyntaxNode resolveTypeDeclaration)
    {
        resolveTypeDeclaration = null;
        return objectCreation.Type.Is(JavaScriptSerializer)
            && objectCreation.Arguments.Length == 1
            && UnsafeResolver(state, objectCreation.Arguments[0].ToArgument().Value, out resolveTypeDeclaration);
    }

    private bool UnsafeResolver(ProgramState state, IOperation operation, out SyntaxNode resolveTypeDeclaration)
    {
        resolveTypeDeclaration = null;
        if (state.ResolveCaptureAndUnwrapConversion(operation).Type.Is(KnownType.System_Web_Script_Serialization_SimpleTypeResolver))
        {
            return true;
        }
        else if (DeclarationCandidates(operation)?.FirstOrDefault(IsResolveType) is { } declaration
            && !ThrowsOrReturnsNull(declaration))
        {
            resolveTypeDeclaration = declaration;
            return true;
        }
        else
        {
            return false;
        }
    }

    private static bool UnsafeLosFormatter(ProgramState state, IObjectCreationOperationWrapper objectCreation) =>
        objectCreation.Type.Is(LosFormatter)
        && objectCreation.ArgumentValue("enableMac") switch
        {
            null => true,
            { } enableMacArgument => state[enableMacArgument]?.HasConstraint(BoolConstraint.True) is not true
        };

    private ProgramState ProcessBinderAssignment(ProgramState state, IAssignmentOperationWrapper assignment)
    {
        if (BinderAssignmentInstance(state, assignment) is { } instance)
        {
            var constraint = BinderIsSafe(state, assignment, out var bindToTypeDeclaration)
                ? SerializationConstraint.Safe
                : SerializationConstraint.Unsafe;
            if (constraint == SerializationConstraint.Unsafe)
            {
                additionalLocationsForOperations[instance] = bindToTypeDeclaration;
            }
            state = state.SetOperationConstraint(instance, constraint);

            if (instance.TrackedSymbol() is { } symbol)
            {
                if (constraint == SerializationConstraint.Unsafe)
                {
                    additionalLocationsForSymbols[symbol] = bindToTypeDeclaration;
                }
                state = state.SetSymbolConstraint(symbol, constraint);
            }
            return state;
        }
        return null;
    }

    private static IOperation BinderAssignmentInstance(ProgramState state, IAssignmentOperationWrapper assignment) =>
        state.ResolveCaptureAndUnwrapConversion(assignment.Target).AsPropertyReference() is { Property.Name: "Binder", Instance: { } propertyInstance }
        && propertyInstance.Type.IsAny(FormattersWithBinder)
            ? state.ResolveCaptureAndUnwrapConversion(propertyInstance)
            : null;

    private bool BinderIsSafe(ProgramState state, IAssignmentOperationWrapper assignment, out SyntaxNode bindToTypeDeclaration)
    {
        if (state[assignment.Value]?.HasConstraint(ObjectConstraint.Null) is true)
        {
            bindToTypeDeclaration = null;
            return false;
        }
        else
        {
            bindToTypeDeclaration = DeclarationCandidates(state.ResolveCaptureAndUnwrapConversion(assignment.Value)).FirstOrDefault(IsBindToType);
            return bindToTypeDeclaration is null || ThrowsOrReturnsNull(bindToTypeDeclaration);
        }
    }

    private IEnumerable<SyntaxNode> DeclarationCandidates(IOperation operation) =>
        SemanticModel.GetTypeInfo(operation.Syntax).Type?.DeclaringSyntaxReferences.SelectMany(x => x.GetSyntax().ChildNodes());

    private SyntaxNode AdditionalLocation(ProgramState state, IOperation operation)
    {
        operation = state.ResolveCaptureAndUnwrapConversion(operation);
        return additionalLocationsForOperations.TryGetValue(operation, out var methodDeclaration)
            || (operation.TrackedSymbol() is { } symbol && additionalLocationsForSymbols.TryGetValue(symbol, out methodDeclaration))
            ? methodDeclaration
            : null;
    }

    private static IInvocationOperationWrapper? UnsafeDeserialization(ProgramState state, IOperation operation) =>
        operation.AsInvocation() is { } invocation
        && invocation.TargetMethod.Name == "Deserialize"
        && invocation.TargetMethod.ContainingType.IsAny(TypesWithDeserializeMethod)
        && state[invocation.Instance]?.HasConstraint(SerializationConstraint.Unsafe) is true
            ? invocation : null;
}
