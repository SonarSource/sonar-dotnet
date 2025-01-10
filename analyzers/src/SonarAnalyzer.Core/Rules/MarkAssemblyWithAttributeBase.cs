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
    public abstract class MarkAssemblyWithAttributeBase : SonarDiagnosticAnalyzer
    {
        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade Language { get; }
        private protected abstract KnownType AttributeToFind { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override bool EnableConcurrentExecution => false;

        protected MarkAssemblyWithAttributeBase(string diagnosticId, string messageFormat) =>
            rule = Language.CreateDescriptor(diagnosticId, messageFormat);

        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCompilationStartAction(c =>
                c.RegisterCompilationEndAction(cc =>
                    {
                        if (!cc.Compilation.Assembly.HasAttribute(AttributeToFind) && !cc.Compilation.Assembly.HasAttribute(KnownType.Microsoft_AspNetCore_Razor_Hosting_RazorCompiledItemAttribute))
                        {
                            cc.ReportIssue(Language.GeneratedCodeRecognizer, rule, (Location)null, cc.Compilation.AssemblyName);
                        }
                    })
                );
    }
}
