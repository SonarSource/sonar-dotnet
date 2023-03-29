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

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    internal static class IOperationExtensions
    {
        internal static ISymbol TrackedSymbol(this IOperation operation) =>
            operation.ToWrapper() switch
            {
                IConversionOperationWrapper conversion => TrackedSymbol(conversion.Operand),
                IFieldReferenceOperationWrapper fieldReference when IsStaticOrThis(fieldReference) => fieldReference.Field,
                ILocalReferenceOperationWrapper localReference => localReference.Local,
                IParameterReferenceOperationWrapper parameterReference => parameterReference.Parameter,
                IArgumentOperationWrapper argument => argument.Value.TrackedSymbol(),
                _ => null
            };

        internal static IInvocationOperationWrapper? AsInvocation(this IOperation operation) =>
            operation.As(OperationKindEx.Invocation, IInvocationOperationWrapper.FromOperation);

        internal static IIsPatternOperationWrapper? AsIsPattern(this IOperation operation) =>
            operation.As(OperationKindEx.IsPattern, IIsPatternOperationWrapper.FromOperation);

        internal static IObjectCreationOperationWrapper? AsObjectCreation(this IOperation operation) =>
            operation.As(OperationKindEx.ObjectCreation, IObjectCreationOperationWrapper.FromOperation);

        internal static IAssignmentOperationWrapper? AsAssignment(this IOperation operation) =>
            operation.As(OperationKindEx.SimpleAssignment, IAssignmentOperationWrapper.FromOperation);

        internal static IPropertyReferenceOperationWrapper? AsPropertyReference(this IOperation operation) =>
            operation.As(OperationKindEx.PropertyReference, IPropertyReferenceOperationWrapper.FromOperation);

        internal static IOperationWrapper ToWrapper(this IOperation operation) =>
            operation?.Kind switch
            {
                OperationKindEx.Invalid => operation.ToInvalid(),
                OperationKindEx.Block => operation.ToBlock(),
                OperationKindEx.VariableDeclarationGroup => operation.ToVariableDeclarationGroup(),
                OperationKindEx.Switch => operation.ToSwitch(),
                OperationKindEx.Loop => operation.ToLoop(),
                OperationKindEx.Labeled => operation.ToLabeled(),
                OperationKindEx.Branch => operation.ToBranch(),
                OperationKindEx.Empty => operation.ToEmpty(),
                OperationKindEx.Return => operation.ToReturn(),
                //OperationKindEx.YieldBreak => operation.ToYieldBreak(),
                OperationKindEx.Lock => operation.ToLock(),
                OperationKindEx.Try => operation.ToTry(),
                OperationKindEx.Using => operation.ToUsing(),
                //OperationKindEx.YieldReturn => operation.ToYieldReturn(),
                OperationKindEx.ExpressionStatement => operation.ToExpressionStatement(),
                OperationKindEx.LocalFunction => operation.ToLocalFunction(),
                OperationKindEx.Stop => operation.ToStop(),
                OperationKindEx.End => operation.ToEnd(),
                OperationKindEx.RaiseEvent => operation.ToRaiseEvent(),
                OperationKindEx.Literal => operation.ToLiteral(),
                OperationKindEx.Conversion => operation.ToConversion(),
                OperationKindEx.Invocation => operation.ToInvocation(),
                OperationKindEx.ArrayElementReference => operation.ToArrayElementReference(),
                OperationKindEx.LocalReference => operation.ToLocalReference(),
                OperationKindEx.ParameterReference => operation.ToParameterReference(),
                OperationKindEx.FieldReference => operation.ToFieldReference(),
                OperationKindEx.MethodReference => operation.ToMethodReference(),
                OperationKindEx.PropertyReference => operation.ToPropertyReference(),
                OperationKindEx.EventReference => operation.ToEventReference(),
                OperationKindEx.Unary => operation.ToUnary(),
                OperationKindEx.Binary => operation.ToBinary(),
                OperationKindEx.Conditional => operation.ToConditional(),
                OperationKindEx.Coalesce => operation.ToCoalesce(),
                OperationKindEx.AnonymousFunction => operation.ToAnonymousFunction(),
                OperationKindEx.ObjectCreation => operation.ToObjectCreation(),
                OperationKindEx.TypeParameterObjectCreation => operation.ToTypeParameterObjectCreation(),
                OperationKindEx.ArrayCreation => operation.ToArrayCreation(),
                OperationKindEx.InstanceReference => operation.ToInstanceReference(),
                OperationKindEx.IsType => operation.ToIsType(),
                OperationKindEx.Await => operation.ToAwait(),
                OperationKindEx.SimpleAssignment => operation.ToSimpleAssignment(),
                OperationKindEx.CompoundAssignment => operation.ToCompoundAssignment(),
                OperationKindEx.Parenthesized => operation.ToParenthesized(),
                OperationKindEx.EventAssignment => operation.ToEventAssignment(),
                OperationKindEx.ConditionalAccess => operation.ToConditionalAccess(),
                OperationKindEx.ConditionalAccessInstance => operation.ToConditionalAccessInstance(),
                OperationKindEx.InterpolatedString => operation.ToInterpolatedString(),
                OperationKindEx.AnonymousObjectCreation => operation.ToAnonymousObjectCreation(),
                OperationKindEx.ObjectOrCollectionInitializer => operation.ToObjectOrCollectionInitializer(),
                OperationKindEx.MemberInitializer => operation.ToMemberInitializer(),
                OperationKindEx.CollectionElementInitializer => operation.ToCollectionElementInitializer(),
                OperationKindEx.NameOf => operation.ToNameOf(),
                OperationKindEx.Tuple => operation.ToTuple(),
                OperationKindEx.DynamicObjectCreation => operation.ToDynamicObjectCreation(),
                OperationKindEx.DynamicMemberReference => operation.ToDynamicMemberReference(),
                OperationKindEx.DynamicInvocation => operation.ToDynamicInvocation(),
                OperationKindEx.DynamicIndexerAccess => operation.ToDynamicIndexerAccess(),
                OperationKindEx.TranslatedQuery => operation.ToTranslatedQuery(),
                OperationKindEx.DelegateCreation => operation.ToDelegateCreation(),
                OperationKindEx.DefaultValue => operation.ToDefaultValue(),
                OperationKindEx.TypeOf => operation.ToTypeOf(),
                OperationKindEx.SizeOf => operation.ToSizeOf(),
                OperationKindEx.AddressOf => operation.ToAddressOf(),
                OperationKindEx.IsPattern => operation.ToIsPattern(),
                OperationKindEx.Increment => operation.ToIncrementOrDecrement(),
                OperationKindEx.Throw => operation.ToThrow(),
                OperationKindEx.Decrement => operation.ToIncrementOrDecrement(),
                OperationKindEx.DeconstructionAssignment => operation.ToDeconstructionAssignment(),
                OperationKindEx.DeclarationExpression => operation.ToDeclarationExpression(),
                OperationKindEx.OmittedArgument => operation.ToOmittedArgument(),
                OperationKindEx.FieldInitializer => operation.ToFieldInitializer(),
                OperationKindEx.VariableInitializer => operation.ToVariableInitializer(),
                OperationKindEx.PropertyInitializer => operation.ToPropertyInitializer(),
                OperationKindEx.ParameterInitializer => operation.ToParameterInitializer(),
                OperationKindEx.ArrayInitializer => operation.ToArrayInitializer(),
                OperationKindEx.VariableDeclarator => operation.ToVariableDeclarator(),
                OperationKindEx.VariableDeclaration => operation.ToVariableDeclaration(),
                OperationKindEx.Argument => operation.ToArgument(),
                OperationKindEx.CatchClause => operation.ToCatchClause(),
                OperationKindEx.SwitchCase => operation.ToSwitchCase(),
                OperationKindEx.CaseClause => operation.ToCaseClause(),
                OperationKindEx.InterpolatedStringText => operation.ToInterpolatedStringText(),
                OperationKindEx.Interpolation => operation.ToInterpolation(),
                OperationKindEx.ConstantPattern => operation.ToConstantPattern(),
                OperationKindEx.DeclarationPattern => operation.ToDeclarationPattern(),
                OperationKindEx.TupleBinary => operation.ToTupleBinary(),
                OperationKindEx.MethodBody => operation.ToMethodBody(),
                OperationKindEx.ConstructorBody => operation.ToConstructorBody(),
                OperationKindEx.Discard => operation.ToDiscard(),
                OperationKindEx.FlowCapture => operation.ToFlowCapture(),
                OperationKindEx.FlowCaptureReference => operation.ToFlowCaptureReference(),
                OperationKindEx.IsNull => operation.ToIsNull(),
                OperationKindEx.CaughtException => operation.ToCaughtException(),
                OperationKindEx.StaticLocalInitializationSemaphore => operation.ToStaticLocalInitializationSemaphore(),
                OperationKindEx.FlowAnonymousFunction => operation.ToFlowAnonymousFunction(),
                OperationKindEx.CoalesceAssignment => operation.ToCoalesceAssignment(),
                OperationKindEx.Range => operation.ToRange(),
                OperationKindEx.ReDim => operation.ToReDim(),
                OperationKindEx.ReDimClause => operation.ToReDimClause(),
                OperationKindEx.RecursivePattern => operation.ToRecursivePattern(),
                OperationKindEx.DiscardPattern => operation.ToDiscardPattern(),
                OperationKindEx.SwitchExpression => operation.ToSwitchExpression(),
                OperationKindEx.SwitchExpressionArm => operation.ToSwitchExpressionArm(),
                OperationKindEx.PropertySubpattern => operation.ToPropertySubpattern(),
                OperationKindEx.UsingDeclaration => operation.ToUsingDeclaration(),
                OperationKindEx.NegatedPattern => operation.ToNegatedPattern(),
                OperationKindEx.BinaryPattern => operation.ToBinaryPattern(),
                OperationKindEx.TypePattern => operation.ToTypePattern(),
                OperationKindEx.RelationalPattern => operation.ToRelationalPattern(),
                OperationKindEx.With => operation.ToWith(),
                _ => null
            };

        internal static IInvalidOperationWrapper ToInvalid(this IOperation operation) =>
            IInvalidOperationWrapper.FromOperation(operation);

        internal static IBlockOperationWrapper ToBlock(this IOperation operation) =>
            IBlockOperationWrapper.FromOperation(operation);

        internal static IVariableDeclarationGroupOperationWrapper ToVariableDeclarationGroup(this IOperation operation) =>
            IVariableDeclarationGroupOperationWrapper.FromOperation(operation);

        internal static ISwitchOperationWrapper ToSwitch(this IOperation operation) =>
            ISwitchOperationWrapper.FromOperation(operation);

        internal static ILoopOperationWrapper ToLoop(this IOperation operation) =>
            ILoopOperationWrapper.FromOperation(operation);

        internal static ILabeledOperationWrapper ToLabeled(this IOperation operation) =>
            ILabeledOperationWrapper.FromOperation(operation);

        internal static IBranchOperationWrapper ToBranch(this IOperation operation) =>
            IBranchOperationWrapper.FromOperation(operation);

        internal static IEmptyOperationWrapper ToEmpty(this IOperation operation) =>
            IEmptyOperationWrapper.FromOperation(operation);

        internal static IReturnOperationWrapper ToReturn(this IOperation operation) =>
            IReturnOperationWrapper.FromOperation(operation);

        //internal static IYieldBreakOperationWrapper ToYieldBreak(this IOperation operation) =>
        //    IYieldBreakOperationWrapper.FromOperation(operation);

        internal static ILockOperationWrapper ToLock(this IOperation operation) =>
            ILockOperationWrapper.FromOperation(operation);

        internal static ITryOperationWrapper ToTry(this IOperation operation) =>
            ITryOperationWrapper.FromOperation(operation);

        internal static IUsingOperationWrapper ToUsing(this IOperation operation) =>
            IUsingOperationWrapper.FromOperation(operation);

        //internal static IYieldReturnOperationWrapper ToYieldReturn(this IOperation operation) =>
        //    IYieldReturnOperationWrapper.FromOperation(operation);

        internal static IExpressionStatementOperationWrapper ToExpressionStatement(this IOperation operation) =>
            IExpressionStatementOperationWrapper.FromOperation(operation);

        internal static ILocalFunctionOperationWrapper ToLocalFunction(this IOperation operation) =>
            ILocalFunctionOperationWrapper.FromOperation(operation);

        internal static IStopOperationWrapper ToStop(this IOperation operation) =>
            IStopOperationWrapper.FromOperation(operation);

        internal static IEndOperationWrapper ToEnd(this IOperation operation) =>
            IEndOperationWrapper.FromOperation(operation);

        internal static IRaiseEventOperationWrapper ToRaiseEvent(this IOperation operation) =>
            IRaiseEventOperationWrapper.FromOperation(operation);

        internal static ILiteralOperationWrapper ToLiteral(this IOperation operation) =>
            ILiteralOperationWrapper.FromOperation(operation);

        internal static IConversionOperationWrapper ToConversion(this IOperation operation) =>
            IConversionOperationWrapper.FromOperation(operation);

        internal static IInvocationOperationWrapper ToInvocation(this IOperation operation) =>
            IInvocationOperationWrapper.FromOperation(operation);

        internal static IArrayElementReferenceOperationWrapper ToArrayElementReference(this IOperation operation) =>
            IArrayElementReferenceOperationWrapper.FromOperation(operation);

        internal static ILocalReferenceOperationWrapper ToLocalReference(this IOperation operation) =>
            ILocalReferenceOperationWrapper.FromOperation(operation);

        internal static IParameterReferenceOperationWrapper ToParameterReference(this IOperation operation) =>
            IParameterReferenceOperationWrapper.FromOperation(operation);

        internal static IFieldReferenceOperationWrapper ToFieldReference(this IOperation operation) =>
            IFieldReferenceOperationWrapper.FromOperation(operation);

        internal static IMethodReferenceOperationWrapper ToMethodReference(this IOperation operation) =>
            IMethodReferenceOperationWrapper.FromOperation(operation);

        internal static IPropertyReferenceOperationWrapper ToPropertyReference(this IOperation operation) =>
            IPropertyReferenceOperationWrapper.FromOperation(operation);

        internal static IEventReferenceOperationWrapper ToEventReference(this IOperation operation) =>
            IEventReferenceOperationWrapper.FromOperation(operation);

        internal static IUnaryOperationWrapper ToUnary(this IOperation operation) =>
            IUnaryOperationWrapper.FromOperation(operation);

        internal static IBinaryOperationWrapper ToBinary(this IOperation operation) =>
            IBinaryOperationWrapper.FromOperation(operation);

        internal static IConditionalOperationWrapper ToConditional(this IOperation operation) =>
            IConditionalOperationWrapper.FromOperation(operation);

        internal static ICoalesceOperationWrapper ToCoalesce(this IOperation operation) =>
            ICoalesceOperationWrapper.FromOperation(operation);

        internal static IAnonymousFunctionOperationWrapper ToAnonymousFunction(this IOperation operation) =>
            IAnonymousFunctionOperationWrapper.FromOperation(operation);

        internal static IObjectCreationOperationWrapper ToObjectCreation(this IOperation operation) =>
            IObjectCreationOperationWrapper.FromOperation(operation);

        internal static ITypeParameterObjectCreationOperationWrapper ToTypeParameterObjectCreation(this IOperation operation) =>
            ITypeParameterObjectCreationOperationWrapper.FromOperation(operation);

        internal static IArrayCreationOperationWrapper ToArrayCreation(this IOperation operation) =>
            IArrayCreationOperationWrapper.FromOperation(operation);

        internal static IInstanceReferenceOperationWrapper ToInstanceReference(this IOperation operation) =>
            IInstanceReferenceOperationWrapper.FromOperation(operation);

        internal static IIsTypeOperationWrapper ToIsType(this IOperation operation) =>
            IIsTypeOperationWrapper.FromOperation(operation);

        internal static IAwaitOperationWrapper ToAwait(this IOperation operation) =>
            IAwaitOperationWrapper.FromOperation(operation);

        internal static ISimpleAssignmentOperationWrapper ToSimpleAssignment(this IOperation operation) =>
            ISimpleAssignmentOperationWrapper.FromOperation(operation);

        internal static ICompoundAssignmentOperationWrapper ToCompoundAssignment(this IOperation operation) =>
            ICompoundAssignmentOperationWrapper.FromOperation(operation);

        internal static IParenthesizedOperationWrapper ToParenthesized(this IOperation operation) =>
            IParenthesizedOperationWrapper.FromOperation(operation);

        internal static IEventAssignmentOperationWrapper ToEventAssignment(this IOperation operation) =>
            IEventAssignmentOperationWrapper.FromOperation(operation);

        internal static IConditionalAccessOperationWrapper ToConditionalAccess(this IOperation operation) =>
            IConditionalAccessOperationWrapper.FromOperation(operation);

        internal static IConditionalAccessInstanceOperationWrapper ToConditionalAccessInstance(this IOperation operation) =>
            IConditionalAccessInstanceOperationWrapper.FromOperation(operation);

        internal static IInterpolatedStringOperationWrapper ToInterpolatedString(this IOperation operation) =>
            IInterpolatedStringOperationWrapper.FromOperation(operation);

        internal static IAnonymousObjectCreationOperationWrapper ToAnonymousObjectCreation(this IOperation operation) =>
            IAnonymousObjectCreationOperationWrapper.FromOperation(operation);

        internal static IObjectOrCollectionInitializerOperationWrapper ToObjectOrCollectionInitializer(this IOperation operation) =>
            IObjectOrCollectionInitializerOperationWrapper.FromOperation(operation);

        internal static IMemberInitializerOperationWrapper ToMemberInitializer(this IOperation operation) =>
            IMemberInitializerOperationWrapper.FromOperation(operation);

        internal static ICollectionElementInitializerOperationWrapper ToCollectionElementInitializer(this IOperation operation) =>
            ICollectionElementInitializerOperationWrapper.FromOperation(operation);

        internal static INameOfOperationWrapper ToNameOf(this IOperation operation) =>
            INameOfOperationWrapper.FromOperation(operation);

        internal static ITupleOperationWrapper ToTuple(this IOperation operation) =>
            ITupleOperationWrapper.FromOperation(operation);

        internal static IDynamicObjectCreationOperationWrapper ToDynamicObjectCreation(this IOperation operation) =>
            IDynamicObjectCreationOperationWrapper.FromOperation(operation);

        internal static IDynamicMemberReferenceOperationWrapper ToDynamicMemberReference(this IOperation operation) =>
            IDynamicMemberReferenceOperationWrapper.FromOperation(operation);

        internal static IDynamicInvocationOperationWrapper ToDynamicInvocation(this IOperation operation) =>
            IDynamicInvocationOperationWrapper.FromOperation(operation);

        internal static IDynamicIndexerAccessOperationWrapper ToDynamicIndexerAccess(this IOperation operation) =>
            IDynamicIndexerAccessOperationWrapper.FromOperation(operation);

        internal static ITranslatedQueryOperationWrapper ToTranslatedQuery(this IOperation operation) =>
            ITranslatedQueryOperationWrapper.FromOperation(operation);

        internal static IDelegateCreationOperationWrapper ToDelegateCreation(this IOperation operation) =>
            IDelegateCreationOperationWrapper.FromOperation(operation);

        internal static IDefaultValueOperationWrapper ToDefaultValue(this IOperation operation) =>
            IDefaultValueOperationWrapper.FromOperation(operation);

        internal static ITypeOfOperationWrapper ToTypeOf(this IOperation operation) =>
            ITypeOfOperationWrapper.FromOperation(operation);

        internal static ISizeOfOperationWrapper ToSizeOf(this IOperation operation) =>
            ISizeOfOperationWrapper.FromOperation(operation);

        internal static IAddressOfOperationWrapper ToAddressOf(this IOperation operation) =>
            IAddressOfOperationWrapper.FromOperation(operation);

        internal static IIsPatternOperationWrapper ToIsPattern(this IOperation operation) =>
            IIsPatternOperationWrapper.FromOperation(operation);

        internal static IIncrementOrDecrementOperationWrapper ToIncrementOrDecrement(this IOperation operation) =>
            IIncrementOrDecrementOperationWrapper.FromOperation(operation);

        internal static IThrowOperationWrapper ToThrow(this IOperation operation) =>
            IThrowOperationWrapper.FromOperation(operation);

        internal static IDeconstructionAssignmentOperationWrapper ToDeconstructionAssignment(this IOperation operation) =>
            IDeconstructionAssignmentOperationWrapper.FromOperation(operation);

        internal static IDeclarationExpressionOperationWrapper ToDeclarationExpression(this IOperation operation) =>
            IDeclarationExpressionOperationWrapper.FromOperation(operation);

        internal static IOmittedArgumentOperationWrapper ToOmittedArgument(this IOperation operation) =>
            IOmittedArgumentOperationWrapper.FromOperation(operation);

        internal static IFieldInitializerOperationWrapper ToFieldInitializer(this IOperation operation) =>
            IFieldInitializerOperationWrapper.FromOperation(operation);

        internal static IVariableInitializerOperationWrapper ToVariableInitializer(this IOperation operation) =>
            IVariableInitializerOperationWrapper.FromOperation(operation);

        internal static IPropertyInitializerOperationWrapper ToPropertyInitializer(this IOperation operation) =>
            IPropertyInitializerOperationWrapper.FromOperation(operation);

        internal static IParameterInitializerOperationWrapper ToParameterInitializer(this IOperation operation) =>
            IParameterInitializerOperationWrapper.FromOperation(operation);

        internal static IArrayInitializerOperationWrapper ToArrayInitializer(this IOperation operation) =>
            IArrayInitializerOperationWrapper.FromOperation(operation);

        internal static IVariableDeclaratorOperationWrapper ToVariableDeclarator(this IOperation operation) =>
            IVariableDeclaratorOperationWrapper.FromOperation(operation);

        internal static IVariableDeclarationOperationWrapper ToVariableDeclaration(this IOperation operation) =>
            IVariableDeclarationOperationWrapper.FromOperation(operation);

        internal static IArgumentOperationWrapper ToArgument(this IOperation operation) =>
            IArgumentOperationWrapper.FromOperation(operation);

        internal static ICatchClauseOperationWrapper ToCatchClause(this IOperation operation) =>
            ICatchClauseOperationWrapper.FromOperation(operation);

        internal static ISwitchCaseOperationWrapper ToSwitchCase(this IOperation operation) =>
            ISwitchCaseOperationWrapper.FromOperation(operation);

        internal static ICaseClauseOperationWrapper ToCaseClause(this IOperation operation) =>
            ICaseClauseOperationWrapper.FromOperation(operation);

        internal static IInterpolatedStringTextOperationWrapper ToInterpolatedStringText(this IOperation operation) =>
            IInterpolatedStringTextOperationWrapper.FromOperation(operation);

        internal static IInterpolationOperationWrapper ToInterpolation(this IOperation operation) =>
            IInterpolationOperationWrapper.FromOperation(operation);

        internal static IConstantPatternOperationWrapper ToConstantPattern(this IOperation operation) =>
            IConstantPatternOperationWrapper.FromOperation(operation);

        internal static IDeclarationPatternOperationWrapper ToDeclarationPattern(this IOperation operation) =>
            IDeclarationPatternOperationWrapper.FromOperation(operation);

        internal static ITupleBinaryOperationWrapper ToTupleBinary(this IOperation operation) =>
            ITupleBinaryOperationWrapper.FromOperation(operation);

        internal static IMethodBodyOperationWrapper ToMethodBody(this IOperation operation) =>
            IMethodBodyOperationWrapper.FromOperation(operation);

        internal static IConstructorBodyOperationWrapper ToConstructorBody(this IOperation operation) =>
            IConstructorBodyOperationWrapper.FromOperation(operation);

        internal static IDiscardOperationWrapper ToDiscard(this IOperation operation) =>
            IDiscardOperationWrapper.FromOperation(operation);

        internal static IFlowCaptureOperationWrapper ToFlowCapture(this IOperation operation) =>
            IFlowCaptureOperationWrapper.FromOperation(operation);

        internal static IFlowCaptureReferenceOperationWrapper ToFlowCaptureReference(this IOperation operation) =>
            IFlowCaptureReferenceOperationWrapper.FromOperation(operation);

        internal static IIsNullOperationWrapper ToIsNull(this IOperation operation) =>
            IIsNullOperationWrapper.FromOperation(operation);

        internal static ICaughtExceptionOperationWrapper ToCaughtException(this IOperation operation) =>
            ICaughtExceptionOperationWrapper.FromOperation(operation);

        internal static IStaticLocalInitializationSemaphoreOperationWrapper ToStaticLocalInitializationSemaphore(this IOperation operation) =>
            IStaticLocalInitializationSemaphoreOperationWrapper.FromOperation(operation);

        internal static IFlowAnonymousFunctionOperationWrapper ToFlowAnonymousFunction(this IOperation operation) =>
            IFlowAnonymousFunctionOperationWrapper.FromOperation(operation);

        internal static ICoalesceAssignmentOperationWrapper ToCoalesceAssignment(this IOperation operation) =>
            ICoalesceAssignmentOperationWrapper.FromOperation(operation);

        internal static IRangeOperationWrapper ToRange(this IOperation operation) =>
            IRangeOperationWrapper.FromOperation(operation);

        internal static IReDimOperationWrapper ToReDim(this IOperation operation) =>
            IReDimOperationWrapper.FromOperation(operation);

        internal static IReDimClauseOperationWrapper ToReDimClause(this IOperation operation) =>
            IReDimClauseOperationWrapper.FromOperation(operation);

        internal static IRecursivePatternOperationWrapper ToRecursivePattern(this IOperation operation) =>
            IRecursivePatternOperationWrapper.FromOperation(operation);

        internal static IDiscardPatternOperationWrapper ToDiscardPattern(this IOperation operation) =>
            IDiscardPatternOperationWrapper.FromOperation(operation);

        internal static ISwitchExpressionOperationWrapper ToSwitchExpression(this IOperation operation) =>
            ISwitchExpressionOperationWrapper.FromOperation(operation);

        internal static ISwitchExpressionArmOperationWrapper ToSwitchExpressionArm(this IOperation operation) =>
            ISwitchExpressionArmOperationWrapper.FromOperation(operation);

        internal static IPropertySubpatternOperationWrapper ToPropertySubpattern(this IOperation operation) =>
            IPropertySubpatternOperationWrapper.FromOperation(operation);

        internal static IUsingDeclarationOperationWrapper ToUsingDeclaration(this IOperation operation) =>
            IUsingDeclarationOperationWrapper.FromOperation(operation);

        internal static INegatedPatternOperationWrapper ToNegatedPattern(this IOperation operation) =>
            INegatedPatternOperationWrapper.FromOperation(operation);

        internal static IBinaryPatternOperationWrapper ToBinaryPattern(this IOperation operation) =>
            IBinaryPatternOperationWrapper.FromOperation(operation);

        internal static ITypePatternOperationWrapper ToTypePattern(this IOperation operation) =>
            ITypePatternOperationWrapper.FromOperation(operation);

        internal static IRelationalPatternOperationWrapper ToRelationalPattern(this IOperation operation) =>
            IRelationalPatternOperationWrapper.FromOperation(operation);

        internal static IWithOperationWrapper ToWith(this IOperation operation) =>
            IWithOperationWrapper.FromOperation(operation);

        public static bool IsStaticOrThis(this IMemberReferenceOperationWrapper reference) =>
            reference.Instance == null // static fields
            || reference.Instance.Kind == OperationKindEx.InstanceReference;

        public static IOperation UnwrapConversion(this IOperation operation)
        {
            while (operation?.Kind == OperationKindEx.Conversion)
            {
                operation = operation.ToConversion().Operand;
            }
            return operation;
        }

        private static T? As<T>(this IOperation operation, OperationKind kind, Func<IOperation, T> fromOperation) where T : struct =>
            operation.Kind == kind ? fromOperation(operation) : null;
    }
}
