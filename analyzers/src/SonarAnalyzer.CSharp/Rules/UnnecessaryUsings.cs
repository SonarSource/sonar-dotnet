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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnnecessaryUsings : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S1128";
    private const string MessageFormat = "Remove this unnecessary 'using'.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            c =>
            {
                // When using top level statements, we are called twice for the same compilation unit. The second call has the containing symbol kind equal to `Method`.
                if (c.IsRedundantPositionalRecordContext())
                {
                    return;
                }
                // Logic copied from
                // https://github.com/dotnet/roslyn/blob/218d39d6cb4b665e7a03663596490a81d87ed07f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Helpers/RemoveUnnecessaryImports/CSharpUnnecessaryImportsProvider.cs#L23-L43
                // It is used by IDE0005 https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0005
                var diagnostics = c.SemanticModel.GetDiagnostics(cancellationToken: c.Cancel);
                var root = c.Node.SyntaxTree.GetRoot();
                HashSet<Diagnostic> reported = null;
                foreach (var diagnostic in diagnostics)
                {
                    if (diagnostic.Id == "CS8019" // Hidden compiler error "HDN_UnusedUsingDirective" https://github.com/dotnet/roslyn/blob/218d39d6cb4b665e7a03663596490a81d87ed07f/src/Compilers/CSharp/Portable/Errors/ErrorCode.cs#L1271
                        && root.FindNode(diagnostic.Location.SourceSpan) is UsingDirectiveSyntax usingDirective
                        && !(reported ??= new()).Contains(diagnostic))
                    {
                        reported.Add(diagnostic);
                        c.ReportIssue(Diagnostic.Create(Rule, usingDirective.GetLocation()));
                    }
                }
            },
            SyntaxKind.CompilationUnit);
}
