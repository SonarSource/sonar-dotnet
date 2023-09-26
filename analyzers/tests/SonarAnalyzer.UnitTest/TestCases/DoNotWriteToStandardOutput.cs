using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tests.Diagnostics
{
    public class ConsoleLogging
    {
        private static byte[] GenerateKey(byte[] key)
        {
            Console.WriteLine("debug key = {0}", BitConverter.ToString(key)); //Noncompliant {{Remove this logging statement.}}
//          ^^^^^^^^^^^^^^^^^

            Console.WriteLine(); //Noncompliant
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
                return property1;
            }
            set
            {
                Console.WriteLine("Property1 written"); // Noncompliant
                property1 = value;
            }
        }
    }

    internal class Exceptions_MethodLevelConditionals
    {
        [System.Diagnostics.Conditional("Wrong conditional")] // Error [CS0633]
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
}
