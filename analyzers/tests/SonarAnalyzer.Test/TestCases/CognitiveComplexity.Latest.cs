// Noncompliant {{Refactor this top-level file to reduce its Cognitive Complexity from 9 to the 0 allowed.}}
using System;
using System.Collections.Generic;

if (1 == 2) // Secondary  {{+1}}
{

}
else if (1 == 3) // Secondary  {{+1}}
{

}
else // Secondary  {{+1}}
{
}

void LocalFunction()
{
    if (true) // Secondary  {{+2 (incl 1 for nesting)}}
    {
        return;
    }
}

// Static local functions are excluded from the complexity computation of method
// that they are nested in. They have their own complexity score as independent methods.
// See issue https://github.com/SonarSource/sonar-dotnet/issues/5625
static void StaticLocalFunction(int x) // Noncompliant {{Refactor this static local function to reduce its Cognitive Complexity from 4 to the 0 allowed.}}
{
    if (x == 1) // Secondary  {{+2 (incl 1 for nesting)}}
    {
        Console.WriteLine(x);
    }
    else if (x == 2) // Secondary  {{+1}}
    {
        Console.WriteLine(x);
    }
    else // Secondary  {{+1}}
    {
        return;
    }
}

string SwitchExpressionPatterns(object o) =>
    o switch // we should propertly count the complexity of the inner patterns
             // Secondary@-1  {{+2 (incl 1 for nesting)}}
    {
        string s => s.ToString(),
        float => "float",
        < 400 and > 30 => "between 30 and 400",  // Secondary  {{+1}}
        0 or 1 => "0 or 1",  // Secondary  {{+1}}
        null => "null",
        not null => "not null",
    };

record MyRecord
{
    string aField;
    string IfInProperty
    {
        init // Noncompliant {{Refactor this accessor to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        {
            if (true)
//          ^^ Secondary {{+1}}
            {
                aField = "";
            }
        }
    }

    bool Prop => aField == null || aField is { Length: 5 }; // Noncompliant {{Refactor this property to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
                                                          //                                                           Secondary@-1 {{+1}}
}

class AndOrConditionsComplexity
{
    bool SimpleNot(object o) => // should be 0
        o is not string;

    void And(int o) // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
    {
        if (o is > 0 and <= 10)
//      ^^ Secondary {{+1}}
//                   ^^^ Secondary@-1 {{+1}}
            Console.WriteLine("More than 0 but less than or equal to 10");
    }

    bool AndOr(int o) => // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
        o is < 500 and > 300 or 1;
    //             ^^^ Secondary {{+1}}
    //                       ^^ Secondary@-1 {{+1}}

    bool ChainedConditions(int number) => // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        number is 1 or 2 or 4 or 5;
    //              ^^ Secondary {{+1}}

    bool ChainedConditionsWithParentheses(int number) => // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        number is 1 or 2 or 4 or (5 or 6);
    //              ^^ Secondary {{+1}}

    bool ChainedSimilarConditionsWithParentheses(int one, int two) => // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
        (one is 1 or 2 or 4 or 5) || (two is 3 or 5);
    //            ^^ Secondary {{+1}}
    //                            ^^ Secondary@-1 {{+1}}
    //                                         ^^ Secondary@-2 {{+1}}

    bool ChainedDifferentConditionsWithParentheses(int one, int? two) => // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
        (one is 1 or 2 or 4 or 5) || (two is not null and 5);
    //            ^^ Secondary {{+1}}
    //                            ^^ Secondary@-1 {{+1}}
    //                                                ^^^ Secondary@-2 {{+1}}

    bool AndOrNot(object o) => // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
        o is < 500 and > 300 or not (string or double);
    //             ^^^ Secondary {{+1}}
    //                       ^^ Secondary@-1 {{+1}}
    //                                      ^^ Secondary@-2 {{+1}}
}

class StaticLambda
{
    private Action<int> act = // Noncompliant {{Refactor this field to reduce its Cognitive Complexity from 2 to the 0 allowed.}}
        static x =>
        {
            if (x > 0)
//          ^^ Secondary {{+2 (incl 1 for nesting)}}
            {

            }
        };
}

abstract class Fruit { public List<int> Prop; }
class Apple : Fruit { }
class Orange : Fruit { }

class TestProgram
{
    static void TargetTypedConditional(bool isTrue, Apple[] apples, Fruit[] givenFruits) // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
    {
        Fruit[] fruits1 = isTrue ? new Apple[1] : new Orange[1];
        //                       ^ Secondary {{+1}}
        Fruit[] fruits2 = apples ?? givenFruits;
        Fruit[] fruits3 = givenFruits ?? apples;
    }
}

record Record
{
    Fruit aField;
    bool Prop => aField == null || aField is { Prop.Count: 5 }; // Noncompliant {{Refactor this property to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
    //                                                           Secondary@-1 {{+1}}

    public void SomeMethod()
    {
        int x;

        (x, var y) = (16, 23);

        var (a, b) = (16, 23);

        var j = (a, b) = (23, 16);
    }
}

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

class LocalFunctionTest
{
    void Foo(int x) // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 4 to the 0 allowed.}}
    {
        if (x == 0) // Secondary  {{+1}}
        {
            return;
        }

        void LocalFunction(int x)
        {
            if (x == 0) // Secondary
            {
                Console.WriteLine(x);
            }
            else // Secondary
            {
                return;
            }
        }

        // Static local functions are excluded from the complexity computation of method
        // that they are nested in. They have their own complexity score as independent methods.
        // See issue https://github.com/SonarSource/sonar-dotnet/issues/5625
        static void StaticLocalFunction(int x) // Noncompliant  {{Refactor this static local function to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
        {
            if (x == 0) // Secondary {{+2 (incl 1 for nesting)}}
            {
                Console.WriteLine(x);
            }
            else // Secondary {{+1}}
            {
                return;
            }
        }
    }
}

public partial class PartialProperty
{
    public partial int Property { get; set; }
}

public static class Extensions
{
    extension(Extensions)
    {
        public static int Prop
        {
            get =>              // Noncompliant  {{Refactor this accessor to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
                true ? 0 : 1;   // Secondary
            set { }
        }

        public static bool Where() { return true ? true : false; } // Noncompliant
                                                                   // Secondary@-1
    }
}

public class FieldKeyWord
{
    public int Prop
    {
        get =>                  // Noncompliant  {{Refactor this accessor to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
            field == 1 ? 1 : 0; // Secondary
    }
}

public partial class PartialConstructor
{
    public partial PartialConstructor();
}

public class NullConditionalAssignment
{
    public class FieldClass
    {
        public int value;
    }
    public FieldClass Prop
    {
        get => new();
        set =>                                           // Noncompliant  {{Refactor this accessor to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
            true ? field?.value = 10 : field?.value = 0; // Secondary
                                                         // Error@-1 [CS0201]
    }

}
