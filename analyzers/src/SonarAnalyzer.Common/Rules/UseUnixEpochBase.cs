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

namespace SonarAnalyzer.Rules;

public abstract class UseUnixEpochBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    public const string DiagnosticId = "S6588";

    protected UseUnixEpochBase() : base(DiagnosticId) { }
}

public abstract class UseUnixEpochBase<TSyntaxKind, TLiteralExpression, TMemberAccessExpression> : UseUnixEpochBase<TSyntaxKind>
    where TSyntaxKind : struct
    where TLiteralExpression : SyntaxNode
    where TMemberAccessExpression : SyntaxNode
{
    private const int EpochYear = 1970;
    private const int EpochMonth = 1;
    private const int EpochDay = 1;

    private static readonly ImmutableArray<KnownType> TypesWithUnixEpochField = ImmutableArray.Create(KnownType.System_DateTime, KnownType.System_DateTimeOffset);

    protected override string MessageFormat => "Use \"{0}.UnixEpoch\" instead of creating {0} instances that point to the unix epoch time";

    protected abstract bool IsDateTimeKindUtc(TMemberAccessExpression memberAccess);
    protected abstract bool IsGregorianCalendar(SyntaxNode node);
    protected abstract bool IsZeroTimeOffset(SyntaxNode node);

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(start =>
        {
            if (!IsUnixEpochSupported(start.Compilation))
            {
                return;
            }

            context.RegisterNodeAction(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    var literalsArguments = Language.Syntax.ArgumentExpressions(c.Node).OfType<TLiteralExpression>();
                    if (!literalsArguments.Any(x => IsValueEqualTo(x, EpochYear) || literalsArguments.Count(x => IsValueEqualTo(x, EpochMonth)) != 2))
                    {
                        return;
                    }

                    var type = c.SemanticModel.GetTypeInfo(c.Node).Type;
                    if (type.IsAny(TypesWithUnixEpochField) && IsEpochCtor(c.Node, c.SemanticModel))
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, c.Node.GetLocation(), type.Name));
                    }
                },
                Language.SyntaxKind.ObjectCreationExpressions);
        });

    protected static bool IsValueEqualTo(TLiteralExpression literal, int value) =>
        int.TryParse(literal.ChildTokens().First().ValueText, out var parsedValue) && parsedValue == value;

    private bool IsEpochCtor(SyntaxNode node, SemanticModel model)
    {
        var methodSymbol = (IMethodSymbol)model.GetSymbolInfo(node).Symbol;
        var lookup = Language.MethodParameterLookup(node, methodSymbol);

        return IsParameterExistingAndLiteralEqualTo("year", EpochYear, lookup)
            && IsParameterExistingAndLiteralEqualTo("month", EpochMonth, lookup)
            && IsParameterExistingAndLiteralEqualTo("day", EpochDay, lookup)
            && IsParameterNonExistingOrLiteralEqualTo("hour", 0, lookup)
            && IsParameterNonExistingOrLiteralEqualTo("minute", 0, lookup)
            && IsParameterNonExistingOrLiteralEqualTo("second", 0, lookup)
            && IsParameterNonExistingOrLiteralEqualTo("millisecond", 0, lookup)
            && IsParameterNonExistingOrLiteralEqualTo("microsecond", 0, lookup)
            && IsDateTimeKindNonExistingOrUtc(lookup)
            && IsCalendarNonExistingOrGregorian(lookup)
            && IsOffsetNonExistingOrZero(lookup);
    }

    private static bool IsParameterExistingAndLiteralEqualTo(string parameterName, int value, IMethodParameterLookup lookup) =>
        lookup.TryGetSyntax(parameterName, out var expressions) && IsLiteralAndEqualTo(expressions[0], value);

    private static bool IsParameterNonExistingOrLiteralEqualTo(string parameterName, int value, IMethodParameterLookup lookup) =>
        !lookup.TryGetSyntax(parameterName, out var expressions) || IsLiteralAndEqualTo(expressions[0], value);

    private static bool IsLiteralAndEqualTo(SyntaxNode node, int value) =>
        node is TLiteralExpression literal && IsValueEqualTo(literal, value);

    private bool IsDateTimeKindNonExistingOrUtc(IMethodParameterLookup lookup) =>
        !lookup.TryGetSyntax("kind", out var expressions)
        || (expressions[0] is TMemberAccessExpression memberAccess && IsDateTimeKindUtc(memberAccess));

    private bool IsCalendarNonExistingOrGregorian(IMethodParameterLookup lookup) =>
        !lookup.TryGetSyntax("calendar", out var expressions) || IsGregorianCalendar(expressions[0]);

    private bool IsOffsetNonExistingOrZero(IMethodParameterLookup lookup) =>
        !lookup.TryGetSyntax("offset", out var expressions) || IsZeroTimeOffset(expressions[0]);

    // DateTime.UnixEpoch introduced at .NET Core 2.1/.NET Standard 2.1
    private static bool IsUnixEpochSupported(Compilation compilation) =>
        compilation.GetTypeByMetadataName(KnownType.System_DateTime) is var dateType && dateType.GetMembers("UnixEpoch").Any();
}
