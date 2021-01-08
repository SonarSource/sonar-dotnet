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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DisablingRequestValidation : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S5753";
        private const string MessageFormat = "Make sure disabling ASP.NET Request Validation feature is safe here.";
        private static readonly DiagnosticDescriptor Rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager).WithNotConfigurable();

        private readonly IAnalyzerConfiguration analyzerConfiguration;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public DisablingRequestValidation() : this(AnalyzerConfiguration.Hotspot)
        {
        }

        public DisablingRequestValidation(IAnalyzerConfiguration analyzerConfiguration)
        {
            this.analyzerConfiguration = analyzerConfiguration;
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            if (analyzerConfiguration.IsEnabled(DiagnosticId))
            {
                context.RegisterSymbolAction(c =>
                    {
                        var attributes = c.Symbol.GetAttributes();
                        if (attributes.IsEmpty)
                        {
                            return;
                        }
                        var attributeWithFalseParameter = attributes.FirstOrDefault(a =>
                            a.ConstructorArguments.Length == 1
                            && a.ConstructorArguments[0].Kind == TypedConstantKind.Primitive
                            && a.ConstructorArguments[0].Value is bool b
                            && b == false);
                        if (attributeWithFalseParameter != null && attributeWithFalseParameter.AttributeClass.Is(KnownType.System_Web_Mvc_ValidateInputAttribute))
                        {
                            c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, attributeWithFalseParameter.ApplicationSyntaxReference.GetSyntax().GetLocation()));
                        }
                    },
                    SymbolKind.NamedType,
                    SymbolKind.Method);
            }
        }
    }
}
