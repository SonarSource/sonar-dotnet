/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.Common
{
    public abstract class FlagsEnumZeroMemberBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S2346";
        protected const string MessageFormat = "Rename '{0}' to 'None'.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
    }

    public abstract class FlagsEnumZeroMemberBase<TLanguageKindEnum, TEnumDeclarationSyntax, TEnumMemberSyntax> : FlagsEnumZeroMemberBase
        where TLanguageKindEnum : struct
        where TEnumDeclarationSyntax : SyntaxNode
        where TEnumMemberSyntax : SyntaxNode
    {
        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    var enumDeclaration = (TEnumDeclarationSyntax)c.Node;

                    if (!enumDeclaration.HasFlagsAttribute(c.SemanticModel))
                    {
                        return;
                    }
                    var zeroMember = GetZeroMember(enumDeclaration, c.SemanticModel);
                    if (zeroMember == null)
                    {
                        return;
                    }

                    var identifier = GetIdentifier(zeroMember);
                    if (identifier.ValueText != "None")
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], zeroMember.GetLocation(),
                            identifier.ValueText));
                    }
                },
                SyntaxKindsOfInterest.ToArray());
        }

        protected abstract SyntaxToken GetIdentifier(TEnumMemberSyntax zeroMember);

        private TEnumMemberSyntax GetZeroMember(TEnumDeclarationSyntax node,
            SemanticModel semanticModel)
        {
            var members = GetMembers(node);

            foreach (var item in members)
            {
                if (!(semanticModel.GetDeclaredSymbol(item) is IFieldSymbol symbol))
                {
                    return null;
                }
                var constValue = symbol.ConstantValue;

                if (constValue == null)
                {
                    continue;
                }

                try
                {
                    var value = Convert.ToInt32(constValue);
                    if (value == 0)
                    {
                        return item;
                    }
                }
                catch (OverflowException)
                {
                    return null;
                }
            }
            return null;
        }

        protected abstract IEnumerable<TEnumMemberSyntax> GetMembers(TEnumDeclarationSyntax node);

        public abstract ImmutableArray<TLanguageKindEnum> SyntaxKindsOfInterest { get; }
    }
}
