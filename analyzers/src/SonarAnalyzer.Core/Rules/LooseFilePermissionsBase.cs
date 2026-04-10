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

namespace SonarAnalyzer.Core.Rules;

public abstract class LooseFilePermissionsBase<TSyntaxKind, TMemberAccess> : SonarDiagnosticAnalyzer
    where TSyntaxKind : struct
    where TMemberAccess : SyntaxNode
{
    protected const string DiagnosticId = "S2612";
    protected const string Everyone = "Everyone";
    protected const string MessageFormat = "Make sure this permission is safe.";

    protected readonly DiagnosticDescriptor rule;

    protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
    protected abstract void VisitInvocations(SonarSyntaxNodeReportingContext context);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

    protected LooseFilePermissionsBase() =>
        rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(c =>
            {
                c.RegisterNodeAction(Language.GeneratedCodeRecognizer, VisitInvocations, Language.SyntaxKind.InvocationExpression);
                c.RegisterNodeAction(Language.GeneratedCodeRecognizer, VisitAssignments, Language.SyntaxKind.IdentifierName);
            });

    protected void VisitAssignments(SonarSyntaxNodeReportingContext context)
    {
        var node = context.Node;
        if (IsFileAccessPermissions(node, context.Model) && !Language.Syntax.IsPartOfBinaryNegationOrCondition(node))
        {
            context.ReportIssue(rule, node);
        }
    }

    protected bool IsSetAccessRule(SyntaxNode invocation, SemanticModel model) =>
        Language.Syntax.NodeExpression(invocation) is TMemberAccess memberAccess
        && Language.Syntax.IsMemberAccessOnKnownType(memberAccess, "SetAccessRule", KnownType.System_Security_AccessControl_FileSystemSecurity, model);

    protected bool IsAddAccessRule(SyntaxNode invocation, SemanticModel model) =>
        Language.Syntax.NodeExpression(invocation) is TMemberAccess memberAccess
        && Language.Syntax.IsMemberAccessOnKnownType(memberAccess, "AddAccessRule", KnownType.System_Security_AccessControl_FileSystemSecurity, model);

    protected bool IsFileAccessPermissions(SyntaxNode node, SemanticModel model) =>
        Language.Syntax.NodeIdentifier(node) is { } identifier
        && LooseFilePermissionsConfig.WeakFileAccessPermissions.Contains(identifier.Text)
        && node.IsKnownType(KnownType.Mono_Unix_FileAccessPermissions, model);
}
