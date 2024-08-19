using System;

class MethodsComplexity
{
    void ListPattern() // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
    {
        object[] numbers = { 1, 2, 3 };

        if (numbers is [_, > 3, 3])
//      ^^ Secondary {{+1}}
        {
            Console.WriteLine("Test");
        }
    }
}

public interface IStaticVirtualMembersInInterfaces
{
    static virtual void Method(string firstParam, string secondParam) // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
    {
        if (firstParam == secondParam)
//      ^^ Secondary {{+1}}
        {
            Console.WriteLine(firstParam.ToString());
        }
    }
}
