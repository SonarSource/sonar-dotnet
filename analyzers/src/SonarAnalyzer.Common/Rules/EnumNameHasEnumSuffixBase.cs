﻿/*
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class EnumNameHasEnumSuffixBase<TSyntaxKind> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S2344";
        private const string MessageFormat = "Rename this enumeration to remove the '{0}' suffix.";

        private readonly IEnumerable<string> nameEndings = ImmutableArray.Create("enum", "flags");

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);
        private readonly DiagnosticDescriptor rule;

        protected EnumNameHasEnumSuffixBase() =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, Language.RspecResources);

        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer, c =>
                {
                    if (Language.Syntax.NodeIdentifier(c.Node) is { } identifier
                        && nameEndings.FirstOrDefault(ending => identifier.ValueText.EndsWith(ending, System.StringComparison.OrdinalIgnoreCase)) is { } nameEnding)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, identifier.GetLocation(), identifier.ValueText.Substring(identifier.ValueText.Length - nameEnding.Length)));
                    }
                },
                Language.SyntaxKind.EnumDeclaration);
    }
}
