/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Common.Constraints;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.Rules.CSharp
{
    internal sealed class RestrictDeserializedTypes : ISymbolicExecutionAnalyzer
    {
        internal const string DiagnosticId = "S5773";
        private const string MessageFormat = "{0}";
        private const string RestrictTypesMessage = "Restrict types of objects allowed to be deserialized.";
        private const string VerifyMacMessage = "Serialized data signature (MAC) should be verified.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public IEnumerable<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        public ISymbolicExecutionAnalysisContext AddChecks(CSharpExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context) =>
            new AnalysisContext(explodedGraph);

        private sealed class AnalysisContext : ISymbolicExecutionAnalysisContext
        {
            private readonly List<LocationInfo> locations = new List<LocationInfo>();

            public AnalysisContext(AbstractExplodedGraph explodedGraph)
            {
                explodedGraph.AddExplodedGraphCheck(new SerializationBinderCheck(explodedGraph, AddIssueLocation));
            }

            public bool SupportsPartialResults => true;

            public IEnumerable<Diagnostic> GetDiagnostics() =>
                locations.Select(location => Diagnostic.Create(rule, location.Primary,  location.SecondaryLocations, location.Message));

            public void Dispose()
            {
                // Nothing to do here.
            }

            private void AddIssueLocation(LocationInfo locationInfo)
            {
                // An issue can appear multiple times for the same node while exploring the exploded graph.
                // In this case the location is added one single time to avoid duplicates.
                if (!locations.Any(location => location.Primary.Equals(locationInfo.Primary)))
                {
                    locations.Add(locationInfo);
                }
            }
        }

        private sealed class SerializationBinderCheck : ExplodedGraphCheck
        {
            private static readonly ImmutableArray<KnownType> typesWithBinder =
                ImmutableArray.Create(
                    KnownType.System_Runtime_Serialization_Formatters_Binary_BinaryFormatter,
                    KnownType.System_Runtime_Serialization_NetDataContractSerializer,
                    KnownType.System_Runtime_Serialization_Formatters_Soap_SoapFormatter);

            private readonly Dictionary<ITypeSymbol, TypeDeclarationInfo> sanitizerDeclarations = new Dictionary<ITypeSymbol, TypeDeclarationInfo>();
            private readonly Dictionary<SymbolicValue, Location> secondaryLocations = new Dictionary<SymbolicValue, Location>();

            private readonly Action<LocationInfo> addLocation;

            public SerializationBinderCheck(AbstractExplodedGraph explodedGraph, Action<LocationInfo> addLocation)
                : base(explodedGraph)
            {
                this.addLocation = addLocation;
            }

            public override ProgramState ObjectCreated(ProgramState programState, SymbolicValue symbolicValue, SyntaxNode instruction)
            {
                if (instruction is ObjectCreationExpressionSyntax objectCreation)
                {
                    var typeSymbol = semanticModel.GetTypeInfo(instruction).Type;
                    if (IsFormatterWithBinder(typeSymbol))
                    {
                        var declarationInfo = GetBinderDeclaration(objectCreation.Initializer);
                        if (!declarationInfo.IsSafe)
                        {
                            AddSecondaryLocation(symbolicValue, declarationInfo);
                            programState = programState.SetConstraint(symbolicValue, SerializationConstraint.Unsafe);
                        }
                    }

                    if (IsJavaScriptSerializer(typeSymbol))
                    {
                        var declarationInfo = GetResolverDeclaration(objectCreation);
                        if (!declarationInfo.IsSafe)
                        {
                            AddSecondaryLocation(symbolicValue, declarationInfo);
                            programState = programState.SetConstraint(symbolicValue, SerializationConstraint.Unsafe);
                        }
                    }

                    if (IsLosFormatter(typeSymbol) &&
                        !IsLosFormatterSafe(objectCreation, programState))
                    {
                        // For LosFormatter the rule is raised directly on the constructor.
                        addLocation(new LocationInfo(objectCreation.GetLocation(), VerifyMacMessage));
                    }
                }

                return base.ObjectCreated(programState, symbolicValue, instruction);
            }

            private bool IsLosFormatterSafe(ObjectCreationExpressionSyntax objectCreation, ProgramState programState)
            {
                // The constructor is safe only if it has 2 arguments and the first argument value is true.
                if (objectCreation.ArgumentList == null ||
                    objectCreation.ArgumentList.Arguments.Count != 2)
                {
                    return false;
                }

                var firstArgument = GetEnableMacArgumentSyntax(objectCreation.ArgumentList);
                if (firstArgument.IsKind(SyntaxKind.FalseLiteralExpression))
                {
                    return false;
                }

                if (firstArgument.IsKind(SyntaxKind.TrueLiteralExpression))
                {
                    return true;
                }

                var symbol = semanticModel.GetSymbolInfo(firstArgument).Symbol;
                if (symbol == null)
                {
                    // In this case we cannot determine if the first parameter is true or false so we
                    // assume it's true to avoid FPs.
                    return true;
                }

                var symbolicValue = programState.GetSymbolValue(symbol);
                return symbolicValue == null ||
                       !programState.HasConstraint(symbolicValue, BoolConstraint.False);
            }

            private static ExpressionSyntax GetEnableMacArgumentSyntax(BaseArgumentListSyntax list) =>
                (list.GetArgumentByName("enableMac") ?? list.Arguments[0]).Expression;

            public override ProgramState PreProcessInstruction(ProgramPoint programPoint, ProgramState programState)
            {
                var instruction = programPoint.Block.Instructions[programPoint.Offset];

                programState = instruction switch
                {
                    MemberAccessExpressionSyntax memberAccess => VisitMemberAccess(memberAccess, programState),
                    AssignmentExpressionSyntax assignmentExpressionSyntax => VisitAssignmentExpression(assignmentExpressionSyntax, programState),
                    _ => programState
                };

                return base.PreProcessInstruction(programPoint, programState);
            }

            private ProgramState VisitMemberAccess(MemberAccessExpressionSyntax memberAccess, ProgramState programState)
            {
                if (IsDeserializeOnKnownType(memberAccess) && programState.HasValue)
                {
                    // If Deserialize is called on an expression which returns a formatter (e.g. new BinaryFormatter().Deserialize()),
                    // the symbolic value corresponding to the returned value will be available on the top of the stack.
                    var symbolValue = programState.PeekValue();

                    if (programState.HasConstraint(symbolValue, SerializationConstraint.Unsafe))
                    {
                        var locationInfo = secondaryLocations.TryGetValue(symbolValue, out var location)
                            ? new LocationInfo(memberAccess.Name.GetLocation(), RestrictTypesMessage, location)
                            : new LocationInfo(memberAccess.Name.GetLocation(), RestrictTypesMessage);

                        addLocation(locationInfo);
                    }
                }

                return programState;
            }

            private ProgramState VisitAssignmentExpression(AssignmentExpressionSyntax assignmentExpression, ProgramState programState)
            {
                if (!(assignmentExpression.Left is MemberAccessExpressionSyntax memberAccess) || !IsBinderProperty(memberAccess))
                {
                    return programState;
                }

                var typeSymbol = semanticModel.GetTypeInfo(memberAccess.Expression).Type;
                if (!IsFormatterWithBinder(typeSymbol))
                {
                    return programState;
                }

                var formatterSymbol = semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
                var formatterSymbolValue = programState.GetSymbolValue(formatterSymbol);
                if (formatterSymbolValue == null)
                {
                    return programState;
                }

                if (assignmentExpression.Right.IsNullLiteral())
                {
                    // The formatter is considered unsafe if the binder is null.
                    return programState.SetConstraint(formatterSymbolValue, SerializationConstraint.Unsafe);
                }

                var binderSymbol = semanticModel.GetSymbolInfo(assignmentExpression.Right).Symbol;
                var binderSymbolValue = programState.GetSymbolValue(binderSymbol);
                if (binderSymbolValue != null &&
                    programState.HasConstraint(binderSymbolValue, ObjectConstraint.Null))
                {
                    // The formatter is considered unsafe if the binder is null.
                    return programState.SetConstraint(formatterSymbolValue, SerializationConstraint.Unsafe);
                }

                var binderType = semanticModel.GetTypeInfo(assignmentExpression.Right).Type;
                if (binderType == null)
                {
                    return programState;
                }

                var declaration = GetOrAddSanitizerDeclaration(binderType);
                if (!declaration.IsSafe)
                {
                    AddSecondaryLocation(formatterSymbolValue, declaration);
                }

                var constraint = declaration.IsSafe
                    ? SerializationConstraint.Safe
                    : SerializationConstraint.Unsafe;

                return programState.SetConstraint(formatterSymbolValue, constraint);
            }

            private void AddSecondaryLocation(SymbolicValue symbolicValue, TypeDeclarationInfo declarationInfo)
            {
                if (declarationInfo.Location != null)
                {
                    secondaryLocations[symbolicValue] = declarationInfo.Location;
                }
            }

            private TypeDeclarationInfo GetResolverDeclaration(ObjectCreationExpressionSyntax objectCreation)
            {
                // JavaScriptSerializer has 2 constructors:
                // - JavaScriptSerializer(): unsafe since it doesn't give the option to set a provider
                // - JavaScriptSerializer(JavaScriptTypeResolver): this one is safe only if the given resolver is safe
                // See: https://docs.microsoft.com/en-us/dotnet/api/system.web.script.serialization.javascriptserializer.-ctor?view=netframework-4.8

                if (objectCreation.ArgumentList == null ||
                    objectCreation.ArgumentList.Arguments.Count == 0)
                {
                    return new TypeDeclarationInfo(false);
                }

                var resolverType = objectCreation.ArgumentList.Arguments[0];
                if (resolverType.Expression.IsNullLiteral())
                {
                    return new TypeDeclarationInfo(false);
                }

                return semanticModel.GetTypeInfo(resolverType.Expression).Type is {} typeSymbol &&
                       !typeSymbol.Is(KnownType.System_Web_Script_Serialization_SimpleTypeResolver)
                    ? GetOrAddSanitizerDeclaration(typeSymbol)
                    : new TypeDeclarationInfo(false);
            }

            private TypeDeclarationInfo GetBinderDeclaration(InitializerExpressionSyntax initializer)
            {
                var typeSymbol = GetBinderTypeSymbol(initializer);
                return typeSymbol == null
                    ? new TypeDeclarationInfo(false)
                    : GetOrAddSanitizerDeclaration(typeSymbol);
            }

            private ITypeSymbol GetBinderTypeSymbol(InitializerExpressionSyntax initializer)
            {
                var binderAssignment = initializer?.Expressions
                    .OfType<AssignmentExpressionSyntax>()
                    .SingleOrDefault(assignment => IsBinderProperty(assignment.Left));

                if (binderAssignment == null ||
                    binderAssignment.Right.IsNullLiteral())
                {
                    return null;
                }

                return semanticModel.GetTypeInfo(binderAssignment.Right).Type;
            }

            private TypeDeclarationInfo GetOrAddSanitizerDeclaration(ITypeSymbol symbol) =>
                sanitizerDeclarations.GetOrAdd(symbol, typeSymbol =>
                {
                    var declaration = typeSymbol.DerivesFrom(KnownType.System_Web_Script_Serialization_JavaScriptTypeResolver)
                        ? GetResolveTypeMethodDeclaration(typeSymbol)
                        : GetBindToTypeMethodDeclaration(typeSymbol);

                    return new TypeDeclarationInfo(declaration);
                });

            private static MethodDeclarationSyntax GetBindToTypeMethodDeclaration(ISymbol symbol) =>
                symbol.DeclaringSyntaxReferences
                    .SelectMany(GetDescendantNodes)
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault(IsBindToType);

            private static MethodDeclarationSyntax GetResolveTypeMethodDeclaration(ISymbol symbol) =>
                symbol.DeclaringSyntaxReferences
                    .SelectMany(GetDescendantNodes)
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault(IsResolveType);

            private static IEnumerable<SyntaxNode> GetDescendantNodes(SyntaxReference syntaxReference) =>
                syntaxReference.GetSyntax().DescendantNodes();

            private static bool IsBindToType(MethodDeclarationSyntax methodDeclaration) =>
                methodDeclaration.Identifier.Text == "BindToType" &&
                methodDeclaration.ReturnType.NameIs("Type") &&
                methodDeclaration.ParameterList.Parameters.Count == 2 &&
                methodDeclaration.ParameterList.Parameters[0].IsString() &&
                methodDeclaration.ParameterList.Parameters[1].IsString();

            private static bool IsResolveType(MethodDeclarationSyntax methodDeclaration) =>
                methodDeclaration.Identifier.Text == "ResolveType" &&
                methodDeclaration.ReturnType.NameIs("Type") &&
                methodDeclaration.ParameterList.Parameters.Count == 1 &&
                methodDeclaration.ParameterList.Parameters[0].IsString();

            private bool IsDeserializeOnKnownType(MemberAccessExpressionSyntax memberAccess) =>
                IsDeserializeMethod(memberAccess) &&
                semanticModel.GetTypeInfo(memberAccess.Expression).Type is {} typeSymbol &&
                (IsFormatterWithBinder(typeSymbol) || IsJavaScriptSerializer(typeSymbol));

            private static bool IsFormatterWithBinder(ITypeSymbol typeSymbol) =>
                typeSymbol.IsAny(typesWithBinder);

            private static bool IsBinderProperty(ExpressionSyntax expression) =>
                expression.NameIs("Binder");

            private static bool IsDeserializeMethod(ExpressionSyntax expression) =>
                expression.NameIs("Deserialize");

            private static bool IsJavaScriptSerializer(ITypeSymbol typeSymbol) =>
                typeSymbol.Is(KnownType.System_Web_Script_Serialization_JavaScriptSerializer);

            private static bool IsLosFormatter(ITypeSymbol typeSymbol) =>
                typeSymbol.Is(KnownType.System_Web_UI_LosFormatter);
        }

        private sealed class LocationInfo
        {
            internal Location Primary { get; }

            internal string Message { get; }

            internal IEnumerable<Location> SecondaryLocations { get; }

            public LocationInfo(Location primary, string message, Location secondary = null)
            {
                Primary = primary;
                Message = message;
                SecondaryLocations = secondary == null
                    ? Enumerable.Empty<Location>()
                    : new[] {secondary};
            }
        }

        private sealed class TypeDeclarationInfo
        {
            internal Location Location { get; }

            internal bool IsSafe { get; }

            public TypeDeclarationInfo(MethodDeclarationSyntax declaration)
            {
                Location = declaration?.Identifier.GetLocation();

                IsSafe = declaration == null || declaration.ThrowsOrReturnsNull();
            }

            public TypeDeclarationInfo(bool isSafe)
            {
                IsSafe = isSafe;
                Location = null;
            }
        }
    }
}
