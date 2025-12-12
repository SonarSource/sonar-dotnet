using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

record Record
{
    void Method()
    {
        string unusedString = string.Empty; // Noncompliant
    }
}

class PrimaryConstructorParameterNotUsed(int someInt) { } // Compliant

class PrimaryConstructorParameterUsedInMethod(int someInt) // Compliant
{
    int Method => someInt;
}

public class StaticLocalFunctions
{
    public void Method()
    {
        static void Bar(List<int> list)
        {
            list.Select(item => 1);

            list.Select(item =>
            {
                var value = 1; // Noncompliant
                return item;
            });
        }
    }
}

//https://github.com/SonarSource/sonar-dotnet/issues/3137
public class Repro_3137
{
    public void GoGoGo(Logger log)
    {
        using var scope = log.BeginScope("Abc"); // Compliant, existence of variable represents a state until it's disposed
        using var _ = log.BeginScope("XXX"); // Underscore is a variable in this case, it's not a discard pattern

        // Locked file represents a state until it's disposed
        using var stream = File.Create("path");
    }

    public class Logger
    {
        public IDisposable BeginScope(string scope)
        {
            return null;
        }
    }
}

public class RecursivePatterns
{
    public void TestMethod()
    {
        var x = "hello";
        var tuple = ("hello", "bye");
        if (tuple is (string strX, string strY)) // Noncompliant
//                                        ^^^^
        {
            strX += ":)";
        }

        if (tuple is (string comX, string comY)) // Compliant
        {
            comX += comY;
        }

        if (x is string { Length: 5 } rec) //Noncompliant
//                                    ^^^
        {
            x += ":)";
        }
        if (x is string { Length: 5 } com) //Compliant
        {
            com += ":)";
        }

        if (tuple is (string str, { } d)) // Noncompliant
//                                    ^
        {
            str += ":)";
        }

        if (tuple is (string y, { } z)) // Noncompliant
//                           ^
        {
            z += ":)";
        }

        if (tuple is (string strCom, { } dCom)) // Compliant
        {
            strCom += dCom;
        }
    }
}

public class SomeClass
{
    public void SomeMethod(object[] byteArray)
    {
        if (byteArray is [1, 2, 3] unusedVar) { }           // Noncompliant
        if (byteArray is [SomeClass unusdeVar2, 42]) { }    // Noncompliant
    }
}

public record struct S
{
    public S()
    {
        (var i, var j) = (0, 0); // Noncompliant [issue1, issue2]
        (var ii, (var jj, var kk)) = (0, (0, 0)); // Noncompliant [issue3, issue4, issue5]

        var (v, p) = (0, 0); // Noncompliant [issue6, issue7]
        var (vv, (pp, vvv)) = (0, (0, 0)); /// Noncompliant [issue8, issue9, issue10]

        (int, int) x = (0, 0); // Noncompliant
//                 ^
        (int, (int, int)) xx = (0, (0, 0)); // Noncompliant
//                        ^^
        (int, int) y = (0, 0);
        y.Item1 = 2;

        var z = (0, 0); // Noncompliant

        var a = 0;
        (a, var b) = (0, 0); // Noncompliant {{Remove the unused local variable 'b'.}}
//              ^
        (a, (int, int) bb) = (0, (0, 0)); // Noncompliant
//                     ^^
        (a, var k) = (0, 0);
        (a, k) = (0, 0);

        (a, (k, var c)) = (0, (0, 0)); // Noncompliant
//                  ^
        (a, k) = (0, 0);

        (int, int) used;
        used.Item1 = 3;

        (int, (int, int)) usedNested;
        usedNested.Item1 = 3;

        (int, int) notUsed; // Noncompliant
//                 ^^^^^^^
        (int, (int, int)) notUsedNested; // Noncompliant
//                        ^^^^^^^^^^^^^
    }
}

public class NullConditionalAssingment
{
    public class Sample
    {
        public int Value { get; set; }
    }

    public void Test()
    {
        var rhs = 0;
        var compliant = new Sample();
        compliant?.Value = rhs;         // Compliant
    }
}

public class FieldKeyword
{
    public int Value
    {
        get
        {
            int compliant = 0;
            field += compliant;         // Compliant
            return field;
        }
        set
        {
            int compliant;
            compliant = field;          // Compliant
        }
    }
}
