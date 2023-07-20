/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules
{
    public abstract class ThrowReservedExceptionsBase : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S112";
        internal const string MessageFormat = "'{0}' should not be thrown by user code.";

        internal static readonly ImmutableArray<KnownType> ReservedExceptionTypeNames =
            ImmutableArray.Create(
                KnownType.System_Exception,
                KnownType.System_ApplicationException,
                KnownType.System_SystemException,
                KnownType.System_ExecutionEngineException,
                KnownType.System_IndexOutOfRangeException,
                KnownType.System_NullReferenceException,
                KnownType.System_OutOfMemoryException
            );

        protected void ReportReservedExceptionCreation(SonarSyntaxNodeReportingContext context,
            SyntaxNode throwStatementExpression)
        {
            if (throwStatementExpression == null ||
                !IsObjectCreation(throwStatementExpression))
            {
                return;
            }

            var expressionType = context.SemanticModel.GetTypeInfo(throwStatementExpression).Type;
            if (expressionType.IsAny(ReservedExceptionTypeNames))
            {
                context.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], throwStatementExpression.GetLocation(), expressionType.ToDisplayString()));
            }
        }

        protected abstract bool IsObjectCreation(SyntaxNode throwStatementExpression);
    }
}
