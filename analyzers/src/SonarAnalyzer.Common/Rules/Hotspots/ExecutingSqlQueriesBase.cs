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
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using InvocationCondition = SonarAnalyzer.Helpers.TrackingCondition<SonarAnalyzer.Helpers.InvocationContext>;

namespace SonarAnalyzer.Rules
{
    public abstract class ExecutingSqlQueriesBase<TSyntaxKind, TExpressionSyntax, TIdentifierNameSyntax> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TExpressionSyntax : SyntaxNode
        where TIdentifierNameSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S2077";
        protected const string AssignmentWithFormattingMessage = "SQL Query is dynamically formatted and assigned to {0}.";
        protected const string AssignmentMessage = "SQL query is assigned to {0}.";
        private const string MessageFormat = "Make sure using a dynamically formatted SQL query is safe here.";
        private const int FirstArgumentIndex = 0;
        private const int SecondArgumentIndex = 1;

        private readonly KnownType[] constructorsForFirstArgument =
            {
                KnownType.Microsoft_EntityFrameworkCore_RawSqlString,
                KnownType.System_Data_SqlClient_SqlCommand,
                KnownType.System_Data_SqlClient_SqlDataAdapter,
                KnownType.System_Data_Odbc_OdbcCommand,
                KnownType.System_Data_Odbc_OdbcDataAdapter,
                KnownType.System_Data_SqlServerCe_SqlCeCommand,
                KnownType.System_Data_SqlServerCe_SqlCeDataAdapter,
                KnownType.System_Data_OracleClient_OracleCommand,
                KnownType.System_Data_OracleClient_OracleDataAdapter,
                KnownType.MySql_Data_MySqlClient_MySqlCommand,
                KnownType.MySql_Data_MySqlClient_MySqlDataAdapter,
                KnownType.MySql_Data_MySqlClient_MySqlScript,
                KnownType.Microsoft_Data_Sqlite_SqliteCommand,
                KnownType.System_Data_Sqlite_SqliteCommand,
                KnownType.System_Data_Sqlite_SQLiteDataAdapter,
            };

        private readonly KnownType[] constructorsForSecondArgument =
            {
                KnownType.MySql_Data_MySqlClient_MySqlScript,
            };

        private readonly MemberDescriptor[] invocationsForFirstTwoArguments =
            {
                new MemberDescriptor(KnownType.Microsoft_EntityFrameworkCore_RelationalDatabaseFacadeExtensions, "ExecuteSqlCommandAsync"),
                new MemberDescriptor(KnownType.Microsoft_EntityFrameworkCore_RelationalDatabaseFacadeExtensions, "ExecuteSqlCommand"),
                new MemberDescriptor(KnownType.Microsoft_EntityFrameworkCore_RelationalQueryableExtensions, "FromSql"),
            };

        private readonly MemberDescriptor[] invocationsForFirstArgument =
            {
                new MemberDescriptor(KnownType.System_Data_Sqlite_SqliteCommand, "Execute"),
            };

        private readonly MemberDescriptor[] invocationsForSecondArgument =
            {
                new MemberDescriptor(KnownType.MySql_Data_MySqlClient_MySqlHelper, "ExecuteDataRow"),
                new MemberDescriptor(KnownType.MySql_Data_MySqlClient_MySqlHelper, "ExecuteDataRowAsync"),
                new MemberDescriptor(KnownType.MySql_Data_MySqlClient_MySqlHelper, "ExecuteDataset"),
                new MemberDescriptor(KnownType.MySql_Data_MySqlClient_MySqlHelper, "ExecuteDatasetAsync"),
                new MemberDescriptor(KnownType.MySql_Data_MySqlClient_MySqlHelper, "ExecuteNonQuery"),
                new MemberDescriptor(KnownType.MySql_Data_MySqlClient_MySqlHelper, "ExecuteNonQueryAsync"),
                new MemberDescriptor(KnownType.MySql_Data_MySqlClient_MySqlHelper, "ExecuteReader"),
                new MemberDescriptor(KnownType.MySql_Data_MySqlClient_MySqlHelper, "ExecuteReaderAsync"),
                new MemberDescriptor(KnownType.MySql_Data_MySqlClient_MySqlHelper, "ExecuteScalar"),
                new MemberDescriptor(KnownType.MySql_Data_MySqlClient_MySqlHelper, "ExecuteScalarAsync"),
                new MemberDescriptor(KnownType.MySql_Data_MySqlClient_MySqlHelper, "UpdateDataSet"),
                new MemberDescriptor(KnownType.MySql_Data_MySqlClient_MySqlHelper, "UpdateDataSetAsync"),
            };

        private readonly MemberDescriptor[] properties =
            {
                new MemberDescriptor(KnownType.System_Data_Odbc_OdbcCommand, "CommandText"),
                new MemberDescriptor(KnownType.System_Data_OracleClient_OracleCommand, "CommandText"),
                new MemberDescriptor(KnownType.System_Data_SqlClient_SqlCommand, "CommandText"),
                new MemberDescriptor(KnownType.System_Data_SqlServerCe_SqlCeCommand, "CommandText"),
                new MemberDescriptor(KnownType.MySql_Data_MySqlClient_MySqlCommand, "CommandText"),
                new MemberDescriptor(KnownType.Microsoft_Data_Sqlite_SqliteCommand, "CommandText"),
                new MemberDescriptor(KnownType.System_Data_Sqlite_SqliteCommand, "CommandText"),
            };

