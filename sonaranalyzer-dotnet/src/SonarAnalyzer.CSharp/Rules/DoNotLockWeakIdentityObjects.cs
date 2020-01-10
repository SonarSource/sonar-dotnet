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

using System.Collections.Immutable;
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
    public sealed class DoNotLockWeakIdentityObjects : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3998";
        private const string MessageFormat = "Replace this lock on '{0}' with a lock against an object that cannot be accessed across application domain boundaries.";

        private static readonly ImmutableArray<KnownType> weakIdentityTypes =
            ImmutableArray.Create(
                KnownType.System_MarshalByRefObject,
                KnownType.System_ExecutionEngineException,
                KnownType.System_OutOfMemoryException,
                KnownType.System_StackOverflowException,
                KnownType.System_String,
                KnownType.System_IO_FileStream,
                KnownType.System_Reflection_MemberInfo,
                KnownType.System_Reflection_ParameterInfo,
                KnownType.System_Threading_Thread
            );

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var lockExpression = ((LockStatementSyntax)c.Node).Expression;
                var lockExpressionType = c.SemanticModel.GetSymbolInfo(lockExpression).Symbol?.GetSymbolType();

                if (lockExpressionType != null &&
                    lockExpressionType.DerivesFromAny(weakIdentityTypes))
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, lockExpression.GetLocation(),
                        lockExpressionType.Name));
                }
            },
            SyntaxKind.LockStatement);
        }
    }
}
