/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseGenericWithRefParameters : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4047";
        private const string MessageFormat = "Make this method generic and replace the 'object' parameter with a type parameter.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(c =>
            {
                var methodDeclaration = (MethodDeclarationSyntax)c.Node;
                var methodSymbol = c.SemanticModel.GetDeclaredSymbol(methodDeclaration);

                if (methodSymbol == null ||
                    methodDeclaration.Identifier.IsMissing)
                {
                    return;
                }

                var refObjectParameters = methodSymbol
                    .GetParameters()
                    .Where(IsRefObject)
                    .ToList();

                if (refObjectParameters.Count > 0)
                {
                    var parameterLocations = refObjectParameters.Select(p => p.Locations.FirstOrDefault()?.ToSecondary()).WhereNotNull();
                    c.ReportIssue(rule, methodDeclaration.Identifier, parameterLocations);
                }

            },
            SyntaxKind.MethodDeclaration);
        }

        private static bool IsRefObject(IParameterSymbol parameter)
        {
            return parameter.RefKind == RefKind.Ref &&
                   parameter.Type.Is(KnownType.System_Object);
        }
    }
}
