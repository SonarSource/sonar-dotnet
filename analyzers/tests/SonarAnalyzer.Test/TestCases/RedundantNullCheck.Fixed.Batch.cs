using System;

namespace Tests.Diagnostics
{
    public class RedundantNullCheck
    {
        public void TestRedundantNullCheck(Object a, RedundantNullCheck b)
        {

            if (a is RedundantNullCheck) // Fixed
            {

            }

            if (a is RedundantNullCheck) // Fixed
            {

            }

            if (a is RedundantNullCheck) // Fixed
            {

            }

            if (a is RedundantNullCheck && b != null) // Fixed
            {

            }

            if (a is RedundantNullCheck aTyped0) // Fixed
            {

            }

            if (a is RedundantNullCheck aTyped1 && aTyped1 != null)
            {

            }

            if (a is RedundantNullCheck aTyped2) // Fixed
            {

            }

            if (((a) is RedundantNullCheck aTyped3)) // Fixed
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

            if (!(a is RedundantNullCheck)) // Fixed
            {

            }

            if (!(a is RedundantNullCheck)) // Fixed
            {

            }

            if (!(a is RedundantNullCheck aTyped1)) // Fixed
            {

            }

            if (!(a is RedundantNullCheck aTyped2)) // Fixed
            {

            }

            if (((!((a) is RedundantNullCheck)))) // Fixed
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
