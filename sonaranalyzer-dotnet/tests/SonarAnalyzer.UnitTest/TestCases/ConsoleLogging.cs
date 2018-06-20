using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tests.Diagnostics
{
    public class ConsoleLogging
    {
        private static byte[] GenerateKey(byte[] key)
        {
            GenerateKey2(key);
            Console.WriteLine("debug key = {0}", BitConverter.ToString(key)); //Noncompliant {{Remove this logging statement.}}
//          ^^^^^^^^^^^^^^^^^

            Console.WriteLine(); //Noncopmliant
//          ^^^^^^^^^^^^^^^^^

            Console.Write("debug key = {0}", BitConverter.ToString(key)); //Noncompliant
//          ^^^^^^^^^^^^^

            Console.Write(true); //Noncompliant
//          ^^^^^^^^^^^^^

            Console.ReadKey();
            System.Diagnostics.Debug.WriteLine("debug key = {0}", BitConverter.ToString(key));
            return key;
        }

        public static void LogDebug(string message) =>
            Console.WriteLine(message); // Noncompliant

        private static void NestedMethod()
        {
            var s = GetData();
            string GetData()
            {
                Console.Write(true); // Noncompliant
                return "data";
            }
        }

        private string property1;
        public string Property1
        {
            get
            {
                Console.WriteLine("Property1 read"); // Noncompliant
                return property;
            }
            set
            {
                Console.WriteLine("Property1 written"); // Noncompliant
                property = value;
            }
        }
    }

    internal class Exceptions_MethodLevelConditionals
    {
        [System.Diagnostics.Conditional("Wrong conditional")]
        public static void LogDebugA(string message)
        {
            Console.WriteLine(); // Noncompliant - wrong conditional
//          ^^^^^^^^^^^^^^^^^
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogDebugB(string message)
        {
            Console.WriteLine(); // compliant - in debug-only method
        }

        [System.Diagnostics.Conditional("debug")] // wrong case
        public static void LogDebugC(string message)
        {
            Console.WriteLine(); // Noncompliant - wrong case
        }
    }

    [Conditional("DEBUG")]
    internal class Exceptions_ClassLevelConditionals
    {
        public static void LogDebug(string message)
        {
            Console.WriteLine(message); // Compliant - whole class is DEBUG

            var s = GetData();
            string GetData()
            {
                Console.Write(true); // Compliant - whole class is DEBUG
                return "data";
            }
        }
    }

    [System.Diagnostics.Conditional("debug")] // wrong case
    internal class Exceptions_ClassLevelConditionals2
    {
        public static void LogDebug(string message)
        {
            Console.WriteLine(message); // Noncompliant - wrong case
            Console.Write(message);     // Noncompliant - wrong case
        }
    }

    [Conditional("Wrong conditional")]
    internal class Exceptions_ClassLevelConditionals3
    {
        public static void LogDebugA(string message)
        {
            Console.WriteLine(message); // Noncompliant
        }
    }

    class Exceptions_NestedClassA
    {
        public static void LogDebugA(string message) =>
            Console.WriteLine(message); // Noncompliant

        [Conditional("DEBUG")]  // Anything inside B or C should be ok
        class Exceptions_NestedClassB
        {
            public static void LogDebugB(string message) =>
                Console.WriteLine(message);

            class Exceptions_NestedClassC
            {
                public static void LogDebugC(string message) =>
                    Console.WriteLine(message);
            }
        }
    }
}
