using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class GotoStatement
    {
        void foo(int a)
        {
            var @goto = 5;

            goto Label; //Noncompliant {{Remove this use of 'goto'.}}
//          ^^^^

            Label:
            ;

            int n = 5;
            switch (n)
            {
                case 1:
                    break;
                case 2:
                    goto default; //Noncompliant
//                  ^^^^
                case 3:
                    goto case 1; //Noncompliant
                default:
                    break;
            }
        }
    }
}
