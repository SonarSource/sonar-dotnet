/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class PartCreationPolicyShouldBeUsedWithExportAttributeBase<TAttributeSyntax, TClassSyntax> : SonarDiagnosticAnalyzer
        where TAttributeSyntax : SyntaxNode
        where TClassSyntax : SyntaxNode
    {
        internal const string DiagnosticId = "S4428";

        protected const string MessageFormat = "Add the 'ExportAttribute' or remove 'PartCreationPolicyAttribute'" +
            " to/from this class definition.";

        protected abstract TClassSyntax GetClassDeclaration(TAttributeSyntax attribute);

        protected void AnalyzeNode(SyntaxNodeAnalysisContext c)
        {
            var attribute = (TAttributeSyntax)c.Node;
            if (!IsPartCreationPolicyAttribute(attribute))
            {
                return;
            }

            var classDeclaration = GetClassDeclaration(attribute);
            if (classDeclaration == null)
            {
                return;
            }

            var classSymbol = c.SemanticModel.GetDeclaredSymbol(classDeclaration) as ITypeSymbol;
            if (classSymbol == null ||
                classSymbol.AnyAttributeDerivesFrom(KnownType.System_ComponentModel_Composition_ExportAttribute) ||
                classSymbol.GetSelfAndBaseTypes()
                    .Union(classSymbol.AllInterfaces)
                    .Any(s => s.AnyAttributeDerivesFrom(KnownType.System_ComponentModel_Composition_InheritedExportAttribute)))
            {
                return;
            }

            c.ReportDiagnosticWhenActive(
                Diagnostic.Create(
                    SupportedDiagnostics[0],
                    attribute.GetLocation()));

            bool IsPartCreationPolicyAttribute(TAttributeSyntax attributeSyntax) =>
                c.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is IMethodSymbol attributeSymbol &&
                attributeSymbol.ContainingType.Is(KnownType.System_ComponentModel_Composition_PartCreationPolicyAttribute);
        }
    }
}
