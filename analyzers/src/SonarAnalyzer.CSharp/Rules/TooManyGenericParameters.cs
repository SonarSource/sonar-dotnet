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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class TooManyGenericParameters : ParameterLoadingDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2436";
        private const string MessageFormat = "Reduce the number of generic parameters in the '{0}' {1} to no more than the {2} authorized.";
        private const int DefaultMaxNumberOfGenericParametersInClass = 2;
        private const int DefaultMaxNumberOfGenericParametersInMethod = 3;

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager, isEnabledByDefault: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        [RuleParameter("max", PropertyType.Integer, "Maximum authorized number of generic parameters.", DefaultMaxNumberOfGenericParametersInClass)]
        public int MaxNumberOfGenericParametersInClass { get; set; } = DefaultMaxNumberOfGenericParametersInClass;

        [RuleParameter("maxMethod", PropertyType.Integer, "Maximum authorized number of generic parameters for methods.", DefaultMaxNumberOfGenericParametersInMethod)]
        public int MaxNumberOfGenericParametersInMethod { get; set; } = DefaultMaxNumberOfGenericParametersInMethod;

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var typeDeclaration = (TypeDeclarationSyntax)c.Node;

                    if (c.ContainingSymbol.Kind != SymbolKind.NamedType
                        || typeDeclaration.TypeParameterList == null
                        || typeDeclaration.TypeParameterList.Parameters.Count <= MaxNumberOfGenericParametersInClass)
                    {
                        return;
                    }

                    c.ReportIssue(Diagnostic.Create(Rule, typeDeclaration.Identifier.GetLocation(),
                        typeDeclaration.Identifier.ValueText, typeDeclaration.GetDeclarationTypeName(), MaxNumberOfGenericParametersInClass));
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKindEx.RecordClassDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)c.Node;

                    if (methodDeclaration.TypeParameterList == null
                        || methodDeclaration.TypeParameterList.Parameters.Count <= MaxNumberOfGenericParametersInMethod)
                    {
                        return;
                    }

                    c.ReportIssue(Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation(),
                        $"{GetEnclosingTypeName(methodDeclaration)}.{methodDeclaration.Identifier.ValueText}", "method",
                        MaxNumberOfGenericParametersInMethod));
                },
                SyntaxKind.MethodDeclaration);
        }

        private static string GetEnclosingTypeName(MethodDeclarationSyntax methodDeclaration)
        {
            var parent = methodDeclaration.Parent;

            while (parent != null)
            {
                switch (parent.Kind())
                {
                    case SyntaxKind.ClassDeclaration:
                        return ((ClassDeclarationSyntax)parent).Identifier.ValueText;

                    case SyntaxKind.StructDeclaration:
                        return ((StructDeclarationSyntax)parent).Identifier.ValueText;

                    case SyntaxKind.InterfaceDeclaration:
                        return ((InterfaceDeclarationSyntax)parent).Identifier.ValueText;

                    case SyntaxKindEx.RecordClassDeclaration:
                        return ((RecordDeclarationSyntaxWrapper)parent).Identifier.ValueText;

                    default:
                        parent = parent.Parent;
                        break;
                }
            }
            return null;
        }
    }
}
