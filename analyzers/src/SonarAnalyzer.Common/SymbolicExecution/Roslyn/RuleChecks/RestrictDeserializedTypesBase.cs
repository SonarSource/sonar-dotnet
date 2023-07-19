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

    private static readonly KnownType[] FormattersWithBinder = new[]
    {
        KnownType.System_Runtime_Serialization_Formatters_Binary_BinaryFormatter,
        KnownType.System_Runtime_Serialization_NetDataContractSerializer,
        KnownType.System_Runtime_Serialization_Formatters_Soap_SoapFormatter
    };
    private static readonly KnownType JavaScriptSerializer = KnownType.System_Web_Script_Serialization_JavaScriptSerializer;
    private static readonly KnownType LosFormatter = KnownType.System_Web_UI_LosFormatter;
    private static readonly KnownType[] TypesWithDeserializeMethod = FormattersWithBinder.Concat(new[] { JavaScriptSerializer }).ToArray();

    private readonly Dictionary<ISymbol, SyntaxNode> additionalLocationsForSymbols = new();
    private readonly Dictionary<IOperation, SyntaxNode> additionalLocationsForOperations = new();

    protected override ProgramState PostProcessSimple(SymbolicContext context)
    {
        var operation = context.Operation.Instance;
        if (operation.Kind == OperationKindEx.ObjectCreation)
        {
            if (operation.Type.IsAny(FormattersWithBinder))
            {
                return context.State.SetOperationConstraint(context.Operation, SerializationConstraint.Unsafe);
            }
            else
            {
                var objectCreation = operation.ToObjectCreation();
                if (objectCreation.Type.Is(JavaScriptSerializer)
                    && ResolverIsUnsafe(context, objectCreation, out var resolveTypeDeclaration))
                {
                    additionalLocationsForOperations[operation] = resolveTypeDeclaration;
                    return context.State.SetOperationConstraint(operation, SerializationConstraint.Unsafe);
                }
                else if (UnsafeLosFormatter(context, objectCreation))
                {
                    ReportIssue(operation, VerifyMacMessage);
                }
            }
        }
        else if (operation.AsAssignment() is { } assignment)
        {
            if (context.State.ResolveCaptureAndUnwrapConversion(assignment.Target).AsPropertyReference() is { Property.Name: "Binder", Instance: { } propertyInstance }
                && propertyInstance.Type.IsAny(FormattersWithBinder))
            {
                propertyInstance = context.State.ResolveCaptureAndUnwrapConversion(propertyInstance);
                var constraint = BinderIsSafe(context.State, assignment, out var bindToTypeDeclaration)
                    ? SerializationConstraint.Safe
                    : SerializationConstraint.Unsafe;

                if (constraint == SerializationConstraint.Unsafe)
                {
                    additionalLocationsForOperations[propertyInstance] = bindToTypeDeclaration;
                }
                var state = context.State.SetOperationConstraint(propertyInstance, constraint);

                if (propertyInstance.TrackedSymbol() is { } symbol)
                {
                    if (constraint == SerializationConstraint.Unsafe)
                    {
                        additionalLocationsForSymbols[symbol] = bindToTypeDeclaration;
                    }
                    return state.SetSymbolConstraint(symbol, constraint);
                }
                return state;
            }
            else if (AdditionalLocation(context, assignment.Value) is { } methodDeclaration
                && assignment.Target.TrackedSymbol() is { } symbol)
            {
                additionalLocationsForSymbols[symbol] = methodDeclaration;
            }
        }
        else if (operation.AsInvocation() is { } invocation
            && invocation.TargetMethod.Name == "Deserialize"
            && invocation.Instance.Type.IsAny(TypesWithDeserializeMethod)
            && context.State[invocation.Instance]?.HasConstraint(SerializationConstraint.Unsafe) is true)
        {
            var methodDeclaration = AdditionalLocation(context, invocation.Instance);
            var additionalLocations = methodDeclaration is not null
                ? new[] { GetIdentifier(methodDeclaration).GetLocation() }
                : Array.Empty<Location>();
            ReportIssue(operation, additionalLocations, RestrictTypesMessage);
        }
        return context.State;
    }

    private static bool UnsafeLosFormatter(SymbolicContext context, IObjectCreationOperationWrapper objectCreation) =>
        objectCreation.Type.Is(LosFormatter)
        && objectCreation.ArgumentValue("enableMac") switch
        {
            null => true,
            { } enableMacArgument => context.State[context.State.ResolveCaptureAndUnwrapConversion(enableMacArgument)]?.HasConstraint(BoolConstraint.True) is not true
        };

    private SyntaxNode AdditionalLocation(SymbolicContext context, IOperation operation)
    {
        operation = context.State.ResolveCaptureAndUnwrapConversion(operation);
        return additionalLocationsForOperations.TryGetValue(operation, out var methodDeclaration)
            || (operation.TrackedSymbol() is { } symbol && additionalLocationsForSymbols.TryGetValue(symbol, out methodDeclaration))
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
        else
        {
            bindToTypeDeclaration = DeclarationCandidates(assignment.Value).FirstOrDefault(IsBindToType);
            return bindToTypeDeclaration is null || ThrowsOrReturnsNull(bindToTypeDeclaration);
        }
    }

    private bool ResolverIsUnsafe(SymbolicContext context, IObjectCreationOperationWrapper objectCreation, out SyntaxNode resolveTypeDeclaration)
    {
        resolveTypeDeclaration = null;
        foreach (var argument in objectCreation.Arguments.Select(x => context.State.ResolveCaptureAndUnwrapConversion(x.ToArgument().Value)))
        {
            if (argument.Type.Is(KnownType.System_Web_Script_Serialization_SimpleTypeResolver))
            {
                return true;
            }
            else if (DeclarationCandidates(argument)?.FirstOrDefault(IsResolveType) is { } declaration
                && !ThrowsOrReturnsNull(declaration))
            {
                resolveTypeDeclaration = declaration;
                return true;
            }
        }
        return false;
    }

    private IEnumerable<SyntaxNode> DeclarationCandidates(IOperation operation) =>
        SemanticModel.GetTypeInfo(operation.Syntax).Type?.DeclaringSyntaxReferences.SelectMany(x => x.GetSyntax().DescendantNodes());

    protected abstract bool IsBindToType(SyntaxNode methodDeclaration);

    protected abstract bool IsResolveType(SyntaxNode methodDeclaration);

    protected abstract bool ThrowsOrReturnsNull(SyntaxNode methodDeclaration);

    protected abstract SyntaxToken GetIdentifier(SyntaxNode methodDeclaration);
}
