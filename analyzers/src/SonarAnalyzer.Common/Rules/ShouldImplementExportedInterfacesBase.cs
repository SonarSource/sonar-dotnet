/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.Common
{
    public abstract class ShouldImplementExportedInterfacesBase<TArgumentSyntax, TAttributeSyntax, TSyntaxKind> : SonarDiagnosticAnalyzer
        where TArgumentSyntax : SyntaxNode
        where TAttributeSyntax : SyntaxNode
        where TSyntaxKind : struct
    {
        internal const string DiagnosticId = "S4159";
        private const string MessageFormat = "{0} '{1}' on '{2}' or remove this export attribute.";
        private const string ActionForInterface = "Implement";
        private const string ActionForClass = "Derive from";

        private readonly DiagnosticDescriptor rule;
        private readonly ImmutableArray<KnownType> exportAttributes =
            ImmutableArray.Create(
                KnownType.System_ComponentModel_Composition_ExportAttribute,
                KnownType.System_ComponentModel_Composition_InheritedExportAttribute);

        protected abstract SeparatedSyntaxList<TArgumentSyntax>? GetAttributeArguments(TAttributeSyntax attributeSyntax);
        protected abstract SyntaxNode GetAttributeName(TAttributeSyntax attributeSyntax);
        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
        protected abstract bool IsClassOrRecordSyntax(SyntaxNode syntaxNode);
        // Retrieve the expression inside of the typeof()/GetType() (e.g. typeof(Foo) => Foo)
        protected abstract SyntaxNode GetTypeOfOrGetTypeExpression(SyntaxNode expressionSyntax);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected ShouldImplementExportedInterfacesBase() =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, Language.RspecResources);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer,
                c =>
                {
                    var attributeSyntax = (TAttributeSyntax)c.Node;

                    if (!(c.SemanticModel.GetSymbolInfo(GetAttributeName(attributeSyntax)).Symbol is IMethodSymbol attributeCtorSymbol)
                        || !attributeCtorSymbol.ContainingType.IsAny(exportAttributes))
                    {
                        return;
                    }

                    var exportedType = GetExportedTypeSymbol(GetAttributeArguments(attributeSyntax), c.SemanticModel);
                    var attributeTargetType = GetAttributeTargetSymbol(attributeSyntax, c.SemanticModel);

                    if (exportedType != null && attributeTargetType != null && !IsOfExportType(attributeTargetType, exportedType))
                    {
                        var action = exportedType.IsInterface()
                            ? ActionForInterface
                            : ActionForClass;

                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, attributeSyntax.GetLocation(), action,
                            exportedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                            attributeTargetType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
                    }
                },
                Language.SyntaxKind.Attribute);

        private static bool IsOfExportType(ITypeSymbol type, INamedTypeSymbol exportedType) =>
            type.GetSelfAndBaseTypes()
                .Union(type.AllInterfaces)
                .Any(currentType => currentType.Equals(exportedType));

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
            return exportedTypeSyntax == null ? null : semanticModel.GetSymbolInfo(exportedTypeSyntax).Symbol as INamedTypeSymbol;
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
            arguments.Count != 1 ? null : arguments[0]; // Only one argument, should be typeof expression
    }
}
