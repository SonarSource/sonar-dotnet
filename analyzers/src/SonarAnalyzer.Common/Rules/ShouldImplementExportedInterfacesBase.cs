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

namespace SonarAnalyzer.Rules
{
    public abstract class ShouldImplementExportedInterfacesBase<TArgumentSyntax, TAttributeSyntax, TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TArgumentSyntax : SyntaxNode
        where TAttributeSyntax : SyntaxNode
        where TSyntaxKind : struct
    {
        private const string DiagnosticId = "S4159";
        private const string ActionForInterface = "Implement";
        private const string ActionForClass = "Derive from";

        private readonly ImmutableArray<KnownType> exportAttributes =
            ImmutableArray.Create(
                KnownType.System_ComponentModel_Composition_ExportAttribute,
                KnownType.System_ComponentModel_Composition_InheritedExportAttribute,
                KnownType.System_Composition_ExportAttribute);

        protected abstract SeparatedSyntaxList<TArgumentSyntax>? GetAttributeArguments(TAttributeSyntax attributeSyntax);
        protected abstract SyntaxNode GetAttributeName(TAttributeSyntax attributeSyntax);
        protected abstract bool IsClassOrRecordSyntax(SyntaxNode syntaxNode);
        // Retrieve the expression inside of the typeof()/GetType() (e.g. typeof(Foo) => Foo)
        protected abstract SyntaxNode GetTypeOfOrGetTypeExpression(SyntaxNode expressionSyntax);

        protected override string MessageFormat => "{0} '{1}' on '{2}' or remove this export attribute.";

        protected ShouldImplementExportedInterfacesBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer,
                c =>
                {
                    var attributeSyntax = (TAttributeSyntax)c.Node;
                    if (c.SemanticModel.GetSymbolInfo(GetAttributeName(attributeSyntax)).Symbol is not IMethodSymbol attributeCtorSymbol
                        || !attributeCtorSymbol.ContainingType.IsAny(exportAttributes))
                    {
                        return;
                    }

                    var exportedType = GetExportedTypeSymbol(GetAttributeArguments(attributeSyntax), c.SemanticModel);
                    var attributeTargetType = GetAttributeTargetSymbol(attributeSyntax, c.SemanticModel);
                    if (exportedType is null
                        || attributeTargetType is null
                        || IsOfExportType(attributeTargetType, exportedType))
                    {
                        return;
                    }

                    var action = exportedType.IsInterface()
                                     ? ActionForInterface
                                     : ActionForClass;

                    c.ReportIssue(
                        CreateDiagnostic(Rule,
                            attributeSyntax.GetLocation(),
                            action,
                            exportedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                            attributeTargetType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
                },
                Language.SyntaxKind.Attribute);

        private static bool IsOfExportType(ITypeSymbol type, INamedTypeSymbol exportedType) =>
            type.GetSelfAndBaseTypes()
                .Union(type.AllInterfaces)
                .Any(currentType =>
                         exportedType.IsUnboundGenericType
                             ? currentType.OriginalDefinition.Equals(exportedType.ConstructedFrom)
                             : currentType.Equals(exportedType));

        private INamedTypeSymbol GetExportedTypeSymbol(SeparatedSyntaxList<TArgumentSyntax>? attributeArguments, SemanticModel semanticModel)
        {
            if (!attributeArguments.HasValue)
            {
                return null;
            }

            var arguments = attributeArguments.Value;
            if (arguments.Count != 1 && arguments.Count != 2)
            {
                return null;
            }

            var argumentSyntax = GetArgumentFromNamedArgument(arguments)
                                 ?? GetArgumentFromSingleArgumentAttribute(arguments)
                                 ?? GetArgumentFromDoubleArgumentAttribute(arguments, semanticModel);

            var typeOfOrGetTypeExpression = Language.Syntax.NodeExpression(argumentSyntax);
            var exportedTypeSyntax = GetTypeOfOrGetTypeExpression(typeOfOrGetTypeExpression);
            return exportedTypeSyntax == null
                       ? null
                       : semanticModel.GetSymbolInfo(exportedTypeSyntax).Symbol as INamedTypeSymbol;
        }

        private ITypeSymbol GetAttributeTargetSymbol(SyntaxNode syntaxNode, SemanticModel semanticModel) =>
            // Parent is AttributeListSyntax, we handle only class attributes
            !IsClassOrRecordSyntax(syntaxNode.Parent?.Parent) ? null : semanticModel.GetDeclaredSymbol(syntaxNode.Parent.Parent) as ITypeSymbol;

        private TArgumentSyntax GetArgumentFromNamedArgument(IEnumerable<TArgumentSyntax> arguments) =>
            arguments.FirstOrDefault(x => "contractType".Equals(Language.Syntax.NodeIdentifier(x)?.ValueText, Language.NameComparison));

        private TArgumentSyntax GetArgumentFromDoubleArgumentAttribute(SeparatedSyntaxList<TArgumentSyntax> arguments, SemanticModel semanticModel)
        {
            if (arguments.Count != 2)
            {
                return null;
            }

            if (Language.Syntax.NodeExpression(arguments[0]) is { } firstArgument && semanticModel.GetConstantValue(firstArgument).Value is string)
            {
                // Two arguments, second should be typeof expression
                return arguments[1];
            }

            return null;
        }

        private static TArgumentSyntax GetArgumentFromSingleArgumentAttribute(SeparatedSyntaxList<TArgumentSyntax> arguments) =>
            arguments.Count == 1 ? arguments[0] : null; // Only one argument, should be typeof expression
    }
}
