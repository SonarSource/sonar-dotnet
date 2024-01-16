using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text
using System.Threading.Tasks;

namespace SonarAnalyzer.Test.TestCasesForRuleFailure
{
    public void Method2(int i, int j) { }

    class DuplicatedInterfaces: IList, IList {}

    class InvalidSyntax
    {
        ;
        public void Method(int i, int j,)
        {
            var x = 6
            for (int i = 0; i < 5; i++)
            }

        private class C
        {
            public C()
        }
    }

    class
    {
        int i;
        public override int GetHashCode()
        {
            int? v = null;
            if (v > missing)
            { }

            return i; // we don't report on this
        }
    }
}
