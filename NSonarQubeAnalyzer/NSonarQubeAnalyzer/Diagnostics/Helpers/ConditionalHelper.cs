using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NSonarQubeAnalyzer.Diagnostics.Helpers
{
    public static class ConditionalHelper
    {
        #region If

        public static IList<IfStatementSyntax> GetPrecedingIfsInConditionChain(this IfStatementSyntax ifStatement)
        {
            var ifList = new List<IfStatementSyntax>();
            var currentIf = ifStatement;

            while (currentIf.Parent is ElseClauseSyntax)
            {
                var precedingIf = currentIf.Parent.Parent as IfStatementSyntax;
                if (precedingIf == null)
                {
                    break;
                }

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

        public static IList<SwitchSectionSyntax> GetPrecedingSections(this SwitchSectionSyntax caseStatement)
        {
            var caseList = new List<SwitchSectionSyntax>();

            var switchStatement = (SwitchStatementSyntax)caseStatement.Parent;

            var currentSectionIndex = switchStatement.Sections.IndexOf(caseStatement);

            for (var i = 0; i < currentSectionIndex; i++)
            {
                caseList.Add(switchStatement.Sections[i]);
            }

            return caseList;
        }

        #endregion
    }
}
