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

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class JwsSecretKeys : HardcodedBytesRuleBase
{
    private const string DiagnosticId = "S6781";
    private const string MessageFormat = "JWT secret keys should not be disclosed.";

    public static readonly DiagnosticDescriptor S6781 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    protected override DiagnosticDescriptor Rule => S6781;
    protected override SymbolicConstraint Hardcoded => CryptographicKeyConstraint.StoredUnsafe;
    protected override SymbolicConstraint NotHardcoded => CryptographicKeyConstraint.StoredSafe;

    public override bool ShouldExecute() => true;

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

    private ProgramState ProcessPropertyReference(ProgramState state, IPropertyReferenceOperationWrapper propertyReference)
    {
        if (propertyReference.Property.Name == "this[]"
            // This needs to be narrowed down to the specific type, as NameValueCollection is a base class for other collections.
            && propertyReference.Property.ContainingType.IsAny(KnownType.System_Collections_Specialized_NameValueCollection, KnownType.Microsoft_Extensions_Configuration_IConfiguration))
        {
            return state.SetOperationConstraint(propertyReference, CryptographicKeyConstraint.StoredUnsafe);
        }
        return state;
    }

    private ProgramState ProcessSymmetricSecurityKeyConstructor(ProgramState state, IObjectCreationOperationWrapper objectCreation)
    {
        // SymmetricSecurityKey is defined in both System.IdentityModel.Tokens and Microsoft.IdentityModel.Tokens.
        if (objectCreation.Type.IsAny(KnownType.System_IdentityModel_Tokens_SymmetricSecurityKey, KnownType.Microsoft_IdentityModel_Tokens_SymmetricSecurityKey)
            && objectCreation.Arguments.Length == 1
            && objectCreation.Arguments[0] is { } argument
            && state[argument]?.HasConstraint(CryptographicKeyConstraint.StoredUnsafe) is true)
        {
            ReportIssue(objectCreation.WrappedOperation);
        }
        return state;
    }
}
