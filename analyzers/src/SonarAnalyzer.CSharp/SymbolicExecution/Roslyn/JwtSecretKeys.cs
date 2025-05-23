﻿/*
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

using SonarAnalyzer.Common.Walkers;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;

public sealed class JwtSecretKeys : HardcodedBytesRuleBase
{
    private const string DiagnosticId = "S6781";
    private const string MessageFormat = "JWT secret keys should not be disclosed.";

    public static readonly DiagnosticDescriptor S6781 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    protected override DiagnosticDescriptor Rule => S6781;
    protected override SymbolicConstraint Hardcoded => CryptographicKeyConstraint.StoredUnsafe;
    protected override SymbolicConstraint NotHardcoded => CryptographicKeyConstraint.StoredSafe;

    public override bool ShouldExecute()
    {
        var walker = new Walker();
        walker.SafeVisit(Node);
        return walker.Result;
    }

    protected override ProgramState PreProcessSimple(SymbolicContext context)
    {
        var state = context.State;
        var operation = context.Operation.Instance;

        if (operation.AsArrayCreation() is { } arrayCreation)
        {
            return ProcessArrayCreation(state, arrayCreation);
        }
        else if (operation.AsArrayElementReference() is { } arrayElementReference)
        {
            return ProcessArrayElementReference(state, arrayElementReference);
        }
        else if (operation.AsPropertyReference() is { } propertyReference)
        {
            return ProcessPropertyReference(state, propertyReference);
        }
        else if (operation.AsObjectCreation() is { } objectCreation)
        {
            return ProcessSymmetricSecurityKeyConstructor(state, objectCreation);
        }
        else if (operation.AsInvocation() is { } invocation)
        {
            return ProcessArraySetValue(state, invocation)
                   ?? ProcessArrayInitialize(state, invocation)
                   ?? ProcessStringToBytes(state, invocation)
                   ?? state;
        }

        return state;
    }

    // This has to happen after Assignment Processor, as it gets cleared
    protected override ProgramState PostProcessSimple(SymbolicContext context) =>
        context.Operation.Instance.AsAssignment() is { } assignment
            ? ProcessAssignment(context.State, assignment)
            : context.State;

    private ProgramState ProcessAssignment(ProgramState state, IAssignmentOperationWrapper assignment) =>
        assignment.Value.ConstantValue.HasValue
        && assignment.Target.TrackedSymbol(state) is { } target
            ? state.SetSymbolConstraint(target, Hardcoded)
            : state;

    // IConfiguration _config["key"]
    // ConfigurationManager.AppSettings["key"]
    private static ProgramState ProcessPropertyReference(ProgramState state, IPropertyReferenceOperationWrapper propertyReference)
    {
        if (propertyReference.Property.Name == "this[]"
            // This needs to be narrowed down to the specific type, as NameValueCollection is a base class for other collections.
            && propertyReference.Property.ContainingType.IsAny(KnownType.System_Collections_Specialized_NameValueCollection, KnownType.Microsoft_Extensions_Configuration_IConfiguration))
        {
            return state.SetOperationConstraint(propertyReference, CryptographicKeyConstraint.StoredUnsafe);
        }
        return state;
    }

    // new SymmetricSecurityKey(bytes)
    private ProgramState ProcessSymmetricSecurityKeyConstructor(ProgramState state, IObjectCreationOperationWrapper objectCreation)
    {
        // SymmetricSecurityKey is defined in both System.IdentityModel.Tokens and Microsoft.IdentityModel.Tokens.
        if (objectCreation.Type.IsAny(KnownType.System_IdentityModel_Tokens_SymmetricSecurityKey, KnownType.Microsoft_IdentityModel_Tokens_SymmetricSecurityKey)
            && objectCreation.Arguments.Length == 1
            && objectCreation.Arguments[0] is { } argument
            && HasUnsafeValue())
        {
            ReportIssue(objectCreation.WrappedOperation);
        }
        return state;

        bool HasUnsafeValue() =>
            argument.AsArgument() is { Value.ConstantValue.HasValue: true } // new ...(null)
            || state[argument]?.HasConstraint(CryptographicKeyConstraint.StoredUnsafe) is true;
    }

    private sealed class Walker : SafeCSharpSyntaxWalker
    {
        public bool Result { get; private set; }

        public override void Visit(SyntaxNode node)
        {
            if (!Result)
            {
                base.Visit(node);
            }
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node) =>
            Result = node.NameIs("SymmetricSecurityKey");
    }
}
