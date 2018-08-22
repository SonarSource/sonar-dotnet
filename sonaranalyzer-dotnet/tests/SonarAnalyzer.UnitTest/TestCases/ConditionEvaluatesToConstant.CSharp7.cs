namespace Tests.Diagnostics
{
    public class ExpressionBodyTest
    {
        void ConstantPattern(object o)
        {
            if (o == null)
            {
                if (o is null) // Noncompliant, always true
                {
                    o.ToString();
                }
                else
                { // Secondary, not executed code
                    o.ToString();
                }
            }
        }

        void ConstantPattern_NotIsNull(object o)
        {
            if (o == null)
            {
                if (!(o is null)) // Noncompliant, always false
                { // Secondary, not executed code
                    o.ToString();
                }
                else
                {
                    o.ToString();
                }
            }
        }

        void VariableDesignationPattern_Variable(object o)
        {
            if (o is string s)
            {
                if (s == null) // Noncompliant, always false
                { // Secondary, not executed code
                    s.ToString();
                }
            }
        }

        void VariableDesignationPattern_Source(object o)
        {
            // We can set NotNull constraint only for one of the variables in the if condition
            // and we choose the declared variable because it is more likely to have usages of
            // it inside the statement body.
            if (o is string s)
            {
                if (o == null) // Compliant, False Negative
                {
                    o.ToString();
                }
            }
        }

        void Patterns_In_Loops(object o, object[] items)
        {
            while (o is string s)
            {
                if (s == null) // Noncompliant, always false
                { // Secondary, not executed code
                    s.ToString();
                }
            }

            do
            {
                // The condition is evaluated after the first execution, so we cannot test s
            }
            while (o is string s);

            for (int i = 0; i < length && items[i] is string s; i++)
            {
                if (s != null) // Noncompliant, always true
                {
                    s.ToString();
                }
            }
        }
    }
}
