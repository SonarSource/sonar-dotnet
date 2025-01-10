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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OperatorsShouldBeOverloadedConsistently : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S4050";
    private const string MessageFormat = "Provide an implementation for: {0}.";

    private static readonly DiagnosticDescriptor Rule =
        DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var classDeclaration = (ClassDeclarationSyntax)c.Node;
                var classSymbol = (INamedTypeSymbol)c.ContainingSymbol;

                if (classDeclaration.Identifier.IsMissing
                    || !classSymbol.IsPubliclyAccessible())
                {
                    return;
                }

                var missingMethods = FindMissingMethods(classSymbol).ToList();
                if (missingMethods.Count > 0)
                {
                    c.ReportIssue(Rule, classDeclaration.Identifier, missingMethods.ToSentence(quoteWords: true));
                }
            },
            // This rule is not applicable for records, as for records it is not possible to override the == operator.
            SyntaxKind.ClassDeclaration);

    private static IEnumerable<string> FindMissingMethods(INamedTypeSymbol classSymbol)
    {
        var implementedMethods = GetImplementedMethods(classSymbol).ToHashSet();
        var requiredMethods = new HashSet<string>();

        if (implementedMethods.Contains(MethodName.OperatorPlus)
            || implementedMethods.Contains(MethodName.OperatorMinus)
            || implementedMethods.Contains(MethodName.OperatorMultiply)
            || implementedMethods.Contains(MethodName.OperatorDivide)
            || implementedMethods.Contains(MethodName.OperatorRemainder))
        {
            requiredMethods.Add(MethodName.OperatorEquals);
            requiredMethods.Add(MethodName.OperatorNotEquals);
            requiredMethods.Add(MethodName.ObjectEquals);
            requiredMethods.Add(MethodName.ObjectGetHashCode);
        }

        if (implementedMethods.Contains(MethodName.OperatorEquals))
        {
            requiredMethods.Add(MethodName.ObjectEquals);
            requiredMethods.Add(MethodName.ObjectGetHashCode);
        }

        if (implementedMethods.Contains(MethodName.OperatorNotEquals))
        {
            requiredMethods.Add(MethodName.ObjectEquals);
            requiredMethods.Add(MethodName.ObjectGetHashCode);
        }

        return requiredMethods.Except(implementedMethods);
    }

    private static IEnumerable<string> GetImplementedMethods(INamedTypeSymbol classSymbol)
    {
        foreach (var member in classSymbol.GetMembers().OfType<IMethodSymbol>().Where(x => !x.IsConstructor()))
        {
            if (ImplementedOperator(member) is { } name)
            {
                yield return name;
            }
            else if (KnownMethods.IsObjectEquals(member))
            {
                yield return MethodName.ObjectEquals;
            }
            else if (KnownMethods.IsObjectGetHashCode(member))
            {
                yield return MethodName.ObjectGetHashCode;
            }
        }
    }

    private static string ImplementedOperator(IMethodSymbol member) =>
        member switch
        {
            { MethodKind: not MethodKind.UserDefinedOperator } => null,
            _ when KnownMethods.IsOperatorBinaryPlus(member) => MethodName.OperatorPlus,
            _ when KnownMethods.IsOperatorBinaryMinus(member) => MethodName.OperatorMinus,
            _ when KnownMethods.IsOperatorBinaryMultiply(member) => MethodName.OperatorMultiply,
            _ when KnownMethods.IsOperatorBinaryDivide(member) => MethodName.OperatorDivide,
            _ when KnownMethods.IsOperatorBinaryModulus(member) => MethodName.OperatorRemainder,
            _ when KnownMethods.IsOperatorEquals(member) => MethodName.OperatorEquals,
            _ when KnownMethods.IsOperatorNotEquals(member) => MethodName.OperatorNotEquals,
            _ => null
        };

    private static class MethodName
    {
        public const string OperatorPlus = "operator+";
        public const string OperatorMinus = "operator-";
        public const string OperatorMultiply = "operator*";
        public const string OperatorDivide = "operator/";
        public const string OperatorRemainder = "operator%";
        public const string OperatorEquals = "operator==";
        public const string OperatorNotEquals = "operator!=";

        public const string ObjectEquals = "Object.Equals";
        public const string ObjectGetHashCode = "Object.GetHashCode";
    }
}
