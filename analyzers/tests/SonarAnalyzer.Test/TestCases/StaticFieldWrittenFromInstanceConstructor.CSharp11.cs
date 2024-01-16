using System;

namespace SonarAnalyzer.Test.TestCases
{
    public class Person
    {
        private static int expectedFingers; // Secondary

        public Person(DateTime birthday)
        {
            expectedFingers >>>= 5; // Noncompliant
        }
    }
}
