using System;

    { // Noncompliant
//  ^
        if (1 < 2)
        {
        }
    }

record TestRecord
{
    public int MeaningOfLife
    {
        get
        {
            { // Noncompliant
//          ^
                return 42;
            }
        }
        init
        {
            { // Noncompliant
//          ^
                throw new ArgumentOutOfRangeException("Value can only be 42.");
            }
        }
    }

    public void Run(int x, int y)
    {
        { // Noncompliant
//      ^
        }
    }
}
