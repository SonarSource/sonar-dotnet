/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ExecutingSqlQueriesBase<TSyntaxKind, TExpressionSyntax> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TExpressionSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S2077";
        protected const string MessageFormat = "Make sure that executing SQL queries is safe here.";

        private readonly KnownType[] constructors = {
            KnownType.Microsoft_EntityFrameworkCore_RawSqlString,
            KnownType.System_Data_SqlClient_SqlCommand,
            KnownType.System_Data_SqlClient_SqlDataAdapter,
            KnownType.System_Data_Odbc_OdbcCommand,
            KnownType.System_Data_Odbc_OdbcDataAdapter,
            KnownType.System_Data_SqlServerCe_SqlCeCommand,
            KnownType.System_Data_SqlServerCe_SqlCeDataAdapter,
            KnownType.System_Data_OracleClient_OracleCommand,
            KnownType.System_Data_OracleClient_OracleDataAdapter
        };

        protected InvocationTracker<TSyntaxKind> InvocationTracker { get; set; }

        protected PropertyAccessTracker<TSyntaxKind> PropertyAccessTracker { get; set; }

        protected ObjectCreationTracker<TSyntaxKind> ObjectCreationTracker { get; set; }

        protected override void Initialize(SonarAnalysisContext context)
        {
            InvocationTracker.Track(context,
                InvocationTracker.MatchMethod(
                    new MemberDescriptor(KnownType.Microsoft_EntityFrameworkCore_RelationalQueryableExtensions, "FromSql")),
                Conditions.And(
                    MethodHasRawSqlQueryParameter(),
                    Conditions.Or(
                        Conditions.Or(ArgumentAtIndexIsConcat(0), ArgumentAtIndexIsFormat(0), ArgumentAtIndexIsInterpolated(0)),
                        Conditions.Or(ArgumentAtIndexIsConcat(1), ArgumentAtIndexIsFormat(1), ArgumentAtIndexIsInterpolated(1))
                    )
                ),
                Conditions.ExceptWhen(
                    InvocationTracker.ArgumentAtIndexIsConstant(0)));

            InvocationTracker.Track(context,
                InvocationTracker.MatchMethod(
                    new MemberDescriptor(KnownType.Microsoft_EntityFrameworkCore_RelationalDatabaseFacadeExtensions, "ExecuteSqlCommandAsync"),
                    new MemberDescriptor(KnownType.Microsoft_EntityFrameworkCore_RelationalDatabaseFacadeExtensions, "ExecuteSqlCommand")),
                Conditions.And(
                    MethodHasRawSqlQueryParameter(),
                    Conditions.Or(
                        Conditions.Or(ArgumentAtIndexIsConcat(0), ArgumentAtIndexIsFormat(0), ArgumentAtIndexIsInterpolated(0)),
                        Conditions.Or(ArgumentAtIndexIsConcat(1), ArgumentAtIndexIsFormat(1), ArgumentAtIndexIsInterpolated(1))
                    )
                ),
                Conditions.ExceptWhen(
                    InvocationTracker.ArgumentAtIndexIsConstant(0)));

            PropertyAccessTracker.Track(context,
                PropertyAccessTracker.MatchProperty(
                    new MemberDescriptor(KnownType.System_Data_Odbc_OdbcCommand, "CommandText"),
                    new MemberDescriptor(KnownType.System_Data_OracleClient_OracleCommand, "CommandText"),
                    new MemberDescriptor(KnownType.System_Data_SqlClient_SqlCommand, "CommandText"),
                    new MemberDescriptor(KnownType.System_Data_SqlServerCe_SqlCeCommand, "CommandText")),
                PropertyAccessTracker.MatchSetter(),
                Conditions.Or(SetterIsConcat(), SetterIsFormat(), SetterIsInterpolation()),
                Conditions.ExceptWhen(
                    PropertyAccessTracker.AssignedValueIsConstant()));

            ObjectCreationTracker.Track(context,
                ObjectCreationTracker.MatchConstructor(this.constructors),
                ObjectCreationTracker.ArgumentAtIndexIs(0, KnownType.System_String),
                Conditions.Or(FirstArgumentIsConcat(), FirstArgumentIsFormat(), FirstArgumentIsInterpolation()),
                Conditions.ExceptWhen(ObjectCreationTracker.ArgumentAtIndexIsConst(0)));
        }

        protected abstract TExpressionSyntax GetInvocationExpression(SyntaxNode expression);

        protected abstract TExpressionSyntax GetArgumentAtIndex(InvocationContext context, int index);

        protected abstract TExpressionSyntax GetSetValue(PropertyAccessContext context);

        protected abstract TExpressionSyntax GetFirstArgument(ObjectCreationContext context);

        protected abstract bool IsConcat(TExpressionSyntax argument, SemanticModel semanticModel);

        protected abstract bool IsInterpolated(TExpressionSyntax expression, SemanticModel semanticModel);

        protected abstract bool IsStringMethodInvocation(string methodName, TExpressionSyntax expression, SemanticModel semanticModel);

        private bool IsFormat(TExpressionSyntax argument, SemanticModel semanticModel) =>
            IsStringMethodInvocation("Format", argument, semanticModel);

        private InvocationCondition MethodHasRawSqlQueryParameter() =>
            context =>
            {
                var invocationExpression = GetInvocationExpression(context.Invocation);

                return invocationExpression != null &&
                       context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol is IMethodSymbol methodSymbol &&
                       (ParameterIsRawString(methodSymbol, 0) || ParameterIsRawString(methodSymbol, 1));

                static bool ParameterIsRawString(IMethodSymbol method, int index) =>
                    method.Parameters.Length > index && method.Parameters[index].IsType(KnownType.Microsoft_EntityFrameworkCore_RawSqlString);
            };

        private InvocationCondition ArgumentAtIndexIsInterpolated(int index) =>
            context => IsInterpolated(GetArgumentAtIndex(context, index), context.SemanticModel);

        private InvocationCondition ArgumentAtIndexIsConcat(int index) =>
            context => IsConcat(GetArgumentAtIndex(context, index), context.SemanticModel);

        private InvocationCondition ArgumentAtIndexIsFormat(int index) =>
            context => IsFormat(GetArgumentAtIndex(context, index), context.SemanticModel);

        private PropertyAccessCondition SetterIsConcat() =>
            context => IsConcat(GetSetValue(context), context.SemanticModel);

        private PropertyAccessCondition SetterIsFormat() =>
            context => IsFormat(GetSetValue(context), context.SemanticModel);

        private PropertyAccessCondition SetterIsInterpolation() =>
            context => IsInterpolated(GetSetValue(context), context.SemanticModel);

        private ObjectCreationCondition FirstArgumentIsConcat() =>
            context => IsConcat(GetFirstArgument(context), context.SemanticModel);

        private ObjectCreationCondition FirstArgumentIsFormat() =>
            context => IsFormat(GetFirstArgument(context), context.SemanticModel);

        private ObjectCreationCondition FirstArgumentIsInterpolation() =>
            context => IsInterpolated(GetFirstArgument(context), context.SemanticModel);
    }
}
