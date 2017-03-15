/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using System.Linq;
using System.Text.RegularExpressions;
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
    public class AvoidExcessiveInheritance : ParameterLoadingDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S110";
        private const string MessageFormat = "This class has {0} parents which is greater than {1} authorized.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        protected sealed override DiagnosticDescriptor Rule => rule;

        private const int MaximumDepthDefaultValue = 5;
        [RuleParameter(
            key: "max",
            type: PropertyType.Integer,
            description: "Maximum depth of the inheritance tree. (Number)",
            defaultValue: MaximumDepthDefaultValue)]
        public int MaximumDepth { get; set; } = MaximumDepthDefaultValue;

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var declaration = (ClassDeclarationSyntax)c.Node;
                    var symbol = c.SemanticModel.GetDeclaredSymbol(declaration);
                    if (symbol == null)
                    {
                        return;
                    }

                    var baseTypesCount = symbol.GetSelfAndBaseTypes().Count() - 1; // remove the class itself
                    if (baseTypesCount > MaximumDepth)
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, declaration.Identifier.GetLocation(),
                            baseTypesCount, MaximumDepth));
                    }

                }, SyntaxKind.ClassDeclaration);
        }
    }
}