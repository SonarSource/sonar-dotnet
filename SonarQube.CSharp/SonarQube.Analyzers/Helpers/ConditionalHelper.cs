using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarQube.Analyzers.Helpers
{
    public static class ConditionalHelper
    {
        #region If

        public static IList<IfStatementSyntax> GetPrecedingIfsInConditionChain(this IfStatementSyntax ifStatement)
        {
            var ifList = new List<IfStatementSyntax>();
            var currentIf = ifStatement;

            while (currentIf.Parent is ElseClauseSyntax &&
                currentIf.Parent.Parent is IfStatementSyntax)
            {
                var precedingIf = (IfStatementSyntax) currentIf.Parent.Parent;
                ifList.Add(precedingIf);
                currentIf = precedingIf;
            }

            ifList.Reverse();
            return ifList;
        }

        public static IEnumerable<StatementSyntax> GetPrecedingStatementsInConditionChain(this IfStatementSyntax ifStatement)
        {
            return GetPrecedingIfsInConditionChain(ifStatement).Select(i => i.Statement);
        }

        public static IEnumerable<ExpressionSyntax> GetPrecedingConditionsInConditionChain(this IfStatementSyntax ifStatement)
        {
            return GetPrecedingIfsInConditionChain(ifStatement).Select(i => i.Condition);
        }

        #endregion

        #region Switch

        public static IEnumerable<SwitchSectionSyntax> GetPrecedingSections(this SwitchSectionSyntax caseStatement)
        {
            if (caseStatement == null)
            {
                return new SwitchSectionSyntax[0];
            }

            var switchStatement = (SwitchStatementSyntax)caseStatement.Parent;

            var currentSectionIndex = switchStatement.Sections.IndexOf(caseStatement);

            return switchStatement.Sections.Take(currentSectionIndex);
        }

        #endregion
    }
}
