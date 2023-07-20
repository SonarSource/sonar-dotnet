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
    public sealed class UseUriInsteadOfString : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticIdRuleS3994 = "S3994";
        private const string DiagnosticIdRuleS3995 = "S3995";
        private const string DiagnosticIdRuleS3996 = "S3996";
        private const string DiagnosticIdRuleS3997 = "S3997";
        private const string DiagnosticIdRuleS4005 = "S4005";
        private const string MessageFormatRuleS3994 = "Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.";
        private const string MessageFormatRuleS3995 = "Change this return type to 'System.Uri'.";
        private const string MessageFormatRuleS3996 = "Change this property type to 'System.Uri'.";
        private const string MessageFormatRuleS3997 = "Refactor this method so it invokes the overload accepting a 'System.Uri' parameter.";
        private const string MessageFormatRuleS4005 = "Call the overload that takes a 'System.Uri' as an argument instead.";

        private static readonly DiagnosticDescriptor RuleS3994 = DescriptorFactory.Create(DiagnosticIdRuleS3994, MessageFormatRuleS3994);
        private static readonly DiagnosticDescriptor RuleS3995 = DescriptorFactory.Create(DiagnosticIdRuleS3995, MessageFormatRuleS3995);
        private static readonly DiagnosticDescriptor RuleS3996 = DescriptorFactory.Create(DiagnosticIdRuleS3996, MessageFormatRuleS3996);
        private static readonly DiagnosticDescriptor RuleS3997 = DescriptorFactory.Create(DiagnosticIdRuleS3997, MessageFormatRuleS3997);
        private static readonly DiagnosticDescriptor RuleS4005 = DescriptorFactory.Create(DiagnosticIdRuleS4005, MessageFormatRuleS4005);
        private static readonly ISet<string> UrlNameVariants = new HashSet<string> { "URI", "URL", "URN" };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleS3994, RuleS3995, RuleS3996, RuleS3997, RuleS4005);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(VerifyMethodDeclaration, SyntaxKind.MethodDeclaration, SyntaxKind.ConstructorDeclaration);

            context.RegisterNodeAction(VerifyPropertyDeclaration, SyntaxKind.PropertyDeclaration);

            context.RegisterNodeAction(
                VerifyInvocationAndCreation,
                SyntaxKind.InvocationExpression,
                SyntaxKind.ObjectCreationExpression,
                SyntaxKindEx.ImplicitObjectCreationExpression);

            context.RegisterNodeAction(
                VerifyRecordDeclaration,
                SyntaxKindEx.RecordClassDeclaration,
                SyntaxKindEx.RecordStructDeclaration);
        }

        private static void VerifyMethodDeclaration(SonarSyntaxNodeReportingContext context)
        {
            var methodDeclaration = (BaseMethodDeclarationSyntax)context.Node;
            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);
            if (methodSymbol == null || methodSymbol.IsOverride)
            {
                return;
            }

            VerifyReturnType(context, methodDeclaration, methodSymbol);
            var stringUrlParams = StringUrlParamIndexes(methodSymbol);
            if (!stringUrlParams.Any())
            {
                return;
            }

            var methodOverloads = FindOverloadsThatUseUriTypeInPlaceOfString(methodSymbol, stringUrlParams).ToHashSet();
            if (methodOverloads.Any())
            {
                if (!methodDeclaration.IsKind(SyntaxKind.ConstructorDeclaration)
                    && !methodDeclaration.ContainsMethodInvocation(context.SemanticModel, x => true, x => methodOverloads.Contains(x)))
                {
                    context.ReportIssue(CreateDiagnostic(RuleS3997, methodDeclaration.FindIdentifierLocation()));
                }
            }
            else
            {
                foreach (var paramIdx in stringUrlParams)
                {
                    context.ReportIssue(CreateDiagnostic(RuleS3994, methodDeclaration.ParameterList.Parameters[paramIdx].Type.GetLocation()));
                }
            }
        }

        private static void VerifyPropertyDeclaration(SonarSyntaxNodeReportingContext context)
        {
            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
            var propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
            if (propertySymbol.Type.Is(KnownType.System_String)
                && !propertySymbol.IsOverride
                && NameContainsUri(propertySymbol.Name))
            {
                context.ReportIssue(CreateDiagnostic(RuleS3996, propertyDeclaration.Type.GetLocation()));
            }
        }

        private static void VerifyRecordDeclaration(SonarSyntaxNodeReportingContext context)
        {
            var declaration = (RecordDeclarationSyntaxWrapper)context.Node;
            if (!context.IsRedundantPositionalRecordContext() && HasStringUriParams(declaration.ParameterList, context.SemanticModel))
            {
                context.ReportIssue(CreateDiagnostic(RuleS3996, declaration.SyntaxNode.GetLocation()));
            }
        }

        private static bool HasStringUriParams(BaseParameterListSyntax parameterList, SemanticModel model) =>
            parameterList != null
            && parameterList.Parameters.Any(x => NameContainsUri(x.Identifier.Text) && model.GetDeclaredSymbol(x).IsType(KnownType.System_String));

        private static void VerifyInvocationAndCreation(SonarSyntaxNodeReportingContext context)
        {
            if (context.SemanticModel.GetSymbolInfo(context.Node).Symbol is IMethodSymbol invokedMethodSymbol
                && !invokedMethodSymbol.IsInType(KnownType.System_Uri)
                && StringUrlParamIndexes(invokedMethodSymbol) is { Count: not 0 } stringUrlParams
                && FindOverloadsThatUseUriTypeInPlaceOfString(invokedMethodSymbol, stringUrlParams).Any())
            {
                context.ReportIssue(CreateDiagnostic(RuleS4005, context.Node.GetLocation()));
            }
        }

        private static void VerifyReturnType(SonarSyntaxNodeReportingContext context, BaseMethodDeclarationSyntax methodDeclaration, IMethodSymbol methodSymbol)
        {
            if ((methodDeclaration as MethodDeclarationSyntax)?.ReturnType?.GetLocation() is { } returnTypeLocation
                && methodSymbol.ReturnType.Is(KnownType.System_String)
                && NameContainsUri(methodSymbol.Name))
            {
                context.ReportIssue(CreateDiagnostic(RuleS3995, returnTypeLocation));
            }
        }

        private static IEnumerable<IMethodSymbol> FindOverloadsThatUseUriTypeInPlaceOfString(IMethodSymbol originalMethodSymbol, ISet<int> paramIdx)
        {
            if (paramIdx.Any())
            {
                foreach (var methodSymbol in OtherMethodOverrides(originalMethodSymbol))
                {
                    if (methodSymbol.Parameters.Where((x, index) => UsesUriInPlaceOfStringUri(x, originalMethodSymbol.Parameters[index], paramIdx.Contains(index))).Any())
                    {
                        yield return methodSymbol;
                    }
                }
            }
        }

        private static ISet<int> StringUrlParamIndexes(IMethodSymbol methodSymbol)
        {
            var ret = new HashSet<int>();
            for (var i = 0; i < methodSymbol.Parameters.Length; i++)
            {
                var parameter = methodSymbol.Parameters[i];
                if (parameter.Type.Is(KnownType.System_String) && NameContainsUri(parameter.Name))
                {
                    ret.Add(i);
                }
            }
            return ret;
        }

        private static IEnumerable<IMethodSymbol> OtherMethodOverrides(IMethodSymbol methodSymbol) =>
            methodSymbol.ContainingType
                .GetMembers(methodSymbol.Name)
                .OfType<IMethodSymbol>()
                .Where(x => x.Parameters.Length == methodSymbol.Parameters.Length && !x.Equals(methodSymbol));

        private static bool UsesUriInPlaceOfStringUri(IParameterSymbol paramSymbol, IParameterSymbol originalParamSymbol, bool isStringUri) =>
            isStringUri
                ? paramSymbol.Type.Is(KnownType.System_Uri)
                : Equals(paramSymbol, originalParamSymbol);

        private static bool NameContainsUri(string name)
        {
            var wordsInName = name.SplitCamelCaseToWords();
            return UrlNameVariants.Overlaps(wordsInName);
        }
    }
}
