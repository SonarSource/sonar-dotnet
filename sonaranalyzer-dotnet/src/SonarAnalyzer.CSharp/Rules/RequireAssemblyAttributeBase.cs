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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class RequireAssemblyAttributeBase : SonarDiagnosticAnalyzer
    {
        protected abstract bool ReportsOnTestSource { get; }
        protected abstract bool IsRequiredAttribute(AttributeSyntax attribute, SemanticModel semanticModel);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCompilationStartAction(c =>
            {
                if (!ReportsOnTestSource && c.Compilation.IsTest())
                {
                    return;
                }

                var shouldRaise = true;

                c.RegisterSemanticModelAction(cc =>
                {
                    var requiredAttributeFound = cc.SemanticModel.SyntaxTree.GetRoot()
                        .DescendantNodes()
                        .OfType<AttributeListSyntax>()
                        .SelectMany(list => list.Attributes)
                        .Any(a => IsRequiredAttribute(a, cc.SemanticModel));

                    if (requiredAttributeFound)
                    {
                        shouldRaise = false;
                    }
                });

                c.RegisterCompilationEndAction(cc =>
                {
                    if (shouldRaise)
                    {
                        cc.ReportDiagnostic(Diagnostic.Create(Rule, null));
                    }
                });
            });
        }
    }
}
