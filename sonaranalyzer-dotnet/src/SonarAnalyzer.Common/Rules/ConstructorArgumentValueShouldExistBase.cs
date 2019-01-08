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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace SonarAnalyzer.Rules
{
    public abstract class ConstructorArgumentValueShouldExistBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S4260";
        protected const string MessageFormat =
            "Change this 'ConstructorArgumentAttribute' value to match one of the existing constructors arguments.";

        protected static AttributeData GetConstructorArgumentAttributeOrDefault(IPropertySymbol propertySymbol)
        {
            var attributes = propertySymbol.GetAttributes(KnownType.System_Windows_Markup_ConstructorArgumentAttribute)
                .ToList();

            return attributes.Count == 1
                ? attributes[0]
                : null;
        }

        protected void CheckConstructorArgumentProperty(
            SyntaxNodeAnalysisContext c, SyntaxNode propertyDeclaration, IPropertySymbol propertySymbol)
        {
            if (propertySymbol == null)
            {
                return;
            }

            var constructorArgumentAttribute = GetConstructorArgumentAttributeOrDefault(propertySymbol);
            if (constructorArgumentAttribute == null ||
                constructorArgumentAttribute.ConstructorArguments.Length != 1)
            {
                return;
            }
            var specifiedName = constructorArgumentAttribute.ConstructorArguments[0].Value.ToString();
            if (!GetAllParentClassConstructorArgumentNames(propertyDeclaration).Any(n => n == specifiedName))
            {
                ReportIssue(c, constructorArgumentAttribute);
            }
        }

        protected abstract IEnumerable<string> GetAllParentClassConstructorArgumentNames(SyntaxNode propertyDeclaration);
        protected abstract void ReportIssue(SyntaxNodeAnalysisContext c, AttributeData constructorArgumentAttribute);
    }
}
