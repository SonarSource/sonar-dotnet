using System;

namespace Tests.TestCases
{
    public partial class StaticFieldInitializerOrder
    {
        public static string s1 = new string('x', Const); // Compliant. Const do not suffer from initialization order fiasco.
        public static string s2 = new string('x', Const2);
        public static string s3 = new string('x', Y); // Noncompliant

        public static Action<int> A = (i) => { var x = i + StaticFieldInitializerOrder.Y; }; // Okay??? Might or might not. For now we don't report on it
        public static int X = Y; // Noncompliant; Y at this time is still assigned default(int), i.e. 0
//                          ^^^
        public static int[] ArrY1 = new[] { Y }; // Noncompliant
        public static int X2 = M(StaticFieldInitializerOrder.Y); // Noncompliant; Y at this time is still assigned default(int), i.e. 0
        public static int Y = 42;
        public static int Z = Y; // Okay
        public static int V = W; // Noncompliant {{Move this field's initializer into a static constructor.}}
        public static int U = Const; // Compliant
        public static int[] ArrY2 = new[] { Y }; // Y was already initialized
        public static int[] ArrC = new[] { Const };
        public const int Const = 5;

        public int nonStat = W;

        public static int moreVariables = 1, withInitializers = 2;

        public static int M(int i) { return i; }
    }

    public partial struct StaticFieldsOrderStruct
    {
        public static string s1 = new string('x', Const); // Compliant. Const do not suffer from initialization order fiasco.
        public static string s2 = new string('x', Const2);
        public static string s3 = new string('x', Y); // Noncompliant

        public static int[] ArrY1 = new[] { Y }; // Noncompliant
        public static Action<int> A = (i) => { var x = i + StaticFieldInitializerOrder.Y; }; // Okay??? Might or might not. For now we don't report on it
        public static int X = Y; // Noncompliant
        public static int X2 = M(StaticFieldInitializerOrder.Y); // Compliant - FN: Y at this time is still assigned default(int), i.e. 0
        public static int Y = 42;
        public static int Z = Y; // Okay
        public static int V = W; // Noncompliant
        public static int U = Const; // Compliant
        public static int[] ArrY2 = new[] { Y }; // Y was already initialized
        public static int[] ArrC = new[] { Const };
        public const int Const = 5;

        public static int M(int i) { return i; }
    }

    public class Derived : Base
    {
        public static int b = a; // Compliant: a gets initialized before b.
    }

    public class Base
    {
        public static int a = 1;
    }
}
