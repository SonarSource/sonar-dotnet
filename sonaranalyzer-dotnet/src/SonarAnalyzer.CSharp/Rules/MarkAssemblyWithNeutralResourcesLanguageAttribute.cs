/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using System.Collections.Immutable;
using System.Linq;
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
    public sealed class MarkAssemblyWithNeutralResourcesLanguageAttribute : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4026";
        private const string MessageFormat = "Mark this assembly with 'System.Resources.NeutralResourcesLanguageAttribute'.";

        private const string StronglyTypedResourceBuilder = "System.Resources.Tools.StronglyTypedResourceBuilder";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCompilationStartAction(
                c =>
                {
                    var hasResx = false;

                    c.RegisterSyntaxNodeAction(
                        cc =>
                        {
                            if (IsResxGeneratedFile(cc.SemanticModel, (ClassDeclarationSyntax)cc.Node))
                            {
                                hasResx = true;
                            }
                        }, SyntaxKind.ClassDeclaration);

                    c.RegisterCompilationEndAction(
                        cc =>
                        {
                            if (!hasResx || HasNeutralResourcesLanguageAttribute(cc.Compilation.Assembly))
                            {
                                return;
                            }

                            cc.ReportDiagnosticWhenActive(Diagnostic.Create(rule, null));
                        });
                });
        }

        private static bool IsDesignerFile(SyntaxTree tree)
        {
            return tree.FilePath?.IndexOf(".Designer.", StringComparison.OrdinalIgnoreCase) > 0;
        }

        private static bool HasGeneratedCodeAttributeWithStronglyTypedResourceBuilderValue(
            SemanticModel semanticModel, ClassDeclarationSyntax classSyntax)
        {
            return classSyntax.AttributeLists
                .SelectMany(list => list.Attributes)
                .Where(attribute => attribute.ArgumentList.Arguments.Count > 0)
                .Select(attribute => attribute.ToSyntaxWithSymbol(semanticModel.GetSymbolInfo(attribute).Symbol as IMethodSymbol))
                .Where(syntaxWithSymbol => syntaxWithSymbol.Symbol != null
                                           && syntaxWithSymbol.Symbol.ContainingType.Is(KnownType.System_CodeDom_Compiler_GeneratedCodeAttribute))
                .Select(syntaxWithSymbol => semanticModel.GetConstantValue(syntaxWithSymbol.Syntax.ArgumentList.Arguments[0].Expression))
                .Select(constantValue => constantValue.Value as string)
                .Any(stringValue => string.Equals(stringValue, StronglyTypedResourceBuilder, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsResxGeneratedFile(SemanticModel semanticModel, ClassDeclarationSyntax classSyntax)
        {
            if (!IsDesignerFile(semanticModel.SyntaxTree))
            {
                return false;
            }

            return HasGeneratedCodeAttributeWithStronglyTypedResourceBuilderValue(semanticModel, classSyntax);
        }

        private static bool HasNeutralResourcesLanguageAttribute(IAssemblySymbol assemblySymbol)
        {
            return assemblySymbol.GetAttributes()
                .Where(attribute => attribute.AttributeClass.Is(KnownType.System_Resources_NeutralResourcesLanguageAttribute))
                .Any(attribute => attribute.ConstructorArguments.Any(
                    constructorArg => constructorArg.Type.Is(KnownType.System_String)
                                      && !string.IsNullOrWhiteSpace((string)constructorArg.Value)));
        }
    }
}
