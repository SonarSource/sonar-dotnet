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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.Core.Rules
{
    public abstract class DangerousGetHandleShouldNotBeCalledBase<TSyntaxKind, TInvocation> : DoNotCallMethodsBase<TSyntaxKind, TInvocation>
        where TSyntaxKind : struct
        where TInvocation : SyntaxNode
    {
        private const string DiagnosticId = "S3869";

        protected override string MessageFormat => "Refactor the code to remove this use of '{0}'.";

        protected override IEnumerable<MemberDescriptor> CheckedMethods { get; } = new List<MemberDescriptor>
        {
            new(KnownType.System_Runtime_InteropServices_SafeHandle, "DangerousGetHandle")
        };

        protected DangerousGetHandleShouldNotBeCalledBase() : base(DiagnosticId) { }
    }
}
