using System;

namespace Tests.Diagnostics
{
    public class RedundantNullCheck
    {
        public void TestRedundantNullCheck(Object a, RedundantNullCheck b)
        {

            if (a != null && a is RedundantNullCheck) // Noncompliant {{Remove this unnecessary null check; 'is' returns false for nulls.}}
//              ^^^^^^^^^
            {

            }

            if (null != a && a is RedundantNullCheck) // Noncompliant
//              ^^^^^^^^^
            {

            }

            if (a is RedundantNullCheck && a != null) // Noncompliant
//                                         ^^^^^^^^^
            {

            }

            if (a is RedundantNullCheck && a != null && b != null) // Noncompliant
//                                         ^^^^^^^^^
            {

            }

            if (a != null && a is RedundantNullCheck aTyped0) // Noncompliant
            {

            }

            if (a is RedundantNullCheck aTyped1 && aTyped1 != null)
            {

            }

            if (a is RedundantNullCheck aTyped2 && a != null) // Noncompliant
            {

            }

            if (((a) != ((null))) && ((a) is RedundantNullCheck aTyped3)) // Noncompliant
            {

            }

            if (a != null && b != null && b is RedundantNullCheck) // FN
            {

            }

            if (a != null || a is RedundantNullCheck) // Compliant - not AND operator
            {

            }

            if (a != null && !(a is RedundantNullCheck)) // Compliant
            {

            }

            if (a == null && a is RedundantNullCheck) // Compliant - rule ConditionEvaluatesToConstant will raise issue here
            {

            }

            if (a != null && b is RedundantNullCheck) // Compliant
            {

            }

            if (b != null && a is RedundantNullCheck) // Compliant
            {

            }

            if (a != null && a != null) // Compliant - not related to this rule
            {

            }
        }

        public void TestRedundantInvertedNullCheck(Object a, RedundantNullCheck b)
        {

            if (a == null || !(a is RedundantNullCheck)) // Noncompliant {{Remove this unnecessary null check; 'is' returns false for nulls.}}
//              ^^^^^^^^^
            {

            }

            if (!(a is RedundantNullCheck) || a == null) // Noncompliant
            {

            }

            if (a == null || !(a is RedundantNullCheck aTyped1)) // Noncompliant
            {

            }

            if (!(a is RedundantNullCheck aTyped2) || a == null) // Noncompliant
//                                                    ^^^^^^^^^
            {

            }

            if (((!((a) is RedundantNullCheck))) || ((a) == (null))) // Noncompliant
            {

            }

            if (a == null || !(b is RedundantNullCheck)) // Compliant
            {

            }

            if (b == null || !(a is RedundantNullCheck)) // Compliant
            {

            }

            if (a == null || a != null) // Compliant - not related to this rule
            {

            }
        }
    }
}
