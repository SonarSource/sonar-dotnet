/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Rules;

public abstract class DateAndTimeShouldNotBeUsedasTypeForPrimaryKeyBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S3363";

    protected static readonly string[] KeyAttributeTypeNames = TypeNamesForAttribute(KnownType.System_ComponentModel_DataAnnotations_KeyAttribute);
    protected static readonly KnownType[] TemporalTypes = new[]
    {
        KnownType.System_DateOnly,
        KnownType.System_DateTime,
        KnownType.System_DateTimeOffset,
        KnownType.System_TimeOnly,
        KnownType.System_TimeSpan
    };

    protected abstract IEnumerable<SyntaxNode> TypeNodesOfTemporalKeyProperties(SonarSyntaxNodeReportingContext context);

    protected override string MessageFormat => "'{0}' should not be used as a type for primary keys";

    protected DateAndTimeShouldNotBeUsedasTypeForPrimaryKeyBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(startContext =>
        {
            if (ShouldRegisterAction(startContext.Compilation))
            {
                startContext.RegisterNodeAction(Language.GeneratedCodeRecognizer, nodeContext =>
                {
                    foreach (var propertyType in TypeNodesOfTemporalKeyProperties(nodeContext))
                    {
                        nodeContext.ReportIssue(Rule, propertyType, Language.Syntax.NodeIdentifier(propertyType)?.ValueText);
                    }
                }, Language.SyntaxKind.ClassDeclaration);
            }
        });

    protected static bool IsKeyPropertyBasedOnName(string propertyName, string className) =>
        propertyName.Equals("Id", StringComparison.InvariantCultureIgnoreCase)
        || propertyName.Equals($"{className}Id", StringComparison.InvariantCultureIgnoreCase);

    protected virtual bool IsTemporalType(string propertyTypeName) =>
        Array.Exists(TemporalTypes, x => propertyTypeName.Equals(x.TypeName, Language.NameComparison)
                                         || propertyTypeName.Equals(x.FullName, Language.NameComparison));

    protected bool MatchesAttributeName(string attributeName, string[] candidates) =>
        Array.Exists(candidates, x => attributeName.Equals(x, Language.NameComparison));

    private static bool ShouldRegisterAction(Compilation compilation) =>
        compilation.GetTypeByMetadataName(KnownType.Microsoft_EntityFrameworkCore_DbContext) is not null
        || compilation.GetTypeByMetadataName(KnownType.Microsoft_EntityFramework_DbContext) is not null;

    private static string[] TypeNamesForAttribute(KnownType attributeType) => new[]
    {
        attributeType.TypeName,
        attributeType.FullName,
        RemoveFromEnd(attributeType.TypeName, "Attribute"),
        RemoveFromEnd(attributeType.FullName, "Attribute"),
    };

    private static string RemoveFromEnd(string text, string subtextFromEnd) =>
        text.Substring(0, text.LastIndexOf(subtextFromEnd));
}
