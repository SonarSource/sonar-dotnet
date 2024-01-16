do
{
    try
    {
        while (true)
        {
            if (true) { } // Noncompliant {{Refactor this code to not nest more than 3 control flow statements.}}
        }
    }
    catch { }
} while (true);

do
{
    try
    {
        if (true) { }
    }
    catch { }
} while (true);

void TopLevelLocalFunction()
{
    do
    {
        try
        {
            while (true)
            {
                if (true) { } // Noncompliant {{Refactor this code to not nest more than 3 control flow statements.}}
            }
        }
        catch { }
    } while (true);
}

public record FunctionNestingDepth
{
    public void M1()
    {
        do
        {
            while (true)
            {
                if (true) { }
            }
        } while (true);

        do
        {
            try
            {
                while (true)
                {
                    if (true) { } // Noncompliant {{Refactor this code to not nest more than 3 control flow statements.}}
                }
            }
            catch { }
        } while (true);
    }

    public int Property
    {
        get => 42;
        init
        {
            if (true)
            {
                if (true)
                {
                    if (true)
                    {
                        if (true) { } // Noncompliant
                    }
                }
            }
        }
    }
}
