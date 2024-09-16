/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.Rules
{
    public abstract class ExecutingSqlQueriesBase<TSyntaxKind, TExpressionSyntax, TIdentifierNameSyntax> : TrackerHotspotDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
        where TExpressionSyntax : SyntaxNode
        where TIdentifierNameSyntax : SyntaxNode
    {
        private const string DiagnosticId = "S2077";
        private const string AssignmentWithFormattingMessage = "SQL Query is dynamically formatted and assigned to {0}.";
        private const string AssignmentMessage = "SQL query is assigned to {0}.";
        private const string MessageFormat = "Make sure using a dynamically formatted SQL query is safe here.";
        private const int FirstArgumentIndex = 0;
        private const int SecondArgumentIndex = 1;

        private readonly KnownType[] constructorsForFirstArgument =
            {
                KnownType.System_Data_Odbc_OdbcCommand,
                KnownType.System_Data_Odbc_OdbcDataAdapter,
                KnownType.System_Data_SqlClient_SqlCommand,
                KnownType.System_Data_SqlClient_SqlDataAdapter,
                KnownType.System_Data_Sqlite_SqliteCommand,
                KnownType.System_Data_Sqlite_SQLiteDataAdapter,
                KnownType.System_Data_SqlServerCe_SqlCeCommand,
                KnownType.System_Data_SqlServerCe_SqlCeDataAdapter,
                KnownType.System_Data_OracleClient_OracleCommand,
                KnownType.System_Data_OracleClient_OracleDataAdapter,
                KnownType.MySql_Data_MySqlClient_MySqlCommand,
                KnownType.MySql_Data_MySqlClient_MySqlDataAdapter,
                KnownType.MySql_Data_MySqlClient_MySqlScript,
                KnownType.Microsoft_Data_Sqlite_SqliteCommand,
                KnownType.Mono_Data_Sqlite_SqliteCommand,
                KnownType.Mono_Data_Sqlite_SqliteDataAdapter,
                KnownType.Microsoft_EntityFrameworkCore_RawSqlString,
                KnownType.Dapper_CommandDefinition
            };

        private readonly KnownType[] constructorsForSecondArgument =
            {
                KnownType.MySql_Data_MySqlClient_MySqlScript
            };

        private readonly MemberDescriptor[] invocationsForFirstTwoArguments =
            {
                new(KnownType.Microsoft_EntityFrameworkCore_RelationalDatabaseFacadeExtensions, "ExecuteSqlCommandAsync"),
                new(KnownType.Microsoft_EntityFrameworkCore_RelationalDatabaseFacadeExtensions, "ExecuteSqlCommand"),
                new(KnownType.Microsoft_EntityFrameworkCore_RelationalQueryableExtensions, "FromSql")
            };

        private readonly MemberDescriptor[] invocationsForFirstTwoArgumentsAfterV2 =
            {
                new(KnownType.Microsoft_EntityFrameworkCore_RelationalDatabaseFacadeExtensions, "ExecuteSqlRaw"),
                new(KnownType.Microsoft_EntityFrameworkCore_RelationalDatabaseFacadeExtensions, "ExecuteSqlRawAsync"),
                new(KnownType.Microsoft_EntityFrameworkCore_RelationalQueryableExtensions, "FromSqlRaw"),
                new(KnownType.Dapper_SqlMapper, "Execute"),
                new(KnownType.Dapper_SqlMapper, "ExecuteAsync"),
                new(KnownType.Dapper_SqlMapper, "ExecuteReader"),
                new(KnownType.Dapper_SqlMapper, "ExecuteReaderAsync"),
                new(KnownType.Dapper_SqlMapper, "ExecuteScalar"),
                new(KnownType.Dapper_SqlMapper, "ExecuteScalarAsync"),
                new(KnownType.Dapper_SqlMapper, "Query"),
                new(KnownType.Dapper_SqlMapper, "QueryAsync"),
                new(KnownType.Dapper_SqlMapper, "QueryFirst"),
                new(KnownType.Dapper_SqlMapper, "QueryFirstAsync"),
                new(KnownType.Dapper_SqlMapper, "QueryFirstOrDefault"),
                new(KnownType.Dapper_SqlMapper, "QueryFirstOrDefaultAsync"),
                new(KnownType.Dapper_SqlMapper, "QueryMultiple"),
                new(KnownType.Dapper_SqlMapper, "QueryMultipleAsync"),
                new(KnownType.Dapper_SqlMapper, "QuerySingle"),
                new(KnownType.Dapper_SqlMapper, "QuerySingleAsync"),
                new(KnownType.Dapper_SqlMapper, "QuerySingleOrDefault"),
                new(KnownType.Dapper_SqlMapper, "QuerySingleOrDefaultAsync"),
                new(KnownType.System_Data_Entity_Database, "SqlQuery"),
                new(KnownType.System_Data_Entity_Database, "ExecuteSqlCommand"),
                new(KnownType.System_Data_Entity_Database, "ExecuteSqlCommandAsync"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApi, "Select"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApi, "SelectLazy"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApi, "SelectNonDefaults"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApi, "Single"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApi, "Scalar"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApi, "Column"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApi, "ColumnLazy"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApi, "ColumnDistinct"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApi, "Lookup"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApi, "Dictionary"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApi, "Exists"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApi, "SqlList"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApi, "SqlColumn"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApi, "SqlScalar"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApi, "ExecuteNonQuery"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApiAsync, "SelectAsync"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApiAsync, "SelectNonDefaultsAsync"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApiAsync, "ScalarAsync"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApiAsync, "SingleAsync"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApiAsync, "ColumnAsync"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApiAsync, "ColumnDistinctAsync"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApiAsync, "LookupAsync"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApiAsync, "DictionaryAsync"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApiAsync, "ExistsAsync"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApiAsync, "SqlListAsync"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApiAsync, "SqlColumnAsync"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApiAsync, "SqlScalarAsync"),
                new(KnownType.ServiceStack_OrmLite_OrmLiteReadApiAsync, "ExecuteNonQueryAsync")
            };

        private readonly MemberDescriptor[] invocationsForFirstArgument =
            {
                new(KnownType.System_Data_Sqlite_SqliteCommand, "Execute"),
                new(KnownType.System_Data_Entity_DbSet, "SqlQuery"),
                new(KnownType.System_Data_Entity_DbSet_TEntity, "SqlQuery"),
                new(KnownType.NHibernate_ISession, "CreateQuery"),
                new(KnownType.NHibernate_ISession, "CreateSQLQuery"),
                new(KnownType.NHibernate_ISession, "Delete"),
                new(KnownType.NHibernate_ISession, "DeleteAsync"),
                new(KnownType.NHibernate_ISession, "GetNamedQuery"),
                new(KnownType.NHibernate_Impl_AbstractSessionImpl, "CreateQuery"),
                new(KnownType.NHibernate_Impl_AbstractSessionImpl, "CreateSQLQuery"),
                new(KnownType.NHibernate_Impl_AbstractSessionImpl, "GetNamedQuery"),
                new(KnownType.NHibernate_Impl_AbstractSessionImpl, "GetNamedSQLQuery")
            };

        private readonly MemberDescriptor[] invocationsForSecondArgument =
            {
                new(KnownType.MySql_Data_MySqlClient_MySqlHelper, "ExecuteDataRow"),
                new(KnownType.MySql_Data_MySqlClient_MySqlHelper, "ExecuteDataRowAsync"),
                new(KnownType.MySql_Data_MySqlClient_MySqlHelper, "ExecuteDataset"),
                new(KnownType.MySql_Data_MySqlClient_MySqlHelper, "ExecuteDatasetAsync"),
                new(KnownType.MySql_Data_MySqlClient_MySqlHelper, "ExecuteNonQuery"),
                new(KnownType.MySql_Data_MySqlClient_MySqlHelper, "ExecuteNonQueryAsync"),
                new(KnownType.MySql_Data_MySqlClient_MySqlHelper, "ExecuteReader"),
                new(KnownType.MySql_Data_MySqlClient_MySqlHelper, "ExecuteReaderAsync"),
                new(KnownType.MySql_Data_MySqlClient_MySqlHelper, "ExecuteScalar"),
                new(KnownType.MySql_Data_MySqlClient_MySqlHelper, "ExecuteScalarAsync"),
                new(KnownType.MySql_Data_MySqlClient_MySqlHelper, "UpdateDataSet"),
                new(KnownType.MySql_Data_MySqlClient_MySqlHelper, "UpdateDataSetAsync"),
                new(KnownType.NHibernate_ISession, "CreateFilter"),
                new(KnownType.NHibernate_ISession, "CreateFilterAsync")
            };

        private readonly MemberDescriptor[] properties =
            {
                new(KnownType.System_Data_IDbCommand, "CommandText")
            };

        protected abstract TExpressionSyntax GetArgumentAtIndex(InvocationContext context, int index);
        protected abstract TExpressionSyntax GetArgumentAtIndex(ObjectCreationContext context, int index);
        protected abstract TExpressionSyntax GetSetValue(PropertyAccessContext context);
        protected abstract bool IsTracked(TExpressionSyntax expression, SyntaxBaseContext context);
        protected abstract bool IsSensitiveExpression(TExpressionSyntax expression, SemanticModel semanticModel);
        protected abstract Location SecondaryLocationForExpression(TExpressionSyntax node, string identifierNameToFind, out string identifierNameFound);

        protected ExecutingSqlQueriesBase(IAnalyzerConfiguration configuration) : base(configuration, DiagnosticId, MessageFormat) { }

        protected override void Initialize(TrackerInput input)
        {
            var inv = Language.Tracker.Invocation;
            inv.Track(input,
                inv.MatchMethod(invocationsForFirstTwoArguments),
                inv.And(MethodHasRawSqlQueryParameter(), inv.Or(ArgumentAtIndexIsTracked(0), ArgumentAtIndexIsTracked(1))),
                inv.ExceptWhen(inv.ArgumentAtIndexIsStringConstant(0)));

            inv.Track(input,
                inv.MatchMethod(invocationsForFirstTwoArgumentsAfterV2),
                inv.Or(ArgumentAtIndexIsTracked(0), ArgumentAtIndexIsTracked(1)),
                inv.ExceptWhen(inv.ArgumentAtIndexIsStringConstant(0)));

            TrackInvocations(input, invocationsForFirstArgument, FirstArgumentIndex);
            TrackInvocations(input, invocationsForSecondArgument, SecondArgumentIndex);

            var pa = Language.Tracker.PropertyAccess;
            pa.Track(input,
                pa.MatchProperty(true, properties),
                pa.MatchSetter(),
                c => IsTracked(GetSetValue(c), c),
                pa.ExceptWhen(pa.AssignedValueIsConstant()));

            TrackObjectCreation(input, constructorsForFirstArgument, FirstArgumentIndex);
            TrackObjectCreation(input, constructorsForSecondArgument, SecondArgumentIndex);
        }

        protected bool IsTrackedVariableDeclaration(TExpressionSyntax expression, SyntaxBaseContext context)
        {
            var node = expression;
            while (node is TIdentifierNameSyntax identifierNameSyntax)
            {
                var identifierName = Language.Syntax.NodeIdentifier(identifierNameSyntax).Value.ValueText;
                node = Language.AssignmentFinder.FindLinearPrecedingAssignmentExpression(identifierName, node) as TExpressionSyntax;

                var location = SecondaryLocationForExpression(node, identifierName, out var foundName);
                if (IsSensitiveExpression(node, context.SemanticModel))
                {
                    context.AddSecondaryLocation(location, AssignmentWithFormattingMessage, foundName);
                    return true;
                }
                else
                {
                    context.AddSecondaryLocation(location, AssignmentMessage, foundName);
                }
            }
            return false;
        }

        private void TrackObjectCreation(TrackerInput input, KnownType[] objectCreationTypes, int argumentIndex)
        {
            var t = Language.Tracker.ObjectCreation;
            t.Track(input,
                t.MatchConstructor(objectCreationTypes),
                t.ArgumentAtIndexIs(argumentIndex, KnownType.System_String),
                c => IsTracked(GetArgumentAtIndex(c, argumentIndex), c),
                t.ExceptWhen(t.ArgumentAtIndexIsConst(argumentIndex)));
        }

        private void TrackInvocations(TrackerInput input, MemberDescriptor[] invocationsDescriptors, int argumentIndex)
        {
            var t = Language.Tracker.Invocation;
            t.Track(input,
                t.MatchMethod(invocationsDescriptors),
                ArgumentAtIndexIsTracked(argumentIndex),
                t.ExceptWhen(t.ArgumentAtIndexIsStringConstant(argumentIndex)));
        }

        private TrackerBase<TSyntaxKind, InvocationContext>.Condition MethodHasRawSqlQueryParameter() =>
            context =>
            {
                return Language.Syntax.NodeExpression(context.Node) is { } invocationExpression
                       && context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol is IMethodSymbol methodSymbol
                       && (ParameterIsRawString(methodSymbol, 0) || ParameterIsRawString(methodSymbol, 1));

                static bool ParameterIsRawString(IMethodSymbol method, int index) =>
                    method.Parameters.Length > index && method.Parameters[index].IsType(KnownType.Microsoft_EntityFrameworkCore_RawSqlString);
            };

        private TrackerBase<TSyntaxKind, InvocationContext>.Condition ArgumentAtIndexIsTracked(int index) =>
            context => IsTracked(GetArgumentAtIndex(context, index), context);
    }
}
