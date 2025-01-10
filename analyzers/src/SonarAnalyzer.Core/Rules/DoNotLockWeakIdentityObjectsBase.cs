/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Rules
{
    public abstract class DoNotLockWeakIdentityObjectsBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S3998";

        private readonly ImmutableArray<KnownType> weakIdentityTypes =
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

        protected abstract TSyntaxKind SyntaxKind { get; }

        protected override string MessageFormat => "Replace this lock on '{0}' with a lock against an object that cannot be accessed across application domain boundaries.";

        protected DoNotLockWeakIdentityObjectsBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
            {
                var lockExpression = Language.Syntax.NodeExpression(c.Node);
                if (c.SemanticModel.GetSymbolInfo(lockExpression).Symbol?.GetSymbolType() is { } lockExpressionType && lockExpressionType.DerivesFromAny(weakIdentityTypes))
                {
                    c.ReportIssue(Rule, lockExpression, lockExpressionType.Name);
                }
            }, SyntaxKind);
    }
}
