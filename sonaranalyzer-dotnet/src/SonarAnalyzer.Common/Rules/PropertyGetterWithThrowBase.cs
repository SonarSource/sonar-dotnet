/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.Common
{
    public abstract class PropertyGetterWithThrowBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S2372";

        protected const string MessageFormat = "Remove the exception throwing from this property getter, or refactor the " +
            "property into a method.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

        internal static readonly ISet<KnownType> AllowedExceptionBaseTypes = new HashSet<KnownType>
        {
            KnownType.System_NotImplementedException,
            KnownType.System_NotSupportedException,
            KnownType.System_InvalidOperationException
        };
    }

    public abstract class PropertyGetterWithThrowBase<TLanguageKindEnum, TAccessorSyntax> :
        PropertyGetterWithThrowBase
        where TLanguageKindEnum : struct
        where TAccessorSyntax : SyntaxNode
    {
        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCodeBlockStartActionInNonGenerated<TLanguageKindEnum>(
                GeneratedCodeRecognizer,
                cbc =>
                {
                    var propertyGetter = cbc.CodeBlock as TAccessorSyntax;
                    if (propertyGetter == null ||
                        !IsGetter(propertyGetter) ||
                        IsIndexer(propertyGetter))
                    {
                        return;
                    }

                    cbc.RegisterSyntaxNodeAction(
                        c =>
                        {
                            var throwExpression = GetThrowExpression(c.Node);

                            // This is the case in rethrow - see ticket #730.
                            if (throwExpression == null)
                            {
                                return;
                            }

                            var symbol = c.SemanticModel.GetSymbolInfo(throwExpression).Symbol;
                            if (symbol == null ||
                                symbol.ContainingType.DerivesFromAny(AllowedExceptionBaseTypes))
                            {
                                return;
                            }

                            c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, c.Node.GetLocation()));
                        },
                        ThrowSyntaxKind);
                });
        }

        protected abstract bool IsIndexer(TAccessorSyntax propertyGetter);

        protected abstract bool IsGetter(TAccessorSyntax propertyGetter);

        protected abstract TLanguageKindEnum ThrowSyntaxKind { get; }

        protected abstract SyntaxNode GetThrowExpression(SyntaxNode syntaxNode);

        protected abstract DiagnosticDescriptor Rule { get; }

        public override sealed ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    }
}
