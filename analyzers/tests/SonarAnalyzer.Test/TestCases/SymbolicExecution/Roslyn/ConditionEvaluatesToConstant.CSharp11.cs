namespace SonarAnalyzer.Test.TestCases
{
    class AClass
    {
        void DoSomething() { }

        void ListPattern()
        {
            int[] a = new int[] { 1, 2, 3 };
            if (a is [1, 2, 3]) // FN
            {
                DoSomething();
            }
        }

        void ListPattern2()
        {
            int[] a = new int[] { 1, 2, 3 };
            if (a is [1, _, 3]) // FN
            {
                DoSomething();
            }
        }

        void ListPattern3()
        {
            int[] a = new int[] { 1, 2, 3 };
            if (a is [1, .., 3]) // FN
            {
                DoSomething();
            }
        }

        void ListPattern4()
        {
            int[] a = new int[] { 1, 2, 3 };
            if (a is [> 0, < 3, < 42]) // FN
            {
                DoSomething();
            }
        }
    }
}
