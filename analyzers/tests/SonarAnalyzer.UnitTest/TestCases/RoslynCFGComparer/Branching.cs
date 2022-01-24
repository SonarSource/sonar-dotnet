using System;
using System.Collections.Generic;

public class Sample
{
    public string Condition(bool condition)
    {
        string value = "Init";
        if (condition)
        {
            value = "True";
        }
        else
        {
            value = "False";
        }
        return value;
    }

    public string ConditionAndOr(bool a, bool b, bool c)
    {
        string value = "Init";
        if (a && ((((b || c)))))
        {
            value = "Modified";
        }
        return value;
    }

    public string ElseIf(int value)
    {
        if (value == 0)
            return "Zero";
        else if (value == 1)
            return "One";
        else if (value == 2)
            return "Two";
        else
            return "Something else";
    }

    public string SwitchStatementBreak(int value)
    {
        string ret;
        switch (value)
        {
            case 0:
                ret = "Zero";
                break;

            case 1:
                ret = "One";
                break;

            case 2:
                ret = "Two";
                break;

            default:
                ret = "Other";
                break;
        }
        return ret;
    }

    public string SwitchStatementGoto(int value)
    {
        string ret;
        switch (value)
        {
            case 0:
                goto case 1;

            case 1:
                ret = "ZeroOrOne";
                break;

            case 2:
                goto default;

            default:
                ret = "Other";
                break;
        }
        return ret;
    }

    public string SwitchStatementBreakWithWhen(int value, bool condition)
    {
        string ret;
        switch (value)
        {
            case 0 when condition:
                ret = "Zero";
                break;

            case 1:
                ret = "One";
                break;

            case 2:
                ret = "Two";
                break;

            default:
                ret = "Other";
                break;
        }
        return ret;
    }

    public string SwitchStatementBreakMultipleConditions(int value)
    {
        string ret;
        switch (value)
        {
            case 0:
            case 1:
            case 2:
                ret = "Small";
                break;

            default:
                ret = "Other";
                break;
        }
        return ret;
    }

    public string SwitchStatementBreakNoDefault(int value)
    {
        string ret = "default";
        switch (value)
        {
            case 0:
                ret = "Zero";
                break;

            case 1:
                ret = "One";
                break;

            case 2:
                ret = "Two";
                break;

        }
        return ret;
    }

    public string SwitchStatementReturn(int value)
    {
        switch (value)
        {
            case 0:
                return "Zero";

            case 1:
                return "One";

            case 2:
                return "Two";

            default:
                return "Other";
        }
    }

    public string SwitchExpression(int value) =>
        value switch
        {
            0 => "Zero",
            1 => "One",
            2 => "Two",
            _ => "Else"
        };


    public string SwitchExpressionArrow(int value)
    {
        return value switch
        {
            0 => "Zero",
            1 => "One",
            2 => "Two",
            _ => "Else"
        };
    }

    public string SwitchExpressionNoDefault(int value)
    {
        return value switch
        {
            0 => "Zero",
            1 => "One",
            2 => "Two",
        };
    }

    public void CoalesceSingle(string a, string b)
    {
        var c = a ?? b;
    }

    public void CoalesceThrow(string a, string b)
    {
        var c = a ?? b ?? throw new ArgumentNullException();
    }

    public void CoalesceMulti(string a, string b, string c, string d)
    {
        var e = a ?? b ?? c ?? d;
    }

    public void CoalescingAssignmentSingle(string a, string b)
    {
        a ??= b;
    }

    public void CoalescingAssignmentMulti(string a, string b, string c, string d)
    {
        a ??= b ??= c ??= d;
    }

    public void ConditionalAccess(object o)
    {
        var ret = o?.ToString();
    }

    public IEnumerable<string> YieldReturn()
    {
        yield return "A";
        yield return "B";
        yield return "C";
    }

    public IEnumerable<string> YieldBreak()
    {
        yield return "A";
        yield return "B";
        yield break;
        yield return "C";
    }

    public IEnumerable<string> YieldBreakCondition(bool condition)
    {
        yield return "A";
        yield return "B";
        if (condition)
        {
            yield break;
        }
        yield return "C";
    }

    int field;

    public void Fixme(bool condition)
    {
        if (condition)
            field = 42;

    }

    public void FixmeTry()
    {
        Fixme(false);
        try
        {
            field = 1;
        }
        finally
        {
            field = 42;
        }
    }

    public string Method(object a, object b) =>
        a?.ToString() + b?.ToString();
}
