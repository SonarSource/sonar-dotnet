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

namespace SonarAnalyzer.Rules.CSharp
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
                    var attributes = GetSerializationAttributes(wrapper.AttributeLists, c.SemanticModel);
                    ReportOnAttributes(c, attributes, "local functions");
                },
                SyntaxKindEx.LocalFunctionStatement);

            context.RegisterNodeAction(c =>
                {
                    var wrapper = (ParenthesizedLambdaExpressionSyntaxWrapper)c.Node;
                    var attributes = GetSerializationAttributes(wrapper.AttributeLists, c.SemanticModel);
                    ReportOnAttributes(c, attributes, "lambdas");
                },
                SyntaxKind.ParenthesizedLambdaExpression);

            base.Initialize(context);
        }

        private void ReportOnAttributes(SonarSyntaxNodeReportingContext context, IEnumerable<AttributeSyntax> attributes, string memberType)
        {
            foreach (var attribute in attributes)
            {
                context.ReportIssue(CreateDiagnostic(AttributeNotConsideredRule, attribute.GetLocation(), memberType));
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
