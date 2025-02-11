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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PropertyNamesShouldNotMatchGetMethods : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S4059";
    private const string MessageFormat = "Change either the name of property '{0}' or the name of method '{1}' to make them distinguishable.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterSymbolAction(c => // Invoked twice for partial properties: once for the property declaration and one for the implementation
        {
            var propertySymbol = (IPropertySymbol)c.Symbol;
            if (!propertySymbol.IsPubliclyAccessible() || propertySymbol.IsOverride)
            {
                return;
            }
            var methods =  propertySymbol.ContainingType.GetMembers().OfType<IMethodSymbol>().Where(x => x.IsPubliclyAccessible()).ToArray();
            if (Array.Find(methods, x => AreCollidingNames(propertySymbol.Name, x.Name)) is { } collidingMethod)
            {
                // When dealing with partial properties, IsPartialDefinition is true only for the declaration, we use this to avoid reporting the secondary location twice
                List<SecondaryLocation> secondaryLocation = propertySymbol.IsPartialDefinition() ? [] : [new(collidingMethod.Locations.First(), string.Empty)];
                c.ReportIssue(Rule, propertySymbol.Locations.First(), secondaryLocation, propertySymbol.Name, collidingMethod.Name);
            }
        }, SymbolKind.Property);

    private static bool AreCollidingNames(string propertyName, string methodName) =>
        methodName.Equals(propertyName, StringComparison.OrdinalIgnoreCase) || methodName.Equals("Get" + propertyName, StringComparison.OrdinalIgnoreCase);
}
