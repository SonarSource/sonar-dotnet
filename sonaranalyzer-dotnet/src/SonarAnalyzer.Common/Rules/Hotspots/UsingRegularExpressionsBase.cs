/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using System.Linq;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class UsingRegularExpressionsBase<TSyntaxKind> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S4784";
        protected const string MessageFormat = "Make sure that using a regular expression is safe here.";

        protected InvocationTracker<TSyntaxKind> InvocationTracker { get; set; }

        protected ObjectCreationTracker<TSyntaxKind> ObjectCreationTracker { get; set; }

        private readonly ISet<char> SpecialCharacters = new HashSet<char> { '{', '+', '*' };

        protected override void Initialize(SonarAnalysisContext context)
        {
            InvocationTracker.Track(context,
                InvocationTracker.MatchMethod(
                    new MemberDescriptor(KnownType.System_Text_RegularExpressions_Regex, "IsMatch"),
                    new MemberDescriptor(KnownType.System_Text_RegularExpressions_Regex, "Match"),
                    new MemberDescriptor(KnownType.System_Text_RegularExpressions_Regex, "Matches"),
                    new MemberDescriptor(KnownType.System_Text_RegularExpressions_Regex, "Replace"),
                    new MemberDescriptor(KnownType.System_Text_RegularExpressions_Regex, "Split")),
                SecondArgumentIsHardcodedRegex(),
                InvocationTracker.MethodIsStatic());

            ObjectCreationTracker.Track(context,
                ObjectCreationTracker.MatchConstructor(KnownType.System_Text_RegularExpressions_Regex),
                FirstArgumentIsHardcodedRegex());
        }

        protected abstract string GetStringLiteralAtIndex(InvocationContext context, int index);

        protected abstract string GetStringLiteralAtIndex(ObjectCreationContext context, int index);

        private InvocationCondition SecondArgumentIsHardcodedRegex() =>
            (context) =>
                GetStringLiteralAtIndex(context, 1) is string hardcodedString &&
                IsComplexRegex(hardcodedString);

        private ObjectCreationCondition FirstArgumentIsHardcodedRegex() =>
            (context) =>
                GetStringLiteralAtIndex(context, 0) is string hardcodedString &&
                IsComplexRegex(hardcodedString);

        private bool IsComplexRegex(string s) =>
            s != null &&
            s.Length > 2 &&
            s.ToCharArray().Count(c => SpecialCharacters.Contains(c)) > 1;
    }
}
