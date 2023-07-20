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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class ArrayCreationLongSyntax : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2355";
        private const string MessageFormat = "Use an array literal here instead.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    var arrayCreation = (ArrayCreationExpressionSyntax)c.Node;
                    if (arrayCreation.Initializer == null
                        || HasSizeSpecifier(arrayCreation)
                        || c.SemanticModel.GetTypeInfo(arrayCreation).Type is not IArrayTypeSymbol arrayType
                        || arrayType.ElementType is null or IErrorTypeSymbol)
                    {
                        return;
                    }
                    if (arrayCreation.Initializer.Initializers.Any())
                    {
                        if (AtLeastOneExactTypeMatch(c.SemanticModel, arrayCreation, arrayType)
                            && AllTypesAreConvertible(c.SemanticModel, arrayCreation, arrayType))
                        {
                            c.ReportIssue(CreateDiagnostic(Rule, arrayCreation.GetLocation()));
                        }
                    }
                    else
                    {
                        if (arrayType.ElementType.Is(KnownType.System_Object))
                        {
                            c.ReportIssue(CreateDiagnostic(Rule, arrayCreation.GetLocation()));
                        }
                    }
                },
                SyntaxKind.ArrayCreationExpression);

        private static bool HasSizeSpecifier(ArrayCreationExpressionSyntax arrayCreation) =>
            arrayCreation.ArrayBounds != null && arrayCreation.ArrayBounds.Arguments.Any();

        private static bool AllTypesAreConvertible(SemanticModel semanticModel, ArrayCreationExpressionSyntax arrayCreation, IArrayTypeSymbol arrayType) =>
            arrayCreation.Initializer.Initializers.All(x => semanticModel.ClassifyConversion(x, arrayType.ElementType) is var conversion
                                                            && conversion.Exists
                                                            && (conversion.IsIdentity || conversion.IsWidening));

        private static bool AtLeastOneExactTypeMatch(SemanticModel semanticModel, ArrayCreationExpressionSyntax arrayCreation, IArrayTypeSymbol arrayType) =>
            arrayCreation.Initializer.Initializers.Any(x => arrayType.ElementType.Equals(semanticModel.GetTypeInfo(x).Type));
    }
}
