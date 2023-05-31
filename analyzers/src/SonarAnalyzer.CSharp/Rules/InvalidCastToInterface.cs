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

using static SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp.InvalidCastToInterface;
using TypeMap = System.Collections.Generic.Dictionary<Microsoft.CodeAnalysis.INamedTypeSymbol, System.Collections.Generic.HashSet<Microsoft.CodeAnalysis.INamedTypeSymbol>>;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InvalidCastToInterfaceAnalyzer : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S1944";
    private const string MessageFormat = "{0}"; // This format string can be removed after we drop the old SE engine.
    private const string MessageReviewFormat = "Review this cast; in this project there's no type that {0}.";

    public static readonly DiagnosticDescriptor S1944 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(S1944);
    protected override bool EnableConcurrentExecution => false;

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterCompilationStartAction(
            compilationStartContext =>
            {
                var interfaceImplementer = BuildTypeMap(compilationStartContext.Compilation.GlobalNamespace.GetAllNamedTypes());
                compilationStartContext.RegisterNodeAction(
                    c =>
                    {
                        var cast = (CastExpressionSyntax)c.Node;
                        var interfaceType = c.SemanticModel.GetTypeInfo(cast.Type).Type as INamedTypeSymbol;
                        var expressionType = c.SemanticModel.GetTypeInfo(cast.Expression).Type as INamedTypeSymbol;
                        if (IsImpossibleCast(interfaceImplementer, interfaceType, expressionType))
                        {
                            var location = cast.Type.GetLocation();
                            var interfaceTypeName = interfaceType.ToMinimalDisplayString(c.SemanticModel, location.SourceSpan.Start);
                            var expressionTypeName = expressionType.ToMinimalDisplayString(c.SemanticModel, location.SourceSpan.Start);
                            var messageReasoning = expressionType.IsInterface()
                                ? $"implements both '{expressionTypeName}' and '{interfaceTypeName}'"
                                : $"extends '{expressionTypeName}' and implements '{interfaceTypeName}'";
                            c.ReportIssue(Diagnostic.Create(S1944, location, string.Format(MessageReviewFormat, messageReasoning)));
                        }
                    },
                    SyntaxKind.CastExpression);
            });
    }

    private static TypeMap BuildTypeMap(IEnumerable<INamedTypeSymbol> allTypes)
    {
        var ret = new TypeMap();
        foreach (var type in allTypes)
        {
            if (type.IsInterface())
            {
                Add(type, type);
            }
            foreach (var @interface in type.AllInterfaces.Select(i => i.OriginalDefinition))    // FIXME: OriginalDefinition? Why? Add or remove
            {
                Add(@interface, type);
            }
        }
        return ret;

        void Add(INamedTypeSymbol key, INamedTypeSymbol value)
        {
            if (!ret.TryGetValue(key, out var values))
            {
                values = new();
                ret.Add(key, values);
            }
            values.Add(value);
        }
    }

    private static bool IsImpossibleCast(TypeMap interfaceImplementer, INamedTypeSymbol interfaceType, INamedTypeSymbol expressionType)
    {
        return interfaceType.IsInterface()
            && ConcreteImplementationExists(interfaceType)
            && expressionType is not null
            && !expressionType.IsSealed
            && !expressionType.Is(KnownType.System_Object)
            && (!expressionType.IsInterface() || ConcreteImplementationExists(expressionType))
            && interfaceImplementer.TryGetValue(interfaceType.OriginalDefinition, out var implementers)
            && !implementers.Any(x => x.DerivesOrImplements(expressionType.OriginalDefinition));

        bool ConcreteImplementationExists(INamedTypeSymbol type) =>
            interfaceImplementer.ContainsKey(type) && interfaceImplementer[type].Any(t => t.IsClassOrStruct());
    }
}
