using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarAnalyzer.Test.TestCases;

struct MyStruct
{
    public bool x;
    public bool y = false;

    public MyStruct()
    {
        x = false;
    }
}

class AClass
{
    public static void DoSomething()
    {
        MyStruct myStruct = new();
        if (myStruct.x) { } // FN
        else if (myStruct.y) { } // FN
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/7057
public class Repro_7057
{
    private (string, int) SomeTuple() => ("hello", 1);
    private string SomeString() => "hello";

    public void WithTuple()
    {
        string text1 = null;
        (text1, var (text2, _)) = (SomeString(), SomeTuple());
        if (text1 == null) // Compliant
        {
            Console.WriteLine();
        }
        if (text2 == null) // Compliant
        {
            Console.WriteLine();
        }

        string text3 = null;
        ((text3, _), var (text4, _)) = ((null, 42), ("hello", 42));
        if (text3 == null)          // Noncompliant
        {
            Console.WriteLine();
        }
        if (text4 == null)          // Noncompliant
        {
            Console.WriteLine();    // Secondary
        }
    }
}
