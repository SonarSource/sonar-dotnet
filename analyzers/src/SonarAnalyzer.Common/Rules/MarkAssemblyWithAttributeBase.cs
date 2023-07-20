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
                            cc.ReportIssue(Language.GeneratedCodeRecognizer, CreateDiagnostic(rule, null, cc.Compilation.AssemblyName));
                        }
                    })
                );
    }
}
