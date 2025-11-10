/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Rules
{
    public abstract class PartCreationPolicyShouldBeUsedWithExportAttributeBase<TAttributeSyntax, TDeclarationSyntax> : SonarDiagnosticAnalyzer
        where TAttributeSyntax : SyntaxNode
        where TDeclarationSyntax : SyntaxNode
    {
        internal const string DiagnosticId = "S4428";

        protected const string MessageFormat = "Add the 'ExportAttribute' or remove 'PartCreationPolicyAttribute' to/from this type definition.";

        protected abstract TDeclarationSyntax GetTypeDeclaration(TAttributeSyntax attribute);

        protected void AnalyzeNode(SonarSyntaxNodeReportingContext c)
        {
            var attribute = (TAttributeSyntax)c.Node;
            if (!IsPartCreationPolicyAttribute(attribute))
            {
                return;
            }

            var declaration = GetTypeDeclaration(attribute);
            if (declaration == null)
            {
                return;
            }

            if (!(c.Model.GetDeclaredSymbol(declaration) is ITypeSymbol symbol)
                || symbol.AnyAttributeDerivesFrom(KnownType.System_ComponentModel_Composition_ExportAttribute)
                || symbol.GetSelfAndBaseTypes()
                         .Union(symbol.AllInterfaces)
                         .Any(s => s.AnyAttributeDerivesFrom(KnownType.System_ComponentModel_Composition_InheritedExportAttribute)))
            {
                return;
            }

            c.ReportIssue(SupportedDiagnostics[0], attribute);

            bool IsPartCreationPolicyAttribute(TAttributeSyntax attributeSyntax) =>
                c.Model.GetSymbolInfo(attributeSyntax).Symbol is IMethodSymbol attributeSymbol
                && attributeSymbol.ContainingType.Is(KnownType.System_ComponentModel_Composition_PartCreationPolicyAttribute);
        }
    }
}
