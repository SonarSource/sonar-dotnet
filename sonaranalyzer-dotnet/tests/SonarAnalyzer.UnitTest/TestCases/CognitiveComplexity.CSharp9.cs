using System;

// FN for top-level statements
if (1 == 2)
{

}
else if (1 == 3)
{

}
else
{
}

void LocalFunction() // FN
{
    if (true) // +1
    {
        return;
    }
}

string SwitchExpressionPatterns(object o) =>
    o switch // we should propertly count the complexity of the inner patterns
    {
        string s => s.ToString(),
        float => "float",
        < 400 and > 30 => "between 30 and 400",
        0 or 1 => "0 or 1",
        null => "null",
        not null => "not null",
    };

record MyRecord
{
    string field;
    string IfInProperty
    {
        init // FN
        {
            if (true)
            {
                field = "";
            }
        }
    }
}

class AndOrConditionsComplexity
{
    bool SimpleNot(object o) => // should be 0
        o is not string;

    void And(object o) // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
    {
        // FN, inner AND boolean expression should count as + 1, just like it would be '&&'
        if (o is > 0 and <= 10)
//      ^^ Secondary {{+1}}
            Console.WriteLine("More than 0 but less than or equal to 10");
    }

    bool AndOr(int o) => // should be 2
        o is < 500 and > 300 or 1;

    bool ChainedConditions(int number) => // should be 1
        number is 1 or 2 or 4 or 5;

    // Below, the fact that we are using both 'or' and '||' should increase the cognitive complexity
    bool ChainedSimilarConditionsWithParentheses(int one, int two) => // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        (one is 1 or 2 or 4 or 5) || (two is 3 or 5);
//                                ^^ Secondary {{+1}}

    // FN below
    bool ChainedDifferentConditionsWithParentheses(int one, int? two) => // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
        (one is 1 or 2 or 4 or 5) || (two is not null and 5);
//                                ^^ Secondary {{+1}}

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

abstract class Fruit { }
class Apple : Fruit { }
class Orange : Fruit { }

class Program
{
    static void TargetTypedConditional(bool isTrue, Apple[] apples, Fruit[] givenFruits) // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
    {
        Fruit[] fruits1 = isTrue ? new Apple[1] : new Orange[1];
//                               ^ Secondary {{+1}}
        Fruit[] fruits2 = apples ?? givenFruits;
        Fruit[] fruits3 = givenFruits ?? apples;
    }
}

// See https://github.com/dotnet/roslyn/issues/45510
namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}
