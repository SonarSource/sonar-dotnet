using System;

public class Sample
{
    public void WhileTrueBreak()
    {
        while (true)
        {
            var beforeBreak = "BeforeBreak";
            break;
            var afterBreak = "AfterBreak";
        }
    }

    public void WhileTrueReturn()
    {
        while (true)
        {
            var beforeReturn = "BeforeReturn";
            return;
            var afterReturn = "afterReturn";
        }
    }

    public void WhileAndOr(bool a, bool b, bool c)
    {
        while (a && (b || c))
        {
            a = false;
        }
    }

    public void DoWhile()
    {
        string value = null;
        do
        {
            value = "Value";
        } while (value == null);
    }

    public void DoContinue()
    {
        var i = 0;
        do
        {
            i++;
            if (i < 5)
            {
                continue;
            }
            i += 10;
        } while (i < 10);
    }

    public void For()
    {
        for(var i = 0; i < 10; i++)
        {
            var value = "Value";
        }
    }

    public void ForEach()
    {
        foreach(var i in new[] { 0, 1, 2, 4, 8, 16 })
        {
            var value = i.ToString();
        }
    }

}
