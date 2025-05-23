﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.TestCases
{
    public interface IWithDefaultImplementation
    {
        decimal Count { get; set; }
        decimal Price { get; set; }

        void Reset(int a);     //Compliant

        //Default interface methods
        decimal Total()
        {
            return Count * Price;
        }

        decimal Total(decimal Discount)
        {
            return Count * Price * (1 - Discount);
        }

        decimal Total(string unused)    // Compliant, because it's interface member
        {
            return Count * Price;
        }
    }

    public class StaticLocalFunctions
    {
        public int DoSomething(int a)   // Compliant
        {
            static int LocalFunction(int x, int seed) => x + seed;
            static int BadIncA(int x, int seed) => x + 1;   //Noncompliant
            static int BadIncB(int x, int seed)             //Noncompliant
            {
                seed = 1;
                return x + seed;
            }
            static int BadIncRecursive(int x, int seed)     //Noncompliant
            {
                seed = 1;
                if (x > 1)
                {
                    return BadIncRecursive(x - 1, seed);
                }
                return x + seed;
            }

            return LocalFunction(a, 42) + BadIncA(a, 42) + BadIncB(a, 42) + BadIncRecursive(a, 42);
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/4377
        private static bool Foo(IEnumerable<int> a, int b)
        {
            bool InsideFoo(int x) => x.Equals(b);
            bool CallInsideFoo(IEnumerable<int> numbers) => numbers.Any(x => false | InsideFoo(x));

            return CallInsideFoo(a);
        }

        public void Method()
        {
            void WithMultipleParameters(int a,
                                        int b, // Noncompliant
                                        int c,
                                        int d) // Noncompliant
            {
                var result = a + c;
            }

            static void WithMultipleParametersStatic(int a,
                                                     int b, // Noncompliant
                                                     int c,
                                                     int d) // Noncompliant
            {
                var result = a + c;
            }
        }

        // See: https://github.com/SonarSource/sonar-dotnet/issues/3803
        private void AddValue(uint id1, uint id2, string value)
        {
            var x = new Dictionary<(uint, uint), string>();
            x[(id1, id2)] = value;
        }

        public void UsedInCatch(int arg, int usedInLocalFunctionArg)
        {
            try
            {
                StaticLocalFunction(42, 42, 42);
            }
            catch
            {
                arg.ToString();
            }

            static async void StaticLocalFunction(int staticLocalArg, int staticLocalArgWithWhen, int staticLocalArgInWhen)
            {
                try
                {
                }
                catch when (staticLocalArgInWhen == 0)
                {
                    staticLocalArgWithWhen.ToString();
                }
                catch
                {
                    staticLocalArg.ToString();
                }
            }
        }
    }

    public class SwitchExpressions
    {
        public int DoSomething(int a, bool b)
        {
            return b switch
            {
                true => a,
                _ => 0
            };
        }
    }

    public class UsageInRange
    {
        public void DoSomething(int a, int b)
        {
            var list = new string[] { "a", "b", "c" };
            var sublist = list[a..];
            System.Range r = ..^b;
            sublist = list[r];
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3255
    public class Repro_3255
    {
        private string UsedInTuple(string value)
        {
            var x = (value, 7);
            return x.value;
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/4704
    public static class Repro_4704
    {
        private static void ConfigureAndValidateSettings(this int someNumber, string someString)    // Compliant, captured in generic local method
        {
            PrintSomeSum<int>();

            void PrintSomeSum<TOptions>() where TOptions : struct
            {
                Console.WriteLine(someNumber + someString.Length);
            }
        }

        private static void NotInvoked(string someString)    // Noncompliant
        {
            var somethingWithGenericName = new System.Lazy<object>(() => null);
            Undefined<int>(); // Error [CS0103] The name 'Undefined' does not exist in the current context

            void PrintSomeSum<TOptions>() where TOptions : struct
            {
                Console.WriteLine(someString.Length);
            }
        }

        private static void UsedByReferenceWithStruct(this int someNumber, string someString)
        {
            Action x = PrintSomeSum<int>;

            x();

            void PrintSomeSum<TOptions>() where TOptions : struct
            {
                Console.WriteLine(someNumber + someString.Length);
            }
        }

        private static void UsedByReferenceAsArgument(int[] list, string arg)
        {
            list.Where(LocalFunction<int>);

            bool LocalFunction<TOptions>(TOptions x) where TOptions : struct
            {
                Console.WriteLine(arg);
                return true;
            }
        }

        private static void InsideNameOf_Valid(string arg)   // Noncompliant
        {
            var name = nameof(LocalFunction);

            void LocalFunction<TOptions>() where TOptions : struct
            {
                Console.WriteLine(arg);
            }
        }

        private static void InsideNameOf_Invalid(string arg)   // Noncompliant
        {
            var name = nameof(LocalFunction<int>);      // Error [CS8084] Type parameters are not allowed on a method group as an argument to 'nameof'

            void LocalFunction<TOptions>() where TOptions : struct
            {
                Console.WriteLine(arg);
            }
        }
    }

    // https://github.com/dotnet/roslyn/issues/56644
    public class RoslynIssue_56644
    {
        private char[] invalidCharacters;

        private bool IsValidViewName(string viewName)    // Compliant, this works as expected under .NET build, but doesn't work under .NET Framework
        {
            return !this.invalidCharacters.Any(viewName.Contains);
        }
    }

    static class Repro5145
    {
        public static void Print(string[] args)
        {
            var dict = new Dictionary<string, int>();
            Console.WriteLine(GetSegmentSortKey(dict));
        }

        private static int GetSegmentSortKey(IDictionary<string, int> nodes) // Noncompliant - FP, See: https://github.com/SonarSource/sonar-dotnet/issues/5145
        {
            return Run(nodes.TryGetValueOrNull);
        }

        private static int Run(Func<string, int> tryGetValueOrNull)
        {
            return tryGetValueOrNull("test");
        }
    }

    public static class DictionaryExtensions
    {
        public static TValue TryGetValueOrNull<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : struct => default(TValue);
    }

}
