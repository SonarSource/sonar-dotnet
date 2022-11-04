using System;

namespace SonarAnalyzer.UnitTest.TestCases
{
    public class Person
    {
        private static int expectedFingers; // FN

        public Person(DateTime birthday)
        {
            expectedFingers >>>= 5; // FN
        }
    }
}
