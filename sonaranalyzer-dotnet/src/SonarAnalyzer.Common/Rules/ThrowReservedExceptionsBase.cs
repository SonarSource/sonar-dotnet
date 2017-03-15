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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ThrowReservedExceptionsBase : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S112";
        internal const string MessageFormat = "'{0}' should not be thrown by user code.";

        protected static readonly ISet<string> ReservedExceptionTypeNames = ImmutableHashSet.Create(
                KnownType.System_Exception.TypeName,
                KnownType.System_ApplicationException.TypeName,
                KnownType.System_SystemException.TypeName,
                KnownType.System_ExecutionEngineException.TypeName,
                KnownType.System_IndexOutOfRangeException.TypeName,
                KnownType.System_NullReferenceException.TypeName,
                KnownType.System_OutOfMemoryException.TypeName);

        protected void ReportReservedExceptionCreation(SyntaxNodeAnalysisContext context)
        {
            var typeInfo = context.SemanticModel.GetTypeInfo(context.Node);

            if (typeInfo.Type == null || typeInfo.Type is IErrorTypeSymbol)
            {
                return;
            }

            var exceptionTypeFullName = typeInfo.Type.ToDisplayString();

            if (ReservedExceptionTypeNames.Contains(exceptionTypeFullName))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, GetLocation(context.Node), exceptionTypeFullName));
            }
        }

        protected abstract Location GetLocation(SyntaxNode node);
    }
}
