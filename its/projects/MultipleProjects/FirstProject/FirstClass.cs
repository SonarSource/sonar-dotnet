using System;

// FIXME: This should raise S1134
// ToDo:  This should raise S1135
namespace FirstProject;

public class FirstClass
{
    public int Value { get; }

    public void FirstMethod()
    {
        // Empty
    }

    public void SecondMethod(bool condition, bool flag)
    {
        if (condition)
        {
            Console.WriteLine("Add some complexity to this method" + (flag ? "!" : "?"));
        }
        else
        {
            Console.WriteLine("Else");
        }
    }
}
