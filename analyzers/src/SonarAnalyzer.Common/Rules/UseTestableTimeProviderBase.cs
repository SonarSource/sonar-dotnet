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

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class UseTestableTimeProviderBase<TSyntaxKind> : SonarDiagnosticAnalyzer
         where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "ToBedesided";
        protected const string MessageFormat = "Use a testable (date) time provider instead.";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);
        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        protected UseTestableTimeProviderBase() =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, Language.RspecResources);

        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer, c =>
            {
                if (c.SemanticModel.GetSymbolInfo(c.Node).Symbol is IMethodSymbol identifier
                    && IsDateTimeProvider(identifier))
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, c.Node.GetLocation()));
                }
            },
            Language.SyntaxKind.IdentifierName);

        private bool IsDateTimeProvider(IMethodSymbol identifier) =>
            identifier.IsInType(KnownType.System_DateTime)
            && IsDateTimeProviderProperty(identifier.Name);

        private bool IsDateTimeProviderProperty(string name)
            => nameof(DateTime.Now).Equals(name, Language.NameComparison)
            || nameof(DateTime.UtcNow).Equals(name, Language.NameComparison)
            || nameof(DateTime.Today).Equals(name, Language.NameComparison);
    }
}
