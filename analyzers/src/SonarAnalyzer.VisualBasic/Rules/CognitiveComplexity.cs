/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using SonarAnalyzer.VisualBasic.Metrics;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class CognitiveComplexity : CognitiveComplexityBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        protected override void Initialize(SonarParametrizedAnalysisContext context)
        {
            context.RegisterNodeAction(
                 c => CheckComplexity<MethodBlockSyntax>(c, m => m, m => m.SubOrFunctionStatement.Identifier.GetLocation(),
                     VisualBasicCognitiveComplexityMetric.GetComplexity, "method", Threshold),
                 SyntaxKind.SubBlock,
                 SyntaxKind.FunctionBlock);

            context.RegisterNodeAction(
                c => CheckComplexity<ConstructorBlockSyntax>(c, co => co, co => co.BlockStatement.DeclarationKeyword.GetLocation(),
                    VisualBasicCognitiveComplexityMetric.GetComplexity, "constructor", Threshold),
                SyntaxKind.ConstructorBlock);

            context.RegisterNodeAction(
                c => CheckComplexity<OperatorBlockSyntax>(c, o => o, o => o.BlockStatement.DeclarationKeyword.GetLocation(),
                    VisualBasicCognitiveComplexityMetric.GetComplexity, "operator", Threshold),
                SyntaxKind.OperatorBlock);

            context.RegisterNodeAction(
                c => CheckComplexity<AccessorBlockSyntax>(c, a => a, a => a.AccessorStatement.DeclarationKeyword.GetLocation(),
                    VisualBasicCognitiveComplexityMetric.GetComplexity, "accessor", PropertyThreshold),
                SyntaxKind.GetAccessorBlock,
                SyntaxKind.SetAccessorBlock);

            context.RegisterNodeAction(
               c => CheckComplexity<FieldDeclarationSyntax>(c, f => f, f => f.Declarators[0].Names[0].Identifier.GetLocation(),
                    VisualBasicCognitiveComplexityMetric.GetComplexity, "field", Threshold),
               SyntaxKind.FieldDeclaration);
        }
    }
}
