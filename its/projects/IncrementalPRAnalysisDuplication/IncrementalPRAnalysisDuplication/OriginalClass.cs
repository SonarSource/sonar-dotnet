using System;

namespace IncrementalPRAnalysisDuplication
{
    public class OriginalClass
    {
        private string String = "String occurence 1";

        private void SomeMethod(int a, int b)
        {
            var someString = "String occurence 2";
            for (int i = 0; i < a; i++)
            {
                someString += b.ToString();
            }

            Console.WriteLine("String occurence 3", someString);
        }

        private void SomeMethod2(object o)
        {
            if (o == "String occurence 4")
            {
                throw new ArgumentException("String occurence 5");
            }
            else
            {
                SomeMethod(16, 23);
            }
        }

        private class SomeInnerClass
        {
            public const int a = 1;
        }
    }
}
