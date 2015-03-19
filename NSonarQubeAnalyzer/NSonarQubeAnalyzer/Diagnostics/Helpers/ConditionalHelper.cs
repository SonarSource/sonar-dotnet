using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSonarQubeAnalyzer.Diagnostics.Helpers
{
    public static class ConditionalHelper
    {
        #region If

        public static IList<IfStatementSyntax> GetPreceedingIfsInConditionChain(this IfStatementSyntax ifStatement)
        {
            var ifList = new List<IfStatementSyntax>();
            var currentIf = ifStatement;

            while (currentIf.Parent is ElseClauseSyntax)
            {
                var preceedingIf = currentIf.Parent.Parent as IfStatementSyntax;
                if (preceedingIf == null)
                {
                    break;
                }

                ifList.Add(preceedingIf);
                currentIf = preceedingIf;
            }

            ifList.Reverse();
            return ifList;
        }

        public static IEnumerable<StatementSyntax> GetPreceedingStatementsInConditionChain(this IfStatementSyntax ifStatement)
        {
            return GetPreceedingIfsInConditionChain(ifStatement).Select(i => i.Statement);
        }

        public static IEnumerable<ExpressionSyntax> GetPreceedingConditionsInConditionChain(this IfStatementSyntax ifStatement)
        {
            return GetPreceedingIfsInConditionChain(ifStatement).Select(i => i.Condition);
        }

        #endregion

        #region Switch

        public static IList<SwitchSectionSyntax> GetPreceedingSections(this SwitchSectionSyntax caseStatement)
        {
            var caseList = new List<SwitchSectionSyntax>();

            var switchStatement = (SwitchStatementSyntax)caseStatement.Parent;

            var currentSectionIndex = switchStatement.Sections.IndexOf(caseStatement);

            for (int i = 0; i < currentSectionIndex; i++)
            {
                caseList.Add(switchStatement.Sections[i]);
            }

            return caseList;
        }

        #endregion
    }
}
