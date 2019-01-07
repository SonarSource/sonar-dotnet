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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class ArrayCreationLongSyntax : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2355";
        private const string MessageFormat = "Use an array literal here instead.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var arrayCreation = (ArrayCreationExpressionSyntax)c.Node;
                    if (arrayCreation.Initializer == null ||
                        HasSizeSpecifier(arrayCreation))
                    {
                        return;
                    }

                    var arrayType = c.SemanticModel.GetTypeInfo(arrayCreation).Type as IArrayTypeSymbol;
                    if (arrayType?.ElementType == null ||
                        arrayType.ElementType is IErrorTypeSymbol)
                    {
                        return;
                    }

                    if (arrayType.ElementType.Is(KnownType.System_Object) &&
                        !arrayCreation.Initializer.Initializers.Any())
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, arrayCreation.GetLocation()));
                        return;
                    }

                    if (!arrayCreation.Initializer.Initializers.Any())
                    {
                        return;
                    }

                    if (AtLeastOneExactTypeMatch(c.SemanticModel, arrayCreation, arrayType) &&
                        AllTypesAreConvertible(c.SemanticModel, arrayCreation, arrayType))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, arrayCreation.GetLocation()));
                    }
                },
                SyntaxKind.ArrayCreationExpression);
        }

        private static bool HasSizeSpecifier(ArrayCreationExpressionSyntax arrayCreation)
        {
            return arrayCreation.ArrayBounds != null &&
                arrayCreation.ArrayBounds.Arguments.Any();
        }

        private static bool AllTypesAreConvertible(SemanticModel semanticModel, ArrayCreationExpressionSyntax arrayCreation, IArrayTypeSymbol arrayType)
        {
            return arrayCreation.Initializer.Initializers.All(initializer =>
            {
                var conversion = semanticModel.ClassifyConversion(initializer, arrayType.ElementType);
                return conversion.Exists && (conversion.IsIdentity || conversion.IsWidening);
            });
        }

        private static bool AtLeastOneExactTypeMatch(SemanticModel semanticModel, ArrayCreationExpressionSyntax arrayCreation, IArrayTypeSymbol arrayType)
        {
            return arrayCreation.Initializer.Initializers.Any(initializer => arrayType.ElementType.Equals(semanticModel.GetTypeInfo(initializer).Type));
        }
    }
}
