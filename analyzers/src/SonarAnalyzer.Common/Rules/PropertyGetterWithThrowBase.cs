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
    public abstract class PropertyGetterWithThrowBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S2372";

        protected const string MessageFormat = "Remove the exception throwing from this property getter, or refactor the " +
            "property into a method.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

        internal static readonly ImmutableArray<KnownType> AllowedExceptionBaseTypes =
            ImmutableArray.Create(
                KnownType.System_NotImplementedException,
                KnownType.System_NotSupportedException,
                KnownType.System_InvalidOperationException
            );
    }

    public abstract class PropertyGetterWithThrowBase<TLanguageKindEnum, TAccessorSyntax> :
        PropertyGetterWithThrowBase
        where TLanguageKindEnum : struct
        where TAccessorSyntax : SyntaxNode
    {
        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCodeBlockStartAction<TLanguageKindEnum>(
                GeneratedCodeRecognizer,
                cbc =>
                {
                    if (!(cbc.CodeBlock is TAccessorSyntax propertyGetter) ||
                        !IsGetter(propertyGetter) ||
                        IsIndexer(propertyGetter))
                    {
                        return;
                    }

                    cbc.RegisterNodeAction(
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

                            c.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], c.Node.GetLocation()));
                        },
                        ThrowSyntaxKind);
                });
        }

        protected abstract bool IsIndexer(TAccessorSyntax propertyGetter);

        protected abstract bool IsGetter(TAccessorSyntax propertyGetter);

        protected abstract TLanguageKindEnum ThrowSyntaxKind { get; }

        protected abstract SyntaxNode GetThrowExpression(SyntaxNode syntaxNode);
    }
}
