/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ImplementSerializationMethodsCorrectly : ImplementSerializationMethodsCorrectlyBase
    {
        private const string ProblemStatic = "non-static";
        private const string ProblemReturnVoidText = "return 'void'";

        protected override ILanguageFacade Language => CSharpFacade.Instance;
        protected override string MethodStaticMessage => ProblemStatic;
        protected override string MethodReturnTypeShouldBeVoidMessage => ProblemReturnVoidText;

        protected override Location GetIdentifierLocation(IMethodSymbol methodSymbol) =>
            methodSymbol.DeclaringSyntaxReferences.Select(x => x.GetSyntax())
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault()
                ?.Identifier
                .GetLocation();

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(c =>
                {
                    var wrapper = (LocalFunctionStatementSyntaxWrapper)c.Node;
                    var attributes = GetSerializationAttributes(wrapper.AttributeLists, c.Model);
                    ReportOnAttributes(c, attributes, "local functions");
                },
                SyntaxKindEx.LocalFunctionStatement);

            context.RegisterNodeAction(c =>
                {
                    var lambda = (ParenthesizedLambdaExpressionSyntax)c.Node;
                    var attributes = GetSerializationAttributes(lambda.AttributeLists, c.Model);
                    ReportOnAttributes(c, attributes, "lambdas");
                },
                SyntaxKind.ParenthesizedLambdaExpression);

            base.Initialize(context);
        }

        private void ReportOnAttributes(SonarSyntaxNodeReportingContext context, IEnumerable<AttributeSyntax> attributes, string memberType)
        {
            foreach (var attribute in attributes)
            {
                context.ReportIssue(AttributeNotConsideredRule, attribute, memberType);
            }
        }

        private static IEnumerable<AttributeSyntax> GetSerializationAttributes(SyntaxList<AttributeListSyntax> attributeList, SemanticModel model) =>
            attributeList.SelectMany(x => x.Attributes)
                         .Where(attribute => attribute.IsKnownType(KnownType.System_Runtime_Serialization_OnSerializingAttribute, model)
                                             || attribute.IsKnownType(KnownType.System_Runtime_Serialization_OnSerializedAttribute, model)
                                             || attribute.IsKnownType(KnownType.System_Runtime_Serialization_OnDeserializingAttribute, model)
                                             || attribute.IsKnownType(KnownType.System_Runtime_Serialization_OnDeserializedAttribute, model));
    }
}
