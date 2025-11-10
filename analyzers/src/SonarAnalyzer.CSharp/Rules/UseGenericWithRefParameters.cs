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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseGenericWithRefParameters : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S4047";
    private const string MessageFormat = "Make this method generic and replace the 'object' parameter with a type parameter.";
    private const string SecondaryMessage = "Replace this parameter with a type parameter.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var methodDeclaration = (MethodDeclarationSyntax)c.Node;
                var methodSymbol = c.Model.GetDeclaredSymbol(methodDeclaration);

                if (methodSymbol is null ||
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
                    var parameterLocations = refObjectParameters.Select(p => p.Locations.FirstOrDefault()?.ToSecondary(SecondaryMessage)).WhereNotNull();
                    c.ReportIssue(Rule, methodDeclaration.Identifier, parameterLocations);
                }
            },
            SyntaxKind.MethodDeclaration);

    private static bool IsRefObject(IParameterSymbol parameter) =>
        parameter.RefKind == RefKind.Ref &&
        parameter.Type.Is(KnownType.System_Object);
}
