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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ReviewSqlQueriesForSecurityVulnerabilities : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3649";
        private const string MessageFormat = "Make sure to sanitize the parameters of this SQL command.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<KnownType> checkedTypes = new HashSet<KnownType>
        {
            KnownType.System_Data_Odbc_OdbcCommand,
            KnownType.System_Data_Odbc_OdbcDataAdapter,
            KnownType.System_Data_OleDb_OleDbCommand,
            KnownType.System_Data_OleDb_OleDbDataAdapter,
            KnownType.Oracle_ManagedDataAccess_Client_OracleCommand,
            KnownType.Oracle_ManagedDataAccess_Client_OracleDataAdapter,
            KnownType.System_Data_SqlServerCe_SqlCeCommand,
            KnownType.System_Data_SqlServerCe_SqlCeDataAdapter,
            KnownType.System_Data_SqlClient_SqlCommand,
            KnownType.System_Data_SqlClient_SqlDataAdapter
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var objectCreation = (ObjectCreationExpressionSyntax)c.Node;
                    var methodSymbol = c.SemanticModel.GetSymbolInfo(objectCreation).Symbol as IMethodSymbol;

                    if (methodSymbol != null &&
                        methodSymbol.IsConstructor() &&
                        methodSymbol.ContainingType.IsAny(checkedTypes) &&
                        methodSymbol.Parameters.FirstOrDefault()?.Type.Is(KnownType.System_String) == true &&
                        !IsSanitizedQuery(objectCreation.ArgumentList.Arguments[0].Expression, c.SemanticModel))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(rule, objectCreation.Type.GetLocation()));
                    }
                }, SyntaxKind.ObjectCreationExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var assignment = (AssignmentExpressionSyntax)c.Node;
                    var propertySymbol = c.SemanticModel.GetSymbolInfo(assignment.Left).Symbol as IPropertySymbol;

                    if (propertySymbol != null &&
                        propertySymbol.Name == "CommandText" &&
                        propertySymbol.ContainingType.IsAny(checkedTypes) &&
                        !IsSanitizedQuery(assignment.Right, c.SemanticModel))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(rule, assignment.Left.GetLocation()));
                    }
                }, SyntaxKind.SimpleAssignmentExpression);
        }

        private static bool IsSanitizedQuery(ExpressionSyntax expression, SemanticModel model)
        {
            return model.GetConstantValue(expression).HasValue;
        }
    }
}
