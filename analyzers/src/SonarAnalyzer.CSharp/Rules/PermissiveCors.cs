/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PermissiveCors : SonarDiagnosticAnalyzer<SyntaxKind>
{
    private const string DiagnosticId = "S5122";

    private const string AccessControlAllowOriginHeader = "Access-Control-Allow-Origin";
    private const string AccessControlAllowOriginPropertyName = "AccessControlAllowOrigin";
    private const string StarConstant = "*";

    protected override string MessageFormat => "Restrict this CORS policy to trusted origins.";

    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    public PermissiveCors() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        var input = new TrackerInput(context, AnalyzerConfiguration.AlwaysEnabled, Rule);
        SetupInvocationTracker(Language.Tracker.Invocation, input);
        SetupObjectCreationTracker(Language.Tracker.ObjectCreation, input);

        context.RegisterCompilationStartAction(c =>
            c.RegisterNodeAction(VisitAttribute, SyntaxKind.Attribute));
    }

    private static void SetupInvocationTracker(InvocationTracker<SyntaxKind> tracker, TrackerInput input)
    {
        const int parameterCount = 2;

        tracker.Track(
            input,
            tracker.MatchMethod(new MemberDescriptor(KnownType.System_Collections_Generic_IDictionary_TKey_TValue, "Add")),
            tracker.MethodHasParameters(parameterCount),
            x => IsFirstArgumentAccessControlAllowOrigin((InvocationExpressionSyntax)x.Node, x.Model)
                    && IsSecondArgumentStarString((InvocationExpressionSyntax)x.Node, x.Model),
            tracker.IsIHeadersDictionary());

        tracker.Track(
            input,
            tracker.MatchMethod(
                new MemberDescriptor(KnownType.Microsoft_AspNetCore_Http_HeaderDictionaryExtensions, "Append"),
                new MemberDescriptor(KnownType.System_Web_HttpResponse, "AppendHeader"),
                new MemberDescriptor(KnownType.System_Web_HttpResponseBase, "AddHeader"),
                new MemberDescriptor(KnownType.System_Collections_Specialized_NameValueCollection, "Add"),
                new MemberDescriptor(KnownType.System_Net_Http_Headers_HttpHeaders, "Add")),
            tracker.MethodHasParameters(parameterCount),
            x => IsFirstArgumentAccessControlAllowOrigin((InvocationExpressionSyntax)x.Node, x.Model)
                    && IsSecondArgumentStarString((InvocationExpressionSyntax)x.Node, x.Model));

        tracker.Track(
            input,
            tracker.MatchMethod(new MemberDescriptor(KnownType.Microsoft_AspNetCore_Cors_Infrastructure_CorsPolicyBuilder, "WithOrigins")),
            x => ContainsStar(((InvocationExpressionSyntax)x.Node).ArgumentList.Arguments.Select(a => a.Expression), x.Model));

        tracker.Track(
            input,
            tracker.MatchMethod(new MemberDescriptor(KnownType.Microsoft_AspNetCore_Cors_Infrastructure_CorsPolicyBuilder, "AllowAnyOrigin")));
    }

    private static void SetupObjectCreationTracker(ObjectCreationTracker<SyntaxKind> tracker, TrackerInput input) =>
        tracker.Track(
            input,
            tracker.MatchConstructor(KnownType.Microsoft_AspNetCore_Cors_Infrastructure_CorsPolicyBuilder),
            x => ContainsStar(ObjectCreationFactory.Create(x.Node), x.Model));

    private void VisitAttribute(SonarSyntaxNodeReportingContext context)
    {
        var attribute = (AttributeSyntax)context.Node;
        if (attribute.IsKnownType(KnownType.System_Web_Http_Cors_EnableCorsAttribute, context.Model)
            && IsStar(attribute.ArgumentList.Arguments[0].Expression, context.Model))
        {
            context.ReportIssue(Rule, attribute);
        }
    }

    private static bool IsFirstArgumentAccessControlAllowOrigin(InvocationExpressionSyntax invocation, SemanticModel model) =>
        invocation.ArgumentList.Arguments.First().Expression switch
        {
            InterpolatedStringExpressionSyntax interpolation => interpolation.FindStringConstant(model) == AccessControlAllowOriginHeader,
            LiteralExpressionSyntax literal => literal.Token.ValueText == AccessControlAllowOriginHeader,
            MemberAccessExpressionSyntax memberAccess => IsAccessControlAllowOriginProperty(memberAccess, model),
            _ => false
        };

    private static bool IsAccessControlAllowOriginProperty(MemberAccessExpressionSyntax memberAccess, SemanticModel model) =>
        memberAccess.Name.Identifier.Text == AccessControlAllowOriginPropertyName
        && memberAccess.Expression.IsKnownType(KnownType.Microsoft_Net_Http_Headers_HeaderNames, model);

    private static bool IsSecondArgumentStarString(InvocationExpressionSyntax invocation, SemanticModel model) =>
        IsStar(invocation.ArgumentList.Arguments[1].Expression, model);

    private static bool IsStar(ExpressionSyntax expressionSyntax, SemanticModel model) =>
        expressionSyntax switch
        {
            InterpolatedStringExpressionSyntax interpolation => interpolation.FindStringConstant(model) == StarConstant,
            LiteralExpressionSyntax literal => ContainsStar(model.GetConstantValue(literal)),
            IdentifierNameSyntax identifier => ContainsStar(model.GetConstantValue(identifier)),
            ImplicitArrayCreationExpressionSyntax arrayCreation => ContainsStar(arrayCreation.Initializer.Expressions, model),
            { } objectCreation when objectCreation.Kind() is SyntaxKind.ObjectCreationExpression or SyntaxKindEx.ImplicitObjectCreationExpression =>
                ContainsStar(ObjectCreationFactory.Create(objectCreation), model),
            _ => false
        };

    private static bool ContainsStar(IEnumerable<ExpressionSyntax> expressions, SemanticModel model) =>
        expressions.Any(x => ContainsStar(model.GetConstantValue(x)));

    private static bool ContainsStar(Optional<object> constantValue) =>
        constantValue is { HasValue: true, Value: StarConstant };

    private static bool ContainsStar(IObjectCreation objectCreation, SemanticModel model) =>
        objectCreation.ArgumentList is { } argumentList
        && (objectCreation.IsKnownType(KnownType.Microsoft_Extensions_Primitives_StringValues, model)
            || objectCreation.IsKnownType(KnownType.Microsoft_AspNetCore_Cors_Infrastructure_CorsPolicyBuilder, model))
        && argumentList.Arguments.Any(x => IsStar(x.Expression, model));
}
