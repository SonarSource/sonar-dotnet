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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class DisablingRequestValidationBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S5753";
        private const string MessageFormat = "Make sure disabling ASP.NET Request Validation feature is safe here.";

        private readonly DiagnosticDescriptor rule;
        private readonly IAnalyzerConfiguration configuration;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected DisablingRequestValidationBase(System.Resources.ResourceManager rspecResources, IAnalyzerConfiguration configuration)
        {
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources).WithNotConfigurable();
            this.configuration = configuration;
        }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSymbolAction(c =>
                {
                    configuration.Initialize(c.Options);
                    if (!configuration.IsEnabled(DiagnosticId))
                    {
                        return;
                    }

                    var attributes = c.Symbol.GetAttributes();
                    if (attributes.IsEmpty)
                    {
                        return;
                    }
                    var attributeWithFalseParameter = attributes.FirstOrDefault(a =>
                        a.ConstructorArguments.Length == 1
                        && a.ConstructorArguments[0].Kind == TypedConstantKind.Primitive
                        && a.ConstructorArguments[0].Value is bool b
                        && b == false
                        && a.AttributeClass.Is(KnownType.System_Web_Mvc_ValidateInputAttribute));
                    if (attributeWithFalseParameter != null)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, attributeWithFalseParameter.ApplicationSyntaxReference.GetSyntax().GetLocation()));
                    }
                },
                SymbolKind.NamedType,
                SymbolKind.Method);
    }
}
