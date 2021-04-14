/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.Hotspots
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class PermissiveCors : TrackerHotspotDiagnosticAnalyzer<SyntaxKind>
    {
        private const string DiagnosticId = "S5122";
        private const string MessageFormat = "Make sure this permissive CORS policy is safe here.";
        private const string AccessControlAllowOriginHeader = "Access-Control-Allow-Origin";
        private const string AccessControlAllowOriginPropertyName = "AccessControlAllowOrigin";
        private const string StarConstant = "*";
        private const int ParameterCount = 2;

        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        public PermissiveCors() : base(AnalyzerConfiguration.Hotspot, DiagnosticId, MessageFormat) { }

        public PermissiveCors(IAnalyzerConfiguration configuration) : base(configuration, DiagnosticId, MessageFormat) { }

        protected override void Initialize(TrackerInput input)
        {
            var iTracker = Language.Tracker.Invocation;
            iTracker.Track(input,
                           iTracker.MatchMethod(new MemberDescriptor(KnownType.Microsoft_AspNetCore_Http_HeaderDictionaryExtensions, "Append")),
                           iTracker.MethodHasParameters(ParameterCount),
                           c => IsFirstArgumentAccessControlAllowOrigin((InvocationExpressionSyntax)c.Node, c.SemanticModel)
                                && IsSecondArgumentStarString((InvocationExpressionSyntax)c.Node, c.SemanticModel));

            iTracker.Track(input,
                           iTracker.MatchMethod(new MemberDescriptor(KnownType.System_Collections_Generic_IDictionary_TKey_TValue, "Add")),
                           iTracker.MethodHasParameters(ParameterCount),
                           c => IsFirstArgumentAccessControlAllowOrigin((InvocationExpressionSyntax)c.Node, c.SemanticModel)
                                && IsSecondArgumentStarString((InvocationExpressionSyntax)c.Node, c.SemanticModel),
                           iTracker.IsIHeadersDictionary());

            iTracker.Track(input,
                           iTracker.MatchMethod(new MemberDescriptor(KnownType.Microsoft_AspNetCore_Cors_Infrastructure_CorsPolicyBuilder, "WithOrigins")),
                           c => ContainsStar(((InvocationExpressionSyntax)c.Node).ArgumentList.Arguments.Select(a => a.Expression), c.SemanticModel));

            iTracker.Track(input, iTracker.MatchMethod(new MemberDescriptor(KnownType.Microsoft_AspNetCore_Cors_Infrastructure_CorsPolicyBuilder, "AllowAnyOrigin")));

            var ocTracker = Language.Tracker.ObjectCreation;
            ocTracker.Track(input,
                            ocTracker.MatchConstructor(KnownType.Microsoft_AspNetCore_Cors_Infrastructure_CorsPolicyBuilder),
                            c => ContainsStar((ObjectCreationExpressionSyntax)c.Node, c.SemanticModel));
        }

        private static bool IsFirstArgumentAccessControlAllowOrigin(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
            invocation.ArgumentList.Arguments.First().Expression switch
            {
                LiteralExpressionSyntax literal => semanticModel.GetConstantValue(literal) is {HasValue: true, Value: AccessControlAllowOriginHeader},
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
                LiteralExpressionSyntax literal => ContainsStar(semanticModel.GetConstantValue(literal)),
                IdentifierNameSyntax identifier => ContainsStar(semanticModel.GetConstantValue(identifier)),
                ImplicitArrayCreationExpressionSyntax arrayCreation => ContainsStar(arrayCreation.Initializer.Expressions, semanticModel),
                ObjectCreationExpressionSyntax objectCreation => ContainsStar(objectCreation, semanticModel),
                _ => false
            };

        private static bool ContainsStar(IEnumerable<ExpressionSyntax> expressions, SemanticModel semanticModel) =>
            expressions.Any(expression => ContainsStar(semanticModel.GetConstantValue(expression)));

        private static bool ContainsStar(Optional<object> constantValue) =>
            constantValue is {HasValue: true, Value: StarConstant};

        private static bool ContainsStar(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel) =>
            (objectCreation.IsKnownType(KnownType.Microsoft_Extensions_Primitives_StringValues, semanticModel)
             || objectCreation.IsKnownType(KnownType.Microsoft_AspNetCore_Cors_Infrastructure_CorsPolicyBuilder, semanticModel))
            && objectCreation.ArgumentList is { }
            && objectCreation.ArgumentList.Arguments.Any(argument => IsStar(argument.Expression, semanticModel));
    }
}
