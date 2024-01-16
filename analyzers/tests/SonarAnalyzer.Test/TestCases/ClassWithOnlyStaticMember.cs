using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public interface IMyInterface
    {

    }

    public struct MyStruct // Compliant, we don't care about structs
    {
        public static string Concatenate(string s1, string s2)
        {
            return s1 + s2;
        }
        public static string Prop { get; set; }
    }

    public class StringUtils // Noncompliant {{Add a 'protected' constructor or the 'static' keyword to the class declaration.}}
//               ^^^^^^^^^^^
    {
        public static string Concatenate(string s1, string s2)
        {
            return s1 + s2;
        }
        public static string Prop { get; set; }
    }

    public sealed class StringUtils22 //Noncompliant
    {
        public static string Concatenate(string s1, string s2)
        {
            return s1 + s2;
        }
        public static string Prop { get; set; }
    }

    public class StringUtilsAsBase
    {
        public StringUtilsAsBase() //Noncompliant {{Hide this public constructor by making it 'protected'.}}
        { }
        public static string Concatenate(string s1, string s2)
        {
            return s1 + s2;
        }
        public static string Prop { get; set; }
    }

    public sealed class SealedStringUtilsAsBase
    {
        public SealedStringUtilsAsBase() //Noncompliant {{Hide this public constructor by making it 'private'.}}
        { }
        public static string Concatenate(string s1, string s2)
        {
            return s1 + s2;
        }
        public static string Prop { get; set; }
    }


    public class BaseClass //Compliant, has no methods at all
    { }

    public class StringUtilsDerived : BaseClass
    {
        public static string Concatenate(string s1, string s2)
        {
            return s1 + s2;
        }
        public static string Prop { get; set; }
    }

    public interface IInterface
    { }
    public class StringUtilsIf : IInterface
    {
        public static string Concatenate(string s1, string s2)
        {
            return s1 + s2;
        }
        public static string Prop { get; set; }
    }

    public static class StringUtils2
    {
        public static string Concatenate(string s1, string s2)
        {
            return s1 + s2;
        }
    }

    public class StringUtils3
    {
        protected StringUtils3()
        {
        }
        public static string Concatenate(string s1, string s2)
        {
            return s1 + s2;
        }
    }

    public class StringUtils4
    {
        public static StringUtils4 Concatenate()
        {
            return null;
        }
    }
    public class StringUtils5
    {
        public static void Concatenate(StringUtils5 p)
        {
        }
    }
    public class StringUtils6
    {
        public static StringUtils6 Prop { get; set; }
    }

    public class StringUtils7
    {
        public static StringUtils7 Field;
    }

    public abstract class AbstractClass
    {
        public static int Answer() => 42;
    }

    public class TestClass // Noncompliant {{Add a 'protected' constructor or the 'static' keyword to the class declaration.}}
    {
        public static void SomeMethod() { }
        public static string Prop { get; set; }
    }

    public class AnotherTestClass : TestClass // FN
    {

    }
}
