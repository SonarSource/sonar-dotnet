﻿/*
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

namespace SonarAnalyzer.Rules
{
    public abstract class EmptyMethodBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        internal const string DiagnosticId = "S1186";

        protected abstract TSyntaxKind[] SyntaxKinds { get; }
        protected abstract void CheckMethod(SonarSyntaxNodeReportingContext context);

        protected override string MessageFormat => "Add a nested comment explaining why this method is empty, throw a 'NotSupportedException' or complete the implementation.";

        protected EmptyMethodBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckMethod, SyntaxKinds.ToArray());
    }
}
