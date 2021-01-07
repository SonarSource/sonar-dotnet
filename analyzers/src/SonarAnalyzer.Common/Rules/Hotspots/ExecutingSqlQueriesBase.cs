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
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ExecutingSqlQueriesBase<TSyntaxKind, TExpressionSyntax> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TExpressionSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S2077";
        private const string MessageFormat = "Make sure that formatting this SQL query is safe here.";

        private readonly KnownType[] constructors =
            {
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

        protected abstract TExpressionSyntax GetInvocationExpression(SyntaxNode expression);
        protected abstract TExpressionSyntax GetArgumentAtIndex(InvocationContext context, int index);
        protected abstract TExpressionSyntax GetSetValue(PropertyAccessContext context);
        protected abstract TExpressionSyntax GetFirstArgument(ObjectCreationContext context);
        protected abstract bool IsTracked(TExpressionSyntax argument, SemanticModel semanticModel);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected InvocationTracker<TSyntaxKind> InvocationTracker { get; set; }
        protected PropertyAccessTracker<TSyntaxKind> PropertyAccessTracker { get; set; }
        protected ObjectCreationTracker<TSyntaxKind> ObjectCreationTracker { get; set; }

        protected DiagnosticDescriptor Rule { get; }

        protected ExecutingSqlQueriesBase(System.Resources.ResourceManager rspecResources) =>
            Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources).WithNotConfigurable();

        protected override void Initialize(SonarAnalysisContext context)
        {
            InvocationTracker.Track(context,
                InvocationTracker.MatchMethod(
                    new MemberDescriptor(KnownType.Microsoft_EntityFrameworkCore_RelationalDatabaseFacadeExtensions, "ExecuteSqlCommandAsync"),
                    new MemberDescriptor(KnownType.Microsoft_EntityFrameworkCore_RelationalDatabaseFacadeExtensions, "ExecuteSqlCommand"),
                    new MemberDescriptor(KnownType.Microsoft_EntityFrameworkCore_RelationalQueryableExtensions, "FromSql")),
                Conditions.And(
                    MethodHasRawSqlQueryParameter(),
                    Conditions.Or(ArgumentAtIndexIsTracked(0), ArgumentAtIndexIsTracked(1))
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
                c => IsTracked(GetSetValue(c), c.SemanticModel),
                Conditions.ExceptWhen(
                    PropertyAccessTracker.AssignedValueIsConstant()));

            ObjectCreationTracker.Track(context,
                ObjectCreationTracker.MatchConstructor(this.constructors),
                ObjectCreationTracker.ArgumentAtIndexIs(0, KnownType.System_String),
                 c => IsTracked(GetFirstArgument(c), c.SemanticModel),
                Conditions.ExceptWhen(ObjectCreationTracker.ArgumentAtIndexIsConst(0)));
        }

        private InvocationCondition MethodHasRawSqlQueryParameter() =>
            context =>
            {
                var invocationExpression = GetInvocationExpression(context.Invocation);

                return invocationExpression != null
                       && context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol is IMethodSymbol methodSymbol
                       && (ParameterIsRawString(methodSymbol, 0) || ParameterIsRawString(methodSymbol, 1));

                static bool ParameterIsRawString(IMethodSymbol method, int index) =>
                    method.Parameters.Length > index && method.Parameters[index].IsType(KnownType.Microsoft_EntityFrameworkCore_RawSqlString);
            };

        private InvocationCondition ArgumentAtIndexIsTracked(int index) =>
            context => IsTracked(GetArgumentAtIndex(context, index), context.SemanticModel);
    }
}
