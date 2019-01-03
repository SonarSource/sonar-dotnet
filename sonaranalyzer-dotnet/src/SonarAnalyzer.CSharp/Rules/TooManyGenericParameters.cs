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
    public class TooManyGenericParameters : ParameterLoadingDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2436";
        internal const string MessageFormat = "Reduce the number of generic parameters in the '{0}' {1} to no more than the {2} authorized.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager,
                isEnabledByDefault: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private const int DefaultMaxNumberOfGenericParametersInClass = 2;
        [RuleParameter("max", PropertyType.Integer, "Maximum authorized number of generic parameters.",
            DefaultMaxNumberOfGenericParametersInClass)]
        public int MaxNumberOfGenericParametersInClass { get; set; } = DefaultMaxNumberOfGenericParametersInClass;

        private const int DefaultMaxNumberOfGenericParametersInMethod = 3;
        [RuleParameter("maxMethod", PropertyType.Integer, "Maximum authorized number of generic parameters for methods.",
            DefaultMaxNumberOfGenericParametersInMethod)]
        public int MaxNumberOfGenericParametersInMethod { get; set; } = DefaultMaxNumberOfGenericParametersInMethod;

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)c.Node;

                    if (classDeclaration.TypeParameterList == null ||
                        classDeclaration.TypeParameterList.Parameters.Count <= MaxNumberOfGenericParametersInClass)
                    {
                        return;
                    }

                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, classDeclaration.Identifier.GetLocation(),
                        classDeclaration.Identifier.ValueText, "class", MaxNumberOfGenericParametersInClass));
                },
                SyntaxKind.ClassDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)c.Node;

                    if (methodDeclaration.TypeParameterList == null ||
                        methodDeclaration.TypeParameterList.Parameters.Count <= MaxNumberOfGenericParametersInMethod)
                    {
                        return;
                    }

                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, methodDeclaration.Identifier.GetLocation(),
                        $"{GetEnclosingTypeName(methodDeclaration)}.{methodDeclaration.Identifier.ValueText}", "method",
                        MaxNumberOfGenericParametersInMethod));
                },
                SyntaxKind.MethodDeclaration);
        }

        private string GetEnclosingTypeName(MethodDeclarationSyntax methodDeclaration)
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

                    default:
                        parent = methodDeclaration.Parent;
                        break;
                }
            }

            return null;
        }
    }
}
