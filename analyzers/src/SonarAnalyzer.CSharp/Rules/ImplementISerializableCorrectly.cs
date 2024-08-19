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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ImplementISerializableCorrectly : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3925";
        private const string MessageFormat = "Update this implementation of 'ISerializable' to conform to the recommended serialization pattern. {0}";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    if (c.IsRedundantPositionalRecordContext())
                    {
                        return;
                    }

                    var typeDeclarationSyntax = (TypeDeclarationSyntax)c.Node;
                    var typeSymbol = (INamedTypeSymbol)c.ContainingSymbol;
                    if (!ImplementsISerializable(typeSymbol) || !OptsInForSerialization(typeSymbol))
                    {
                        return;
                    }

                    var getObjectData = typeSymbol.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(KnownMethods.IsGetObjectData);
                    var implementationErrors = new List<SecondaryLocation>();
                    implementationErrors.AddRange(CheckSerializableAttribute(typeDeclarationSyntax.Keyword, typeSymbol));
                    implementationErrors.AddRange(CheckConstructor(typeDeclarationSyntax, typeSymbol));
                    implementationErrors.AddRange(CheckGetObjectDataAccessibility(typeDeclarationSyntax, typeSymbol, getObjectData));
                    implementationErrors.AddRange(CheckGetObjectData(typeDeclarationSyntax, typeSymbol, getObjectData));
                    if (implementationErrors.Any())
                    {
                        c.ReportIssue(Rule, typeDeclarationSyntax.Identifier, implementationErrors, implementationErrors.JoinStr(" ", x => x.Message));
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKindEx.RecordDeclaration,
                SyntaxKindEx.RecordStructDeclaration,
                SyntaxKind.StructDeclaration);

        private static IEnumerable<SecondaryLocation> CheckSerializableAttribute(SyntaxToken typeKeyword, INamedTypeSymbol typeSymbol)
        {
            if (!typeSymbol.IsAbstract && !HasSerializableAttribute(typeSymbol))
            {
                yield return new(typeKeyword.GetLocation(), $"Add 'System.SerializableAttribute' attribute on '{typeSymbol.Name}' because it implements 'ISerializable'.");
            }
        }

        // Symbol should be checked for null in the caller.
        private static IEnumerable<TSyntax> DeclarationOrImplementation<TSyntax>(TypeDeclarationSyntax typeDeclaration, IMethodSymbol symbol) =>
            symbol.PartialImplementationPart is not null
            && symbol.PartialImplementationPart.DeclaringSyntaxReferences.First().GetSyntax() is var partialImplementation
            && typeDeclaration.DescendantNodes().Any(x => x.Equals(partialImplementation))
                ? new[] { partialImplementation }.Cast<TSyntax>()
                : symbol.DeclaringSyntaxReferences.Select(x => x.GetSyntax()).Cast<TSyntax>();

        private static IEnumerable<SecondaryLocation> CheckGetObjectData(TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol, IMethodSymbol getObjectData)
        {
            if (!ImplementsISerializable(typeSymbol.BaseType))
            {
                yield break;
            }

            if (getObjectData == null)
            {
                var serializableFields = GetSerializableFieldNames(typeSymbol).ToList();
                if (serializableFields.Any())
                {
                    yield return new(typeDeclaration.Keyword.GetLocation(), $"Override 'GetObjectData(SerializationInfo, StreamingContext)' and serialize '{serializableFields.JoinAnd()}'.");
                }
            }
            else if (getObjectData.IsOverride && !IsCallingBase(getObjectData))
            {
                foreach (var declaration in DeclarationOrImplementation<MethodDeclarationSyntax>(typeDeclaration, getObjectData))
                {
                    yield return new(declaration.Identifier.GetLocation(), "Invoke 'base.GetObjectData(SerializationInfo, StreamingContext)' in 'GetObjectData'.");
                }
            }
        }

        private static IEnumerable<SecondaryLocation> CheckGetObjectDataAccessibility(TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol, IMethodSymbol getObjectData)
        {
            if (getObjectData == null || typeSymbol.IsSealed || IsPublicVirtual(getObjectData) || IsExplicitImplementation(getObjectData))
            {
                yield break;
            }
            foreach (var declaration in DeclarationOrImplementation<MethodDeclarationSyntax>(typeDeclaration, getObjectData))
            {
                yield return new(declaration.Identifier.GetLocation(), $"Make 'GetObjectData' 'public' and 'virtual', or seal '{typeSymbol.Name}'.");
            }
        }

        private static IEnumerable<string> GetSerializableFieldNames(INamedTypeSymbol typeSymbol) =>
            typeSymbol.GetMembers().OfType<IFieldSymbol>()
                .Where(x => !x.IsStatic && ImplementsISerializable(x.Type))
                .Select(x => x.Name);

        private static IEnumerable<SecondaryLocation> CheckConstructor(TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol)
        {
            var accessibility = typeSymbol.IsSealed ? SyntaxConstants.Private : SyntaxConstants.Protected;
            if (typeSymbol.Constructors.FirstOrDefault(KnownMethods.IsSerializationConstructor) is { } serializationConstructor)
            {
                var constructorSyntax = DeclarationOrImplementation<ConstructorDeclarationSyntax>(typeDeclaration, serializationConstructor).First();

                if ((typeSymbol.IsSealed && serializationConstructor.DeclaredAccessibility != Accessibility.Private)
                    || (!typeSymbol.IsSealed && serializationConstructor.DeclaredAccessibility != Accessibility.Protected))
                {
                    yield return new(constructorSyntax.Identifier.GetLocation(), $"Make the serialization constructor '{accessibility}'.");
                }

                if (ImplementsISerializable(typeSymbol.BaseType) && !IsCallingBaseConstructor(serializationConstructor))
                {
                    yield return new(constructorSyntax.Identifier.GetLocation(), $"Call 'base(SerializationInfo, StreamingContext)' on the serialization constructor.");
                }
            }
            else
            {
                yield return new(typeDeclaration.Keyword.GetLocation(), $"Add a '{accessibility}' constructor '{typeSymbol.Name}(SerializationInfo, StreamingContext)'.");
            }
        }

        private static bool IsCallingBase(IMethodSymbol methodSymbol) =>
            methodSymbol.ImplementationSyntax() is { } methodDeclaration
            && methodDeclaration.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Select(x => x.Expression)
                .OfType<MemberAccessExpressionSyntax>()
                .Any(x => x.IsKind(SyntaxKind.SimpleMemberAccessExpression) && x.Expression.IsKind(SyntaxKind.BaseExpression) && x.Name.Identifier.ValueText == nameof(ISerializable.GetObjectData));

        private static bool IsCallingBaseConstructor(IMethodSymbol constructorSymbol) =>
            constructorSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is ConstructorDeclarationSyntax { Initializer: { ThisOrBaseKeyword: { RawKind: (int)SyntaxKind.BaseKeyword } } };

        private static bool ImplementsISerializable(ITypeSymbol typeSymbol) =>
            typeSymbol != null
            && typeSymbol.IsPubliclyAccessible()
            && typeSymbol.Implements(KnownType.System_Runtime_Serialization_ISerializable);

        private static bool OptsInForSerialization(INamedTypeSymbol typeSymbol) =>
            typeSymbol.IsSerializable() // [Serializable] is present at the types declaration
            || typeSymbol.Interfaces.Any(x => x.Is(KnownType.System_Runtime_Serialization_ISerializable)) // ISerializable is listed in the types declaration base type list
            || typeSymbol.Constructors.Any(KnownMethods.IsSerializationConstructor); // A serialization constructor is defined

        private static bool HasSerializableAttribute(ISymbol symbol) =>
            symbol.HasAttribute(KnownType.System_SerializableAttribute);

        private static bool IsPublicVirtual(IMethodSymbol methodSymbol) =>
            methodSymbol.DeclaredAccessibility == Accessibility.Public
            && (methodSymbol.IsVirtual || methodSymbol.IsOverride);

        private static bool IsExplicitImplementation(IMethodSymbol methodSymbol) =>
            methodSymbol.ExplicitInterfaceImplementations.Any(KnownMethods.IsGetObjectData);
    }
}
