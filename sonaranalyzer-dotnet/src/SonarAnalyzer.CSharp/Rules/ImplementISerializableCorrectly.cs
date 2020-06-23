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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class ImplementISerializableCorrectly : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3925";
        internal const string MessageFormat = "Update this implementation of 'ISerializable' to conform to the " +
            "recommended serialization pattern.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)c.Node;
                    var typeSymbol = c.SemanticModel.GetDeclaredSymbol(classDeclaration);

                    if (!ImplementsISerializable(typeSymbol))
                    {
                        return;
                    }

                    var getObjectData = typeSymbol.GetMembers()
                        .OfType<IMethodSymbol>()
                        .FirstOrDefault(KnownMethods.IsGetObjectData);

                    var implementationErrors = new List<SecondaryLocation>();

                    implementationErrors.AddRange(CheckSerializableAttribute(classDeclaration, typeSymbol));
                    implementationErrors.AddRange(CheckConstructor(classDeclaration, typeSymbol));
                    implementationErrors.AddRange(CheckGetObjectDataAccessibility(typeSymbol, getObjectData));
                    implementationErrors.AddRange(CheckGetObjectData(typeSymbol, getObjectData, classDeclaration));

                    if (implementationErrors.Count > 0)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, classDeclaration.Identifier.GetLocation(),
                            additionalLocations: implementationErrors.ToAdditionalLocations(),
                            properties: implementationErrors.ToProperties()));
                    }
                },
                SyntaxKind.ClassDeclaration);
        }

        private static IEnumerable<SecondaryLocation> CheckSerializableAttribute(
            ClassDeclarationSyntax classDeclaration, INamedTypeSymbol typeSymbol)
        {
            if (!HasSerializableAttribute(typeSymbol) &&
                !typeSymbol.IsAbstract)
            {
                yield return new SecondaryLocation(classDeclaration.Keyword.GetLocation(),
                    $"Add 'System.SerializableAttribute' attribute on '{typeSymbol.Name}' because it " +
                    "implements 'ISerializable'.");
            }
        }

        private static IEnumerable<TSyntax> GetDeclarations<TSyntax>(ISymbol symbol)
        {
            if (symbol == null)
            {
                return Enumerable.Empty<TSyntax>();
            }

            return symbol.DeclaringSyntaxReferences.Select(r => r.GetSyntax()).Cast<TSyntax>();
        }

        private static IEnumerable<SecondaryLocation> CheckGetObjectData(INamedTypeSymbol typeSymbol,
            IMethodSymbol getObjectData, ClassDeclarationSyntax classDeclaration)
        {
            if (!ImplementsISerializable(typeSymbol.BaseType))
            {
                yield break;
            }

            if (getObjectData == null)
            {
                var serializableFields = GetSerializableFieldNames(typeSymbol).ToList();
                if (serializableFields.Count > 0)
                {
                    yield return new SecondaryLocation(classDeclaration.Keyword.GetLocation(),
                        "Override 'GetObjectData(SerializationInfo, StreamingContext)' and serialize " +
                        $"'{string.Join(", ", serializableFields)}'.");
                }
            }
            else if (getObjectData.IsOverride && !IsCallingBase(getObjectData))
            {
                foreach (var declaration in GetDeclarations<MethodDeclarationSyntax>(getObjectData))
                {
                    yield return new SecondaryLocation(declaration.Identifier.GetLocation(),
                        "Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in this method.");
                }
            }
            else
            {
                /*do nothing*/
            }
        }

        private static IEnumerable<SecondaryLocation> CheckGetObjectDataAccessibility(INamedTypeSymbol typeSymbol,
            IMethodSymbol getObjectData)
        {
            if (getObjectData == null)
            {
                yield break;
            }

            if (typeSymbol.IsSealed ||
                IsPublicVirtual(getObjectData) ||
                IsExplicitImplementation(getObjectData))
            {
                yield break;
            }

            foreach (var declaration in GetDeclarations<MethodDeclarationSyntax>(getObjectData))
            {
                yield return new SecondaryLocation(declaration.Identifier.GetLocation(),
                    $"Make 'GetObjectData' 'public' and 'virtual', or seal '{typeSymbol.Name}'.");
            }
        }

        private static IEnumerable<string> GetSerializableFieldNames(INamedTypeSymbol typeSymbol) =>
            typeSymbol.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(f => !f.IsStatic)
                .Where(f => ImplementsISerializable(f.Type))
                .Select(f => f.Name);

        private static IEnumerable<SecondaryLocation> CheckConstructor(ClassDeclarationSyntax classDeclaration,
            INamedTypeSymbol typeSymbol)
        {
            var serializationConstructor = typeSymbol.Constructors.FirstOrDefault(KnownMethods.IsSerializationConstructor);

            var accessibility = typeSymbol.IsSealed ? "private" : "protected";

            if (serializationConstructor == null)
            {
                yield return new SecondaryLocation(classDeclaration.Keyword.GetLocation(),
                    $"Add a '{accessibility}' constructor '{typeSymbol.Name}(SerializationInfo, StreamingContext)'.");
                yield break;
            }

            var constructorSyntax = GetDeclarations<ConstructorDeclarationSyntax>(serializationConstructor).First();

            if (typeSymbol.IsSealed && serializationConstructor.DeclaredAccessibility != Accessibility.Private ||
                !typeSymbol.IsSealed && serializationConstructor.DeclaredAccessibility != Accessibility.Protected)
            {
                yield return new SecondaryLocation(constructorSyntax.Identifier.GetLocation(),
                    $"Make this constructor '{accessibility}'.");
            }

            if (ImplementsISerializable(typeSymbol.BaseType) &&
                !IsCallingBaseConstructor(serializationConstructor))
            {
                yield return new SecondaryLocation(constructorSyntax.Identifier.GetLocation(),
                    $"Call constructor 'base(SerializationInfo, StreamingContext)'.");
            }
        }

        private static bool IsCallingBase(IMethodSymbol methodSymbol)
        {
            var methodDeclaration = (MethodDeclarationSyntax)methodSymbol
                .DeclaringSyntaxReferences
                .FirstOrDefault()
                ?.GetSyntax();
            if (methodDeclaration == null)
            {
                return false;
            }

            return methodDeclaration.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Select(ies => ies.Expression)
                .OfType<MemberAccessExpressionSyntax>()
                .Any(memberAccess => memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                                     memberAccess.Expression.IsKind(SyntaxKind.BaseExpression) &&
                                     memberAccess.Name.Identifier.ValueText == nameof(ISerializable.GetObjectData));
        }

        private static bool IsCallingBaseConstructor(IMethodSymbol constructorSymbol)
        {
            var constructorDeclaration = (ConstructorDeclarationSyntax)constructorSymbol
                .DeclaringSyntaxReferences
                .FirstOrDefault()
                ?.GetSyntax();
            if (constructorDeclaration == null)
            {
                return false;
            }

            var baseKeyword = constructorDeclaration?.Initializer?.ThisOrBaseKeyword;
            return baseKeyword.HasValue && baseKeyword.Value.IsKind(SyntaxKind.BaseKeyword);
        }

        private static bool ImplementsISerializable(ITypeSymbol typeSymbol) =>
            typeSymbol != null &&
            typeSymbol.IsPubliclyAccessible() &&
            typeSymbol.AllInterfaces.Any(IsOrImplementsISerializable);

        private static bool HasSerializableAttribute(ISymbol typeSymbol) =>
            typeSymbol.GetAttributes(KnownType.System_SerializableAttribute).Any();

        private static bool IsOrImplementsISerializable(ITypeSymbol typeSymbol) =>
            typeSymbol.Is(KnownType.System_Runtime_Serialization_ISerializable) ||
            typeSymbol.Implements(KnownType.System_Runtime_Serialization_ISerializable);

        private static bool IsPublicVirtual(IMethodSymbol methodSymbol) =>
            methodSymbol.DeclaredAccessibility == Accessibility.Public &&
            (methodSymbol.IsVirtual || methodSymbol.IsOverride);

        private static bool IsExplicitImplementation(IMethodSymbol methodSymbol) =>
            methodSymbol.ExplicitInterfaceImplementations.Any(KnownMethods.IsGetObjectData);
    }
}
