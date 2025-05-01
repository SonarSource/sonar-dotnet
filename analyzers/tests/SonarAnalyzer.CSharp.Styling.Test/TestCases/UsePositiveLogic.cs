public class Sample
{
    private bool condition;

    public object Not(object value)
    {
        if (!condition) // Noncompliant {{Swap the branches and use positive condition.}}
        //  ^^^^^^^^^^
        {
            return null;
        }
        else
        {
            return value;
        }

        if (!condition) // Compliant, doesn't have 'else'
        {
            return null;
        }

        if (!condition) // Compliant, has `else if`
        {
            return null;
        }
        else if (condition)
        {
            return null;
        }

        if (!condition) // Compliant, has `else if`
        {
            return null;
        }
        else if (!condition)    // Noncompliant
        {
            return null;
        }
        else
        {
            return value;
        }

        if (condition)
        {
            return value;
        }
        else
        {
            return null;
        }

        if (!condition) // Compliant, has `else if`
        {
            return null;
        }
        else if (condition)
        {
            return value;
        }
        else
        {
            return null;
        }
    }

    public object NotEquals(object left, object right)
    {
        if (left != right)  // Noncompliant
        {
            return null;
        }
        else
        {
            return left;
        }

        if (left == right)
        {
            return left;
        }
        else
        {
            return null;
        }
    }

    public object NotEqualsBool(bool left, bool right)
    {
        if (left != right)  // Noncompliant
        {
            return null;
        }
        else
        {
            return left;
        }

        if (left == right)
        {
            return left;
        }
        else
        {
            return null;
        }
    }

    public void Ternary(object value)
    {
        _ = !condition  // Noncompliant
            ? null
            : value;

        _ = condition
            ? value
            : null;
    }

    public void NotAndChain(object value)
    {
        _ = !condition && (!condition && (!condition))  // Noncompliant
            ? null
            : value;

        _ = condition || condition || condition
            ? value
            : null;

        _ = !condition || !condition || !condition  // Noncompliant
            ? null
            : value;

        _ = condition && condition && condition
            ? value
            : null;

        _ = !condition && !(condition || condition && !condition)  // Noncompliant, the outer can be inverted
            ? null
            : value;

        _ = condition || condition || (condition && !condition)
            ? value
            : null;

        _ = !condition && condition && !condition      // Compliant, there's at least one positive
            ? null
            : value;

        _ = !condition && (!condition || !condition)   // Compliant
            ? null
            : value;
    }

    public void PatternMatching(object value)
    {
        _ = value is not null   // Noncompliant
            ? value
            : null;

        _ = value is not true   // Noncompliant
            ? value
            : null;

        _ = value is not string // Noncompliant
            ? value
            : null;

        _ = value is not string and not int and not bool // Noncompliant
            ? value
            : null;

        _ = value is not string or not int or not bool  // Noncompliant
            ? value
            : null;

        _ = value is not string and not int or not bool // Compliant, there's 'or' in the 'and' chain
            ? value
            : null;

        _ = value is null
            ? null
            : value;
    }


    public void ConditionAndPatternChain_Simple(object value)
    {
        _ = !condition && value is not string and not int   // Noncompliant
            ? value
            : null;

        _ = !condition || value is not string or not int    // Noncompliant
            ? value
            : null;

        _ = !condition && value is not string or not int    // Compliant, there's 'or' in the 'and' chain
            ? value
            : null;

        _ = !condition || value is not string and not int   // Compliant, there's 'and' in the 'or' chain
            ? value
            : null;
    }

    public void ConditionAndPatternChain_Long(object value)
    {
        _ = !condition && !condition && value is not string and not int and not bool && !condition  // Noncompliant
            ? value
            : null;

        _ = !condition || !condition || value is not string or not int or not bool || !condition    // Noncompliant
            ? value
            : null;

        _ = !condition && !condition && value is not string and not int and not bool || !condition  // Compliant, there's 'or' in the 'and' chain
            ? value
            : null;

        _ = !condition && !condition && value is not string and not int or not bool && !condition   // Compliant, there's 'or' in the 'and' chain
            ? value
            : null;

        _ = !condition || !condition || value is not string or not int or not bool && !condition    // Compliant, there's 'and' in the 'or' chain
            ? value
            : null;

        _ = !condition || !condition || value is not string or not int and not bool || !condition   // Compliant, there's 'and' in the 'or' chain
            ? value
            : null;
    }
}
