/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ImplementSerializationMethodsCorrectlyBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S3927";
        private const string MessageFormat = "Make this method {0}.";
        private const string ProblemParameterText = "have a single parameter of type 'StreamingContext'";
        private const string ProblemGenericParameterText = "have no type parameters";
        private const string ProblemPublicText = "non-public";

        private readonly DiagnosticDescriptor rule;
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ImmutableArray<KnownType> SerializationAttributes =
            ImmutableArray.Create(
                KnownType.System_Runtime_Serialization_OnSerializingAttribute,
                KnownType.System_Runtime_Serialization_OnSerializedAttribute,
                KnownType.System_Runtime_Serialization_OnDeserializingAttribute,
                KnownType.System_Runtime_Serialization_OnDeserializedAttribute
            );

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
        protected abstract string MethodStaticMessage { get; }
        protected abstract string MethodReturnTypeShouldBeVoidMessage { get; }
        protected abstract Location GetIdentifierLocation(IMethodSymbol methodSymbol);

        protected ImplementSerializationMethodsCorrectlyBase(System.Resources.ResourceManager rspecResources) =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSymbolAction(
                c =>
                {
                    var methodSymbol = (IMethodSymbol)c.Symbol;
                    if (!methodSymbol.GetAttributes(SerializationAttributes).Any())
                    {
                        return;
                    }

                    var issues = FindIssues(methodSymbol);
                    if (issues.Any() && GetIdentifierLocation(methodSymbol) is { } location)
                    {
                        c.ReportDiagnosticIfNonGenerated(GeneratedCodeRecognizer, Diagnostic.Create(SupportedDiagnostics[0], location, issues.ToSentence()));
                    }
                },
                SymbolKind.Method);

        private IEnumerable<string> FindIssues(IMethodSymbol methodSymbol)
        {
            var ret = new List<string>();
            Evaluate(ProblemPublicText, methodSymbol.DeclaredAccessibility == Accessibility.Public);
            Evaluate(MethodStaticMessage, methodSymbol.IsStatic);
            Evaluate(MethodReturnTypeShouldBeVoidMessage, !methodSymbol.ReturnsVoid);
            Evaluate(ProblemGenericParameterText, !methodSymbol.TypeParameters.IsEmpty);
            Evaluate(ProblemParameterText, methodSymbol.Parameters.Length != 1 || !methodSymbol.Parameters.First().IsType(KnownType.System_Runtime_Serialization_StreamingContext));
            return ret;

            void Evaluate(string message, bool condition)
            {
                if (condition)
                {
                    ret.Add(message);
                }
            }
        }
    }
}
