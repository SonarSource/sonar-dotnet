/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotCallGCSuppressFinalize : DoNotCallMethodsCSharpBase
    {
        private const string DiagnosticId = "S3971";

        protected override string MessageFormat => "Do not call 'GC.SuppressFinalize'.";

        protected override IEnumerable<MemberDescriptor> CheckedMethods { get; } = new List<MemberDescriptor>
        {
            new(KnownType.System_GC, "SuppressFinalize")
        };

        public DoNotCallGCSuppressFinalize() : base(DiagnosticId) { }

        protected override bool ShouldReportOnMethodCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, MemberDescriptor memberDescriptor)
        {
            if (invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>() is not { } methodDeclaration
                || (methodDeclaration.Identifier.ValueText != "Dispose"
                    && methodDeclaration.Identifier.ValueText != "DisposeAsync"))
            {
                // We want to report on all calls not made from a method
                return true;
            }

            return semanticModel.GetDeclaredSymbol(methodDeclaration) is var methodSymbol
                   && !methodSymbol.IsIDisposableDispose()
                   && !methodSymbol.IsIAsyncDisposableDisposeAsync();
        }
    }
}
