using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace Tests.Diagnostics
{
    public class Resource : IDisposable
    {
        public void Dispose()
        {
        }
        public int DoSomething()
        {
            return 1;
        }
        public int DoSomethingElse()
        {
            return 5;
        }

        public void IgnoredValues()
        {
            var stringEmpty = string.Empty; // Compliant
            stringEmpty = "other";

            string stringNull = null; // Compliant
            stringNull = "other";

            var boolFalse = false; // Compliant
            boolFalse = true;

            var boolTrue = true; // Compliant
            boolTrue = false;

            object objectNull = null; // Compliant
            objectNull = new object();

            var intZero = 0; // Compliant
            intZero = 42;

            var intOne = 1; // Compliant
            intOne = 42;

            var intMinusOne = -1; // Compliant
            intMinusOne = 42;

            var intPlusOne = +1; // Compliant
            intPlusOne = 42;

            var emptyStringLiteral = ""; // Compliant
            emptyStringLiteral = "other";

            var emptyInterpolatedStringLiteral = $""; // Compliant
            emptyInterpolatedStringLiteral = "other";

            // Variables should be used in order the rule to trigger
            Console.WriteLine("", emptyStringLiteral, emptyInterpolatedStringLiteral, stringEmpty, stringNull, boolFalse, boolTrue,
                objectNull, intZero, intOne, intMinusOne, intPlusOne);
        }

        public void ExpressionResultsInConstantIgnoredValue()
        {
            var boolFalse = 0 != 0;     // Noncompliant, only explicit 'false' is ignored by the rule
            boolFalse = true;

            var boolTrue = 0 == 0;      // Noncompliant
            boolTrue = false;

            var intZero = 1 - 1;        // Noncompliant
            intZero = 42;

            var intOne = 0 + 1;         // Noncompliant
            intOne = 42;

            var intMinusOne = 0 - 1;    // Noncompliant
            intMinusOne = 42;

            // Variables should be used in order the rule to trigger
            Console.WriteLine("", boolFalse, boolTrue, intZero, intOne, intMinusOne);
        }

        public void Defaults()
        {
            var s = default(string); // Compliant
            s = "";

            var b = default(bool); // Compliant
            b = true;

            var o = default(object); // Compliant
            o = new object();

            var i = default(int); // Compliant
            i = 42;

            // Variables should be used in order the rule to trigger
            Console.WriteLine("", s, b, o, i);
        }
    }

    public class DeadStores
    {
        public int Property { get; set; }

        int doSomething() => 1;
        int doSomethingElse() => 1;

        void calculateRate(int a, int b)
        {
            b = doSomething(); // Noncompliant {{Remove this useless assignment to local variable 'b'.}}
//            ^^^^^^^^^^^^^^^

            int i, j;
            i = a + 12;
            i += i + 2; // Noncompliant
            i = 5;
            j = i;
            i
                = doSomething(); // Noncompliant; retrieved value overwritten in for loop
            for (i = 0; i < j + 10; i++)
            {
                //  ...
            }

            if ((i = doSomething()) == 5 ||
                (i = doSomethingElse()) == 5)
            {
                i += 5; // Noncompliant
            }

            var resource = new Resource(); // Noncompliant; retrieved value not used
            using (resource = new Resource())
            {
                resource.DoSomething();
            }

            var x
                = 10; // Noncompliant
            var y =
                x = 11; // Noncompliant
            Console.WriteLine(y);

            int k = 12; // Noncompliant
            X(out k);   // Compliant, not reporting on out parameters
        }
        void X(out int i) { i = 10; }

        void calculateRate2(int a, int b)
        {
            var x = 0;
            x = 1; // Noncompliant
            try
            {
                x = 11; // Noncompliant
                x = 12;
                Console.Write(x);
                x = 13; // Noncompliant
            }
            catch (Exception)
            {
                x = 21; // Noncompliant
                x = 22;
                Console.Write(x);
                x = 23; // Noncompliant
            }
            x = 31; // Noncompliant
        }

        void storeI(int i) { }

        void calculateRate3(int a, int b)
        {
            int i;

            i = doSomething();
            i += a + b;
            storeI(i);

            for (i = 0; i < 10; i++)
            {
                //  ...
            }
        }

        int pow(int a, int b)
        {
            if (b == 0)
            {
                return 0;
            }
            int x = a;
            for (int i = 1; i < b; i++)
            {
                x = x * a;  //Not detected yet, we are in a loop, Dead store because the last return statement should return x instead of returning a
            }
            return a;
        }

        public void Assignment(DeadStores sender)
        {
            // Compliant cases
            sender.Property += 42;
            sender.Property = 42;
            Property += 42;
            Property = 42;
            undefined += 42;    // Error [CS0103]: The name 'undefined' does not exist in the current context
            undefined = 42;     // Error [CS0103]: The name 'undefined' does not exist in the current context

            var captured = 10;
            Action a = () => { Console.WriteLine(captured); };
            captured += 11;     // Not reporting on captured local variables

            var add = 40;
            add += 2;
            Use(add);
            add += 100;     // Noncompliant

            var sub = 40;
            sub -= 2;
            Use(sub);
            sub -= 100;     // Noncompliant

            var mul = 40;
            mul *= 2;
            Use(mul);
            mul *= 100;     // Noncompliant

            var div = 40;
            div /= 2;
            Use(div);
            div /= 100;     // Noncompliant

            var mod = 40;
            mod += 2;
            Use(mod);
            mod %= 100;     // Noncompliant

            var and = false;
            and &= true;
            Use(and);
            and &= false;   // Noncompliant

            var or = false;
            or |= false;
            Use(or);
            or |= true;     // Noncompliant

            var xor = 40;
            xor ^= 2;
            Use(xor);
            xor ^= 100;     // Noncompliant

            var left = 40;
            left <<= 2;
            Use(left);
            left <<= 100;   // Noncompliant

            var right = 40;
            right >>= 2;
            Use(right);
            right >>= 100;  // Noncompliant

            string coa = SomeString();
            coa ??= SomeString();
            Use(coa);
            coa ??= SomeString();  // Noncompliant

        }

        private void Use(int arg) { }
        private void Use(bool arg) { }
        private void Use(string arg) { }
        private string SomeString() => null;

        public void Switch()
        {
            const int c = 5; // Compliant
            var b = 5;
            switch (b)
            {
                case 6:
                    b = 5; // Noncompliant
                    break;
                case 7:
                    b = 56; // Noncompliant
                    break;
                case c:
                    break;
            }

            b = 7;
            Console.Write(b);
            b += 7; // Noncompliant
        }

        public int Switch1(int x)
        {
            var b = 0; // Compliant
            switch (x)
            {
                case 6:
                    b = 5;
                    break;
                case 7:
                    b = 56;
                    break;
                default:
                    b = 0;
                    break;
            }

            return b;
        }

        public int Switch2(int x)
        {
            var b = 0; // Compliant
            switch (x)
            {
                case 6:
                    b = 5;
                    break;
                case 7:
                    b = 56;
                    break;
            }

            return b;
        }

        private int MyProp
        {
            get
            {
                var i = 10;
                Console.WriteLine(i);
                i++; // Noncompliant
                i = 12;
                ++i; // Noncompliant
                i = 12;
                var a = ++i;
                return a;
            }
        }

        private int MyProp2
        {
            get
            {
                var i = 10; // Noncompliant
                if (nameof(((i))) == "i") // Error [CS8081]
                {
                    i = 11;
                }
                else
                {
                    i = 12;
                }
                Console.WriteLine(i);

                return 42;
            }
        }

        public List<int> Method(int i)
        {
            var l = new List<int>();

            Func<List<int>> func = () =>
            {
                return (l = new List<int>(new[] { i }));
            };

            var x = l; // Noncompliant
            x = null;  // Noncompliant

            return func();
        }

        public List<int> Method2(int i)
        {
            var l = new List<int>(); // Compliant, not reporting on captured variables

            return (() => // Error [CS0149] - no method name
            {
                var k = 10; // Noncompliant
                k = 12; // Noncompliant
                return (l = new List<int>(new[] { i })); // l captured here
            })();
        }

        public List<int> Method3(int i)
        {
            bool f = false;
            if (true || (f = false))
            {
                if (f) { }
            }

            return null;
        }

        public List<int> Method4(int i)
        {
            bool f;
            f = true;
            if (true || (f = false))
            {
                if (f) { }
            }

            return null;
        }

        public List<int> Method5(out int i, ref int j)
        {
            i = 10; // Compliant, out parameter

            j = 11;
            if (j == 11)
            {
                j = 12; // Compliant, ref parameter
            }

            return null;
        }
        public void Method5Call1(out int i)
        {
            int x = 10;
            Method5(out i, ref x);
        }

        public void Method5Call2()
        {
            int x;
            Method5Call1(out x); // Compliant, reporting on this can be considered false positive, although it's not.
        }

        public List<int> Method6()
        {
            var i = 10;
            Action a = () => { Console.WriteLine(i); };
            i = 11;     // Not reporting on captured local variables
            a();

            return null;
        }

        public List<int> Method7_Foreach()
        {
            foreach (var item in new int[0])
            {
                Console.WriteLine(item);
            }

            foreach (var
                item // A new value is assigned here, which is not used. But we are not reporting on it.
                in new int[0])
            {
            }

            return null;
        }

        public void Unused()
        {
            var x = 5; // Compliant, S1481 already reports on it.

            var y = 5; // Noncompliant
            y = 6; // Noncompliant
        }

        private void SimpleAssignment(bool b1, bool b2)
        {
            var x = false;  // Compliant
            (x) = b1 && b2; // Noncompliant
            x = b1 && b2;   // Noncompliant
        }

        private class NameOfTest
        {
            private int MyProp2
            {
                get
                {
                    var i = 10; // Compliant
                    if (nameof(((i))) == "i")
                    {
                        i = 11;
                    }
                    else
                    {
                        i = 12;
                    }
                    Console.WriteLine(i);

                    return 0;
                }
            }

            private static string nameof(int i)
            {
                return "some text";
            }
        }

        private class ActorBase
        {
            public virtual int Create()
            {
                var number = 5;
                try
                {
                    return 0;
                }
                catch
                {
                    return number;
                }
            }
        }
    }

    public class CSharp8
    {

        public interface IWithDefaultImplementation
        {
            decimal Count { get; set; }
            decimal Price { get; set; }

            //Default interface methods
            decimal Total(decimal discount)
            {
                decimal bias;
                bias = 42.42M;   // Noncompliant
                bias = 0;
                var ret = bias + Count * Price * (1 - discount);
                discount = 0;   // Noncompliant
                return ret;
            }

        }

        public class StaticLocalFunctions
        {
            public int DoSomething(int a)
            {
                static int LocalFunction(int x)
                {
                    int seed;
                    seed = 1;       // Noncompliant
                    seed = 42;
                    return x + seed;
                }

                return LocalFunction(a);
            }
        }

        public class SwitchExpressions
        {
            int Compute(int a, bool isOK)
            {
                var state = a;  //Compliant, it is used inside switch expression
                state = isOK switch
                {
                    true => (state + 1) % 4,
                    false => state
                };
                return state;
            }
        }

        public class NullCoalescingAssignment
        {
            int[] Compute(int[] arr)
            {
                var lst = arr;    //Compliant
                var unused = arr;
                lst ??= new int[0];
                unused ??= new int[0]; //Noncompliant
                return lst;
            }
        }

    }

    public class AkkaSnippet
    {
        internal void OnlyFinally(object actor)
        {
            try
            {
                Foo(actor);
            }
            finally
            {
                actor = null; // Noncompliant
            }
        }

        internal void OnlyFinally_WithTryInside(object actor)
        {
            try
            {
                Foo(actor);
            }
            finally
            {
                try
                {
                    actor = null; // Noncompliant
                }
                catch
                {
                    Foo(null);
                }
            }
        }

        internal void Finally_Followed_By_Deadstore(object actor)
        {
            try
            {
                Foo(actor);
            }
            finally
            {
                actor = null;   // Noncompliant
            }
            actor = null;       // Noncompliant
        }

        internal void SnippetTwo(object actor)
        {
            try
            {
                Foo(actor);
            }
            catch (Exception)
            {
                actor = null; // Noncompliant
            }
            Foo(null);
        }

        internal void SnippetThree(object actor, int i)
        {
            try
            {
                Foo(actor);
            }
            catch (Exception) when (i == 1)
            {
                actor = null; // Noncompliant
            }
            finally
            {
                actor = null; // Noncompliant
            }
        }

        private void Foo(object obj) { }
    }

    public class ReproGithubIssue2311
    {
        private void SwitchCaseWithWhenAndLocalScope(object sender, DateTimeKind e)
        {
            var tab = new string(' ', 2);
            switch (e)
            {
                case DateTimeKind.Local:
                    {
                        Console.WriteLine(tab);
                        break;
                    }

                case DateTimeKind.Unspecified when tab == "  ":
                    {
                        break;
                    }
            }
        }

        private void SwitchCaseWithLocalScope(object sender, DateTimeKind e)
        {
            var tab = new string(' ', 2);
            switch (e)
            {
                case DateTimeKind.Local:
                    {
                        Console.WriteLine(tab);
                        break;
                    }

                case DateTimeKind.Unspecified:
                    {
                        break;
                    }
            }
        }

        private void SwitchCase(object sender, DateTimeKind e)
        {
            var tab = new string(' ', 2);
            switch (e)
            {
                case DateTimeKind.Utc:
                    Console.WriteLine(tab);
                    break;

                case DateTimeKind.Unspecified:
                    break;
            }
        }
    }

    public class ReproGithubIssue697
    {
        private bool DoStuff() => true;

        public void Foo()
        {
            bool shouldCatch = false;
            try
            {
                shouldCatch = true; // ok, is read in catch filter
                throw new InvalidOperationException("bar");
            }
            catch (Exception) when (shouldCatch)
            {
                Console.WriteLine("Error");
            }
        }

        public void Bar(bool cond)
        {
            bool shouldCatch = false;
            try
            {
                DoStuff();
                shouldCatch = true; // ok, read in catch filter
                if (cond)
                {
                    throw new InvalidOperationException("bar");
                }
            }
            catch (Exception) when (shouldCatch)
            {
                Console.WriteLine("Error");
            }
        }

    }

    public class ReproGithubIssue2393
    {
        public static void RetryOnException(int retries)
        {
            var attempts = 0;
            do
            {
                try
                {
                    attempts++;
                    DoNothing();
                    break;
                }
                catch (Exception ex)
                {
                    if (attempts > retries)
                        throw;
                }
            } while (true);
        }

        public static void ComplexRetryOnException(int retries)
        {
            var attempts = 0;
            try
            {
                do
                {
                    if (retries > 0)
                    {
                        DoNothing();
                        try
                        {
                            attempts++;
                            DoNothing();
                            break;
                        }
                        catch (ArgumentException ex)
                        {
                            DoNothing();
                        }
                    }
                } while (true);
            }
            catch (Exception ex)
            {
                if (attempts > retries)
                    throw;
                DoNothing();
            }
        }

        public void Bar()
        {
            bool isFirst = true;
            foreach (var i in System.Linq.Enumerable.Range(1, 10))
            {
                try
                {
                    DoNothing(isFirst);
                }
                finally
                {
                    isFirst = false; // is used in DoNothing, after loop
                }
            }
        }

        private static void DoNothing() { }
        private static void DoNothing(bool b) { }
    }

    public static class ReproIssues
    {
        // https://github.com/SonarSource/sonar-dotnet/issues/2596
        public static long WithConstantValue(string path)
        {
            const int unknownfilelength = -1;
            long length = unknownfilelength; // Noncompliant FP
            try
            {
                length = new System.IO.FileInfo(path).Length;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return length;
        }

        public static long WithMinus1(string path)
        {
            long length = -1;
            try
            {
                length = new System.IO.FileInfo(path).Length;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return length;
        }

        const int UNKNOWN = -1;
        public static long WithClassConstant(string path)
        {
            long length = UNKNOWN; // Noncompliant FP
            try
            {
                length = new System.IO.FileInfo(path).Length;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return length;
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/2598
        public static string WithCastedNull(string path)
        {
            var length = (string)null; // Noncompliant FP
            try
            {
                length = new System.IO.FileInfo(path).Length.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return length;
        }

        public static string WithDefault(string path)
        {
            string length = default(string);
            try
            {
                length = new System.IO.FileInfo(path).Length.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return length;
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/2600
    public class FooBarBaz
    {
        public int Start()
        {
            const int x = -1;
            int exitCode = x; // Noncompliant FP
            Exception exception = null;

            try
            {
                Archive();

                exitCode = 1;
            }
            catch (SystemException e)
            {
                exception = e; // Noncompliant
            }

            return exitCode;
        }

        public void Archive() { }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/2426
    public class ReproGithubIssue2426
    {
        public int VarPatternCheck(string value)
        {
            switch (value)
            {
                case var x when x == "one":
                    return 1;

                default:
                    return 0;
            }
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/2607
    public class Verify2607
    {
        public static void DeadStore(int[] array)
        {
            var x = 0;
            x = array[^1]; // Noncompliant
            try
            {
                x = 11; // Noncompliant
                x = 12;
                Console.Write(x);
                x = 13; // Noncompliant
            }
            catch (Exception)
            {
                x = 21; // Noncompliant
                x = 22;
                Console.Write(x);
                x = 23; // Noncompliant
            }
            x = 31; // Noncompliant
        }
    }

    // issue https://github.com/SonarSource/sonar-dotnet/issues/3094
    public class TupleReturn
    {
        public static (int foo, int bar) M(string text)
        {
            int b = int.Parse(text);
            return (1, b);
        }

        public void UnusedTuple_FalseNegative()
        {
            (int x, int y) t = (1, 2); // Compliant - FN, tuples are not yet covered: https://github.com/SonarSource/sonar-dotnet/issues/2933
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3126
    public class Issue_3126
    {
        public string VariableDeclarator_WithLocalFunction()
        {
            string buffer = "Value"; // Compliant
            return Local();

            string Local()
            {
                return buffer;
            }
        }

        public string Assignment_WithLocalFunction()
        {
            string buffer;
            buffer = "Value"; // Compliant
            return Local();

            string Local()
            {
                return buffer;
            }
        }

        public int PrefixExpression_WithLocalFunction()
        {
            var count = 0;
            ++count;        // Compliant
            return Local();

            int Local()
            {
                return count;
            }
        }

        public int PostfixExpression_WithLocalFunction()
        {
            var count = 0;
            count++;        // Compliant
            return Local();

            int Local()
            {
                return count;
            }
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3304
    public class Repro_3304
    {
        public void WithRefKeyword(int[] values)
        {
            ref int value = ref values[0];  // Compliant, because `value` keeps the reference to `values[0]`, and below `default` is actually assigned to `values[0]`
            value = default;                // Compliant, because it's ref variable and value is propagated somewhere
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3348
    public class Repro_3348
    {
        public int Indices(int[] values)
        {
            var index = 2; // Compliant, value is used in Indice
            return values[^index];
        }

        public int[] Ranges(int[] values)
        {
            var first = 2; // Compliant, value is used in Range
            var last = 22; // Compliant, value is used in Range
            return values[first..last];
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3719
    public class Repro_3719
    {
        public void UseVariableInLocalPredicate()
        {
            bool usedBool = BoolInitializer(true); // Compliant, value is used in local predicate function
            var list = new List<bool>();
            list.Where(LocalPredicate);

            bool LocalPredicate(bool input)
            {
                return usedBool && input;
            }

            bool BoolInitializer(bool value)
            {
                return value;
            }
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/2303
    public class Repro_2303
    {
        public void WithRefKeywrod()
        {
            Span<int> span = new[] { 42 };
            int j = 0;

            foreach (ref var e in span)
            {
                e = j--; // Compliant because of ref keyword
            }
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/2766
    public class Repro_2766
    {
        public int WithTryCatch()
        {
            var name = string.Empty;
            try
            {
                var values = GetValues();
                name = values.name; // Noncompliant FP, DoWork can throw

                DoWork();
                return 5;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{name}: {e}");
                throw;
            }
        }

        private (string name, int count) GetValues() => ("foo", 1);

        private void DoWork() => throw new InvalidOperationException("bang");
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/2761
    public class Repro_2761
    {
        public static void CreateDirectory(string directory)
        {
            const int CopyWaitInterval = 250;
            bool created = false;
            int attempts = 10;

            do
            {
                try
                {
                    System.IO.Directory.CreateDirectory(directory);
                    created = true;
                }
                catch (UnauthorizedAccessException)
                {
                    if (attempts == 0)
                    {
                        throw;
                    }
                }

                if (!created)   // Compliant
                {
                    --attempts;
                }
            } while (!created); // Compliant
        }
    }
}
