using System.IO;
using System.Reflection;
using System.Resources;
using System.Globalization;

namespace Tests.Diagnostics
{
    static class RuntimeInformation
    {
        public static bool IsWindows() { return true; }
    }

    class Program
    {
        public static int i;
        static string s;

        static Program() // Noncompliant
//             ^^^^^^^
        {
            i = 3;
            ResourceManager sm = new ResourceManager("strings", Assembly.GetExecutingAssembly());
            s = sm.GetString("mystring");
            sm = null;
        }
    }

    class Foo
    {
        static Foo()
        {
            System.Console.WriteLine("test");
        }
    }

    class Bar
    {
        static Bar()
        {
            Program.i = 42;
        }
    }

    static class Class1
    {
        public static readonly Foo foo1 = new Foo();
        public static readonly Foo foo2 = new Foo();

        public static readonly Foo someFoo;

        static Class1() // Noncompliant
        {
            if (RuntimeInformation.IsWindows())
                someFoo = foo1;
            else
                someFoo = foo2;
        }
    }

    static class ExpressionBodyCtor
    {
        private static readonly bool IsAvailable;

        static ExpressionBodyCtor() => IsAvailable = Initialize(); // Noncompliant

        static bool Initialize() => true;
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/4832
    public class ReproIfSingle
    {
        private static readonly string Key;

        static ReproIfSingle()  // Noncompliant
        {
            if (RuntimeInformation.IsWindows())
            {
                Key = "Key A";
            }
            else
            {
                Key = "Key B";
            }
        }
    }

    public class ReproIfMulti
    {
        private static readonly string Key;
        private static readonly string Value;

        static ReproIfMulti()  // Compliant, because there are multiple variables assigned conditionally. Not easy to inline.
        {
            if (RuntimeInformation.IsWindows())
            {
                Key = "Key A";
                Value = "Value A";
            }
            else
            {
                Key = "Key B";
                Value = "Value B";
            }
        }
    }

    public class ReproIfMultiReducable
    {
        private static readonly string Key;
        private static readonly string Value;

        static ReproIfMultiReducable()  // FN
        {
            if (RuntimeInformation.IsWindows())
            {
                Key = "Key A";
            }
            else
            {
                Key = "Key B";
            }
            Value = "Value";
        }
    }

    public class ReproSwitchSingle
    {
        private static readonly string Key;

        static ReproSwitchSingle()    // Noncompliant
        {
            switch (RuntimeInformation.IsWindows())
            {
                case true:
                    Key = "Key A";
                    break;
                default:
                    Key = "Key B";
                    break;
            }
        }
    }

    public class ReproSwitchMulti
    {
        private static readonly string Key;
        private static readonly string Value;

        static ReproSwitchMulti()    // Compliant, because there are multiple variable assigned conditionally. Not easy to inline.
        {
            switch (RuntimeInformation.IsWindows())
            {
                case true:
                    Key = "Key A";
                    Value = "Value A";
                    break;
                default:
                    Key = "Key B";
                    Value = "Value B";
                    break;
            }
        }
    }

    public static class TestUtil
    {
        // https://github.com/SonarSource/sonar-dotnet/issues/6343
        static TestUtil() // Compliant
        {
            if (!Directory.Exists(""))
            {
                Directory.CreateDirectory("");
            }
        }
    }

    // https://sonarsource.atlassian.net/browse/NET-184
    static class Repro_184
    {
        private static readonly bool IsAvailable;

        static Repro_184() // Noncompliant FP
        {
            IsAvailable = true;
            System.Console.Write("side effect");
        }
    }
}
