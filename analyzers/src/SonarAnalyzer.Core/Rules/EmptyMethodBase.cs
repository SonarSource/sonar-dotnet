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

namespace SonarAnalyzer.Core.Rules
{
    public abstract class EmptyMethodBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        internal const string DiagnosticId = "S1186";

        protected abstract HashSet<TSyntaxKind> SyntaxKinds { get; }
        protected abstract void CheckMethod(SonarSyntaxNodeReportingContext context);

        protected override string MessageFormat => "Add a nested comment explaining why this method is empty, throw a 'NotSupportedException' or complete the implementation.";

        protected EmptyMethodBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckMethod, SyntaxKinds.ToArray());
    }
}
