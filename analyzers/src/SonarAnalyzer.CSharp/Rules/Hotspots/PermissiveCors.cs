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

using SonarAnalyzer.Helpers.Trackers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PermissiveCors : TrackerHotspotDiagnosticAnalyzer<SyntaxKind>
    {
        private const string DiagnosticId = "S5122";
        private const string MessageFormat = "Make sure this permissive CORS policy is safe here.";
        private const string AccessControlAllowOriginHeader = "Access-Control-Allow-Origin";
        private const string AccessControlAllowOriginPropertyName = "AccessControlAllowOrigin";
        private const string StarConstant = "*";

        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        public PermissiveCors() : base(AnalyzerConfiguration.Hotspot, DiagnosticId, MessageFormat) { }

        public PermissiveCors(IAnalyzerConfiguration configuration) : base(configuration, DiagnosticId, MessageFormat) { }

        protected override void Initialize(SonarAnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterCompilationStartAction(compilationContext =>
            {
                if (IsEnabled(compilationContext.Options))
                {
                    context.RegisterNodeAction(VisitAttribute, SyntaxKind.Attribute);
                }
            });
        }

        protected override void Initialize(TrackerInput input)
        {
            SetupInvocationTracker(Language.Tracker.Invocation, input);
            SetupObjectCreationTracker(Language.Tracker.ObjectCreation, input);
        }

        private static void SetupInvocationTracker(InvocationTracker<SyntaxKind> tracker, TrackerInput input)
        {
            const int parameterCount = 2;

            tracker.Track(
                input,
                tracker.MatchMethod(new MemberDescriptor(KnownType.System_Collections_Generic_IDictionary_TKey_TValue, "Add")),
                tracker.MethodHasParameters(parameterCount),
                c => IsFirstArgumentAccessControlAllowOrigin((InvocationExpressionSyntax)c.Node, c.SemanticModel)
                    && IsSecondArgumentStarString((InvocationExpressionSyntax)c.Node, c.SemanticModel),
                tracker.IsIHeadersDictionary());

            tracker.Track(
                input,
                tracker.MatchMethod(new MemberDescriptor(KnownType.Microsoft_AspNetCore_Http_HeaderDictionaryExtensions, "Append"),
                                    new MemberDescriptor(KnownType.System_Web_HttpResponse, "AppendHeader"),
                                    new MemberDescriptor(KnownType.System_Web_HttpResponseBase, "AddHeader"),
                                    new MemberDescriptor(KnownType.System_Collections_Specialized_NameValueCollection, "Add"),
                                    new MemberDescriptor(KnownType.System_Net_Http_Headers_HttpHeaders, "Add")),
                tracker.MethodHasParameters(parameterCount),
                c => IsFirstArgumentAccessControlAllowOrigin((InvocationExpressionSyntax)c.Node, c.SemanticModel)
                    && IsSecondArgumentStarString((InvocationExpressionSyntax)c.Node, c.SemanticModel));

            tracker.Track(
                input,
                tracker.MatchMethod(new MemberDescriptor(KnownType.Microsoft_AspNetCore_Cors_Infrastructure_CorsPolicyBuilder, "WithOrigins")),
                c => ContainsStar(((InvocationExpressionSyntax)c.Node).ArgumentList.Arguments.Select(a => a.Expression), c.SemanticModel));

            tracker.Track(
                input,
                tracker.MatchMethod(new MemberDescriptor(KnownType.Microsoft_AspNetCore_Cors_Infrastructure_CorsPolicyBuilder, "AllowAnyOrigin")));
        }

        private static void SetupObjectCreationTracker(ObjectCreationTracker<SyntaxKind> tracker, TrackerInput input) =>
            tracker.Track(
                input,
                tracker.MatchConstructor(KnownType.Microsoft_AspNetCore_Cors_Infrastructure_CorsPolicyBuilder),
                c => ContainsStar(ObjectCreationFactory.Create(c.Node), c.SemanticModel));

        private void VisitAttribute(SonarSyntaxNodeReportingContext context)
        {
            var attribute = (AttributeSyntax)context.Node;
            if (attribute.IsKnownType(KnownType.System_Web_Http_Cors_EnableCorsAttribute, context.SemanticModel)
                && IsStar(attribute.ArgumentList.Arguments[0].Expression, context.SemanticModel))
            {
                context.ReportIssue(CreateDiagnostic(Rule, attribute.GetLocation()));
            }
        }

        private static bool IsFirstArgumentAccessControlAllowOrigin(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
            invocation.ArgumentList.Arguments.First().Expression switch
            {
                InterpolatedStringExpressionSyntax interpolation => interpolation.FindStringConstant(semanticModel) == AccessControlAllowOriginHeader,
                LiteralExpressionSyntax literal => literal.Token.ValueText == AccessControlAllowOriginHeader,
                MemberAccessExpressionSyntax memberAccess => IsAccessControlAllowOriginProperty(memberAccess, semanticModel),
                _ => false
            };

        private static bool IsAccessControlAllowOriginProperty(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel) =>
            memberAccess.Name.Identifier.Text == AccessControlAllowOriginPropertyName
            && memberAccess.Expression.IsKnownType(KnownType.Microsoft_Net_Http_Headers_HeaderNames, semanticModel);

        private static bool IsSecondArgumentStarString(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
            IsStar(invocation.ArgumentList.Arguments[1].Expression, semanticModel);

        private static bool IsStar(ExpressionSyntax expressionSyntax, SemanticModel semanticModel) =>
            expressionSyntax switch
            {
                InterpolatedStringExpressionSyntax interpolation => interpolation.FindStringConstant(semanticModel) == StarConstant,
                LiteralExpressionSyntax literal => ContainsStar(semanticModel.GetConstantValue(literal)),
                IdentifierNameSyntax identifier => ContainsStar(semanticModel.GetConstantValue(identifier)),
                ImplicitArrayCreationExpressionSyntax arrayCreation => ContainsStar(arrayCreation.Initializer.Expressions, semanticModel),
                { } objectCreation when objectCreation.IsAnyKind(SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression) =>
                    ContainsStar(ObjectCreationFactory.Create(objectCreation), semanticModel),
                _ => false
            };

        private static bool ContainsStar(IEnumerable<ExpressionSyntax> expressions, SemanticModel semanticModel) =>
            expressions.Any(expression => ContainsStar(semanticModel.GetConstantValue(expression)));

        private static bool ContainsStar(Optional<object> constantValue) =>
            constantValue is {HasValue: true, Value: StarConstant};

        private static bool ContainsStar(IObjectCreation objectCreation, SemanticModel semanticModel) =>
            objectCreation.ArgumentList is { } argumentList
            && (objectCreation.IsKnownType(KnownType.Microsoft_Extensions_Primitives_StringValues, semanticModel)
                || objectCreation.IsKnownType(KnownType.Microsoft_AspNetCore_Cors_Infrastructure_CorsPolicyBuilder, semanticModel))
            && argumentList.Arguments.Any(argument => IsStar(argument.Expression, semanticModel));
    }
}
