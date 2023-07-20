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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RedundantInheritanceList : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1939";
        internal const string RedundantIndexKey = "redundantIndex";
        private const string MessageEnum = "'int' should not be explicitly used as the underlying type.";
        private const string MessageObjectBase = "'Object' should not be explicitly extended.";
        private const string MessageAlreadyImplements = "'{0}' implements '{1}' so '{1}' can be removed from the inheritance list.";
        private const string MessageFormat = "{0}";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
            {
                if (c.IsRedundantPositionalRecordContext() || c.Node is not BaseTypeDeclarationSyntax { BaseList: { Types: { Count: > 0 } } })
                {
                    return;
                }
                switch (c.Node)
                {
                    case EnumDeclarationSyntax enumDeclaration:
                        ReportRedundantBaseType(c, enumDeclaration, KnownType.System_Int32, MessageEnum);
                        break;
                    case InterfaceDeclarationSyntax interfaceDeclaration:
                        ReportRedundantInterfaces(c, interfaceDeclaration);
                        break;
                    case TypeDeclarationSyntax nonInterfaceDeclaration:
                        ReportRedundantBaseType(c, nonInterfaceDeclaration, KnownType.System_Object, MessageObjectBase);
                        ReportRedundantInterfaces(c, nonInterfaceDeclaration);
                        break;
                }
            },
            SyntaxKind.EnumDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKindEx.RecordClassDeclaration,
            SyntaxKindEx.RecordStructDeclaration);

        private static void ReportRedundantBaseType(SonarSyntaxNodeReportingContext context, BaseTypeDeclarationSyntax typeDeclaration, KnownType redundantType, string message)
        {
            var baseTypeSyntax = typeDeclaration.BaseList.Types.First().Type;
            if (context.SemanticModel.GetSymbolInfo(baseTypeSyntax).Symbol is ITypeSymbol baseTypeSymbol
                && baseTypeSymbol.Is(redundantType))
            {
                var location = GetLocationWithToken(baseTypeSyntax, typeDeclaration.BaseList.Types);
                context.ReportIssue(CreateDiagnostic(Rule, location, DiagnosticsProperties(0), message));
            }
        }

        private static void ReportRedundantInterfaces(SonarSyntaxNodeReportingContext context, BaseTypeDeclarationSyntax typeDeclaration)
        {
            var declaredType = context.SemanticModel.GetDeclaredSymbol(typeDeclaration);
            if (declaredType is null)
            {
                return;
            }

            var baseList = typeDeclaration.BaseList;
            var interfaceTypesWithAllInterfaces = GetImplementedInterfaceMappings(baseList, context.SemanticModel);

            for (var i = 0; i < baseList.Types.Count; i++)
            {
                var baseType = baseList.Types[i];
                if (context.SemanticModel.GetSymbolInfo(baseType.Type).Symbol is INamedTypeSymbol interfaceType
                    && interfaceType.IsInterface()
                    && CollidingDeclaration(declaredType, interfaceType, interfaceTypesWithAllInterfaces) is { } collidingDeclaration)
                {
                    var location = GetLocationWithToken(baseType.Type, baseList.Types);
                    var message = string.Format(MessageAlreadyImplements,
                        collidingDeclaration.ToMinimalDisplayString(context.SemanticModel, baseType.Type.SpanStart),
                        interfaceType.ToMinimalDisplayString(context.SemanticModel, baseType.Type.SpanStart));

                    context.ReportIssue(CreateDiagnostic(Rule, location, DiagnosticsProperties(i), message));
                }
            }
        }

        private static MultiValueDictionary<INamedTypeSymbol, INamedTypeSymbol> GetImplementedInterfaceMappings(BaseListSyntax baseList, SemanticModel semanticModel) =>
            baseList.Types
                    .Select(baseType => semanticModel.GetSymbolInfo(baseType.Type).Symbol as INamedTypeSymbol)
                    .WhereNotNull()
                    .Distinct()
                    .ToMultiValueDictionary(x => x.AllInterfaces);

        private static INamedTypeSymbol CollidingDeclaration(INamedTypeSymbol declaredType, INamedTypeSymbol interfaceType, MultiValueDictionary<INamedTypeSymbol, INamedTypeSymbol> interfaceMappings)
        {
            var collisionMapping = interfaceMappings.FirstOrDefault(x => x.Key.IsInterface() && x.Value.Contains(interfaceType));
            if (collisionMapping.Key is not null)
            {
                return collisionMapping.Key;
            }

            var baseClassMapping = interfaceMappings.FirstOrDefault(x => x.Key.IsClass());
            if (baseClassMapping.Key is null)
            {
                return null;
            }

            var canBeRemoved = CanInterfaceBeRemovedBasedOnMembers(declaredType, interfaceType);
            return canBeRemoved ? baseClassMapping.Key : null;
        }

        private static bool CanInterfaceBeRemovedBasedOnMembers(INamedTypeSymbol declaredType, INamedTypeSymbol interfaceType)
        {
            var allMembersOfInterface = AllInterfacesAndSelf(interfaceType).SelectMany(x => x.GetMembers()).ToImmutableArray();

            if (!allMembersOfInterface.Any())
            {
                return false;
            }

            foreach (var interfaceMember in allMembersOfInterface)
            {
                if (declaredType.FindImplementationForInterfaceMember(interfaceMember) is { } classMember
                    && (classMember.ContainingType.Equals(declaredType)
                        || !classMember.ContainingType.Interfaces.Any(x => AllInterfacesAndSelf(x).Contains(interfaceType))))
                {
                    return false;
                }
            }
            return true;

            static IEnumerable<INamedTypeSymbol> AllInterfacesAndSelf(INamedTypeSymbol interfaceType) =>
                interfaceType.AllInterfaces.Concat(new[] { interfaceType });
        }

        private static Location GetLocationWithToken(TypeSyntax type, SeparatedSyntaxList<BaseTypeSyntax> baseTypes)
        {
            var span = baseTypes.Count == 1 || baseTypes.First().Type != type
                ? TextSpan.FromBounds(type.GetFirstToken().GetPreviousToken().Span.Start, type.Span.End)
                : TextSpan.FromBounds(type.SpanStart, type.GetLastToken().GetNextToken().Span.End);

            return Location.Create(type.SyntaxTree, span);
        }

        private static ImmutableDictionary<string, string> DiagnosticsProperties(int redundantIndex) =>
            ImmutableDictionary.Create<string, string>().Add(RedundantIndexKey, redundantIndex.ToString());
    }
}
