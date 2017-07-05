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

using System.Collections.Generic;
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
    public sealed class SetLocaleForDataTypes : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4057";
        private const string MessageFormat = "Set the locale for this '{0}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<KnownType> invalidTypes = new HashSet<KnownType>
        {
            KnownType.System_Data_DataTable,
            KnownType.System_Data_DataSet
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var objectCreation = (ObjectCreationExpressionSyntax)c.Node;
                    var objectType = c.SemanticModel.GetSymbolInfo(objectCreation.Type).Symbol as ITypeSymbol;

                    if (objectType == null ||
                        !objectType.IsAny(invalidTypes))
                    {
                        return;
                    }

                    var variableDeclarator = objectCreation.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
                    if (variableDeclarator != null)
                    {
                        
                    }                   

                    c.ReportDiagnostic(Diagnostic.Create(rule, objectCreation.GetLocation()));
                }, SyntaxKind.ObjectCreationExpression);
        }

        private static string Get(ExpressionSyntax expression)
        {
            var identifier = expression as IdentifierNameSyntax;
            if (identifier != null)
            {
                return identifier.Identifier.ValueText;
            }
        }
    }
}