        protected abstract TExpressionSyntax GetInvocationExpression(SyntaxNode expression);
        protected abstract TExpressionSyntax GetArgumentAtIndex(InvocationContext context, int index);
        protected abstract TExpressionSyntax GetArgumentAtIndex(ObjectCreationContext context, int index);
        protected abstract TExpressionSyntax GetSetValue(PropertyAccessContext context);
        protected abstract bool IsTracked(TExpressionSyntax expression, BaseContext context);
        protected abstract bool IsSensitiveExpression(TExpressionSyntax expression, SemanticModel semanticModel);
        protected abstract Location SecondaryLocationForExpression(TExpressionSyntax node, string identifierName);
        protected abstract string GetIdentifierName(TIdentifierNameSyntax identifierNameSyntax);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected InvocationTracker<TSyntaxKind> InvocationTracker { get; set; }
        protected PropertyAccessTracker<TSyntaxKind> PropertyAccessTracker { get; set; }
        protected ObjectCreationTracker<TSyntaxKind> ObjectCreationTracker { get; set; }
        protected AssignmentFinder AssignmentFinder { get; set; }

        protected DiagnosticDescriptor Rule { get; }

        protected ExecutingSqlQueriesBase(System.Resources.ResourceManager rspecResources) =>
            Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources).WithNotConfigurable();

        protected override void Initialize(SonarAnalysisContext context)
        {
            InvocationTracker.Track(context,
                InvocationTracker.MatchMethod(invocationsForFirstTwoArguments),
                Conditions.And(
                    MethodHasRawSqlQueryParameter(),
                    Conditions.Or(ArgumentAtIndexIsTracked(0), ArgumentAtIndexIsTracked(1))
                ),
                Conditions.ExceptWhen(InvocationTracker.ArgumentAtIndexIsConstant(0)));

            TrackInvocations(context, invocationsForFirstArgument, FirstArgumentIndex);
            TrackInvocations(context, invocationsForSecondArgument, SecondArgumentIndex);

            PropertyAccessTracker.Track(context,
                PropertyAccessTracker.MatchProperty(properties),
                PropertyAccessTracker.MatchSetter(),
                c => IsTracked(GetSetValue(c), c),
                Conditions.ExceptWhen(PropertyAccessTracker.AssignedValueIsConstant()));

            TrackObjectCreation(context, constructorsForFirstArgument, FirstArgumentIndex);
            TrackObjectCreation(context, constructorsForSecondArgument, SecondArgumentIndex);
        }

        protected bool IsTrackedVariableDeclaration(TExpressionSyntax expression, BaseContext context)
        {
            if (expression is TIdentifierNameSyntax)
            {
                var node = expression;
                while (node is TIdentifierNameSyntax identifierNameSyntax)
                {
                    var identifierName = GetIdentifierName(identifierNameSyntax);
                    node = AssignmentFinder.FindLinearPrecedingAssignmentExpression(identifierName, node) as TExpressionSyntax;

                    if (IsSensitiveExpression(node, context.SemanticModel))
                    {
                        context.AddSecondaryLocation(new SecondaryLocation(SecondaryLocationForExpression(node, identifierName), string.Format(AssignmentWithFormattingMessage, identifierName)));
                        return true;
                    }
                    else
                    {
                        context.AddSecondaryLocation(new SecondaryLocation(SecondaryLocationForExpression(node, identifierName), string.Format(AssignmentMessage, identifierName)));
                    }
                }
            }

            return false;
        }

        private void TrackObjectCreation(SonarAnalysisContext context, KnownType[] objectCreationTypes, int argumentIndex) =>
            ObjectCreationTracker.Track(context,
                ObjectCreationTracker.MatchConstructor(objectCreationTypes),
                ObjectCreationTracker.ArgumentAtIndexIs(argumentIndex, KnownType.System_String),
                    c => IsTracked(GetArgumentAtIndex(c, argumentIndex), c),
                Conditions.ExceptWhen(ObjectCreationTracker.ArgumentAtIndexIsConst(argumentIndex)));

        private void TrackInvocations(SonarAnalysisContext context, MemberDescriptor[] incovationsDescriptors, int argumentIndex) =>
            InvocationTracker.Track(context,
                InvocationTracker.MatchMethod(incovationsDescriptors),
                ArgumentAtIndexIsTracked(argumentIndex),
                Conditions.ExceptWhen(InvocationTracker.ArgumentAtIndexIsConstant(argumentIndex)));

        private InvocationCondition MethodHasRawSqlQueryParameter() =>
            context =>
            {
                var invocationExpression = GetInvocationExpression(context.Node);

                return invocationExpression != null
                       && context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol is IMethodSymbol methodSymbol
                       && (ParameterIsRawString(methodSymbol, 0) || ParameterIsRawString(methodSymbol, 1));

                static bool ParameterIsRawString(IMethodSymbol method, int index) =>
                    method.Parameters.Length > index && method.Parameters[index].IsType(KnownType.Microsoft_EntityFrameworkCore_RawSqlString);
            };

        private InvocationCondition ArgumentAtIndexIsTracked(int index) =>
            context => IsTracked(GetArgumentAtIndex(context, index), context);
    }
}
