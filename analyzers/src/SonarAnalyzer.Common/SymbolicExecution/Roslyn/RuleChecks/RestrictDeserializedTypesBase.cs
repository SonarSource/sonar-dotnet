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

using System.Runtime.Serialization;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks;

public abstract class RestrictDeserializedTypesBase : SymbolicRuleCheck
{
    protected const string DiagnosticId = "S5773";
    protected const string MessageFormat = "{0}";
    private const string RestrictTypesMessage = "Restrict types of objects allowed to be deserialized.";
    private const string VerifyMacMessage = "Serialized data signature (MAC) should be verified.";
    private const string SecondaryMessage = "This method allows all types.";

    private static readonly KnownType[] FormattersWithBinderProperty =
    {
        KnownType.System_Runtime_Serialization_Formatters_Binary_BinaryFormatter,
        KnownType.System_Runtime_Serialization_NetDataContractSerializer,
        KnownType.System_Runtime_Serialization_Formatters_Soap_SoapFormatter
    };
    private static readonly KnownType JavaScriptSerializer = KnownType.System_Web_Script_Serialization_JavaScriptSerializer;
    private static readonly KnownType LosFormatter = KnownType.System_Web_UI_LosFormatter;
    private static readonly KnownType[] TypesWithDeserializeMethod = FormattersWithBinderProperty.Append(JavaScriptSerializer).ToArray();

    private readonly Dictionary<ISymbol, SyntaxNode> unsafeMethodsForSymbols = new();
    private readonly Dictionary<IOperation, SyntaxNode> unsafeMethodsForOperations = new();

    protected abstract SyntaxNode FindBindToTypeMethodDeclaration(IOperation operation);
    protected abstract SyntaxNode FindResolveTypeMethodDeclaration(IOperation operation);
    protected abstract bool ThrowsOrReturnsNull(SyntaxNode methodDeclaration);
    protected abstract SyntaxToken GetIdentifier(SyntaxNode methodDeclaration);

    protected override ProgramState PreProcessSimple(SymbolicContext context)
    {
        var state = context.State;
        var operation = context.Operation.Instance;
        if (operation.Kind == OperationKindEx.ObjectCreation)
        {
            return operation.Type.IsAny(FormattersWithBinderProperty)
                ? state.SetOperationConstraint(operation, SerializationConstraint.Unsafe)
                : ProcessOtherSerializerCreations(state, operation.ToObjectCreation());
        }
        else if (operation.AsAssignment() is { } assignment)
        {
            if (ProcessBinderAssignment(state, assignment) is { } binderProcessedState)
            {
                return binderProcessedState;
            }
            else if (UnsafeMethodDeclaration(state, assignment.Value) is { } methodDeclaration
                && assignment.Target.TrackedSymbol(state) is { } symbol)
            {
                // Assignments propagate constraints. The same needs to be done for method declarations.
                // This is especially relevant, when the property is set in an object initializer:
                /// var formatter = new BinaryFormatter { Binder = binder };
                // The constraint will be learned on a FlowCaptureReference and propagated via the assignment.
                unsafeMethodsForSymbols[symbol] = methodDeclaration;
            }
        }
        else if (UnsafeDeserialization(state, operation) is { } invocation)
        {
            var methodDeclaration = UnsafeMethodDeclaration(state, invocation.Instance);
            var additionalLocations = methodDeclaration is null ? [] : new[] { GetIdentifier(methodDeclaration).GetLocation().ToSecondary(SecondaryMessage) };
            ReportIssue(operation, additionalLocations, RestrictTypesMessage);
        }
        return state;
    }

    private ProgramState ProcessOtherSerializerCreations(ProgramState state, IObjectCreationOperationWrapper objectCreation)
    {
        if (UnsafeJavaScriptSerializer(state, objectCreation, out var resolveTypeDeclaration))
        {
            unsafeMethodsForOperations[objectCreation.WrappedOperation] = resolveTypeDeclaration;
            return state.SetOperationConstraint(objectCreation.WrappedOperation, SerializationConstraint.Unsafe);
        }
        else if (objectCreation.Type.Is(LosFormatter) && !EnableMacIsTrue(state, objectCreation))
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
        operation = state.ResolveCaptureAndUnwrapConversion(operation);
        if (operation.Type.Is(KnownType.System_Web_Script_Serialization_SimpleTypeResolver))
        {
            return true;
        }
        else if (FindResolveTypeMethodDeclaration(operation) is { } declaration && !ThrowsOrReturnsNull(declaration))
        {
            resolveTypeDeclaration = declaration;
            return true;
        }
        else
        {
            return false;
        }
    }

    private static bool EnableMacIsTrue(ProgramState state, IObjectCreationOperationWrapper objectCreation) =>
        objectCreation.ArgumentValue("enableMac") is { } enableMacArgument
        && state[enableMacArgument].HasConstraint(BoolConstraint.True) is true;

    private ProgramState ProcessBinderAssignment(ProgramState state, IAssignmentOperationWrapper assignment)
    {
        if (BinderAssignmentInstance(state, assignment) is { } instance)
        {
            var constraint = BinderIsSafe(state, assignment, out var bindToTypeDeclaration)
                ? SerializationConstraint.Safe
                : SerializationConstraint.Unsafe;
            if (constraint == SerializationConstraint.Unsafe)
            {
                unsafeMethodsForOperations[instance] = bindToTypeDeclaration;
            }
            state = state.SetOperationConstraint(instance, constraint);

            if (instance.TrackedSymbol(state) is { } symbol)
            {
                if (constraint == SerializationConstraint.Unsafe)
                {
                    unsafeMethodsForSymbols[symbol] = bindToTypeDeclaration;
                }
                state = state.SetSymbolConstraint(symbol, constraint);
            }
            return state;
        }
        return null;
    }

    private static IOperation BinderAssignmentInstance(ProgramState state, IAssignmentOperationWrapper assignment) =>
        state.ResolveCaptureAndUnwrapConversion(assignment.Target).AsPropertyReference() is { Property.Name: nameof(IFormatter.Binder), Instance: { } propertyInstance }
        && propertyInstance.Type.IsAny(FormattersWithBinderProperty)
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
            bindToTypeDeclaration = FindBindToTypeMethodDeclaration(state.ResolveCaptureAndUnwrapConversion(assignment.Value));
            return bindToTypeDeclaration is null || ThrowsOrReturnsNull(bindToTypeDeclaration);
        }
    }

    private SyntaxNode UnsafeMethodDeclaration(ProgramState state, IOperation operation)
    {
        operation = state.ResolveCaptureAndUnwrapConversion(operation);
        return unsafeMethodsForOperations.TryGetValue(operation, out var methodDeclaration)
            || (operation.TrackedSymbol(state) is { } symbol && unsafeMethodsForSymbols.TryGetValue(symbol, out methodDeclaration))
            ? methodDeclaration
            : null;
    }

    private static IInvocationOperationWrapper? UnsafeDeserialization(ProgramState state, IOperation operation) =>
        operation.AsInvocation() is { } invocation
        && invocation.TargetMethod.Name == nameof(IFormatter.Deserialize)
        && invocation.TargetMethod.ContainingType.IsAny(TypesWithDeserializeMethod)
        && state[invocation.Instance]?.HasConstraint(SerializationConstraint.Unsafe) is true
            ? invocation : null;
}
