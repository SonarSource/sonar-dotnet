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

using System.Runtime.Serialization;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;

public sealed class RestrictDeserializedTypes : RestrictDeserializedTypesBase
{
    public static readonly DiagnosticDescriptor S5773 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    protected override DiagnosticDescriptor Rule => S5773;

    public override bool ShouldExecute() => true;

    protected override bool IsBindToTypeMethod(SyntaxNode methodDeclaration) =>
        methodDeclaration is MethodDeclarationSyntax { Identifier.Text: nameof(SerializationBinder.BindToType), ParameterList.Parameters.Count: 2 } syntax
        && syntax.EnsureCorrectSemanticModelOrDefault(SemanticModel) is { } semanticModel
        && syntax.ParameterList.Parameters[0].Type.IsKnownType(KnownType.System_String, semanticModel)
        && syntax.ParameterList.Parameters[1].Type.IsKnownType(KnownType.System_String, semanticModel);

    protected override bool IsResolveTypeMethod(SyntaxNode methodDeclaration) =>
        methodDeclaration is MethodDeclarationSyntax { Identifier.Text: "ResolveType", ParameterList.Parameters.Count: 1 } syntax
        && syntax.EnsureCorrectSemanticModelOrDefault(SemanticModel) is { } semanticModel
        && syntax.ParameterList.Parameters[0].Type.IsKnownType(KnownType.System_String, semanticModel);

    protected override bool ThrowsOrReturnsNull(SyntaxNode methodDeclaration) => ((MethodDeclarationSyntax)methodDeclaration).ThrowsOrReturnsNull();

    protected override SyntaxToken GetIdentifier(SyntaxNode methodDeclaration) => ((MethodDeclarationSyntax)methodDeclaration).Identifier;
}
