using System;
using M = Math;
/// <summary>
/// <seealso cref="TTTestClass"/>
/// </summary>
public class TTTestClass
{
    // FIXME: fix this issue

    //TODO: I need to fix this
    public object MyMethod()
    {
        using (y = null)
        { }

        var x = 5;
        if (1==1)
        {
            new TTTestClass();
            return "" + x;
        }

        return 'c';
        ;
    }

    private int myVar;

    public int MyProperty
    {
        get { return myVar; }
        set { myVar = value; }
    }
}