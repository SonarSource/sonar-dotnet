using System;

namespace CodeDuplicationTest
{
    public class DuplicatedClass1
    {
        public string DuplicatedProperty { get; }

        public void UniqueMethod(string parameter) =>
            Console.WriteLine(parameter);

        public string SomeDuplicatedProperty { get; }

        public string AnotherDuplicatedProperty { get; set; }

        public int FirstDuplicatedMethod(int a, int b)
        {
            if (a < b)
            {
                return a;
            }
            else if (b < a)
            {
                return b;
            }
            else
            {
                return a + b;
            }
        }

        public int SecondDuplicatedMethod(int a, int b)
        {
            if (a < b)
            {
                return a;
            }
            else if (b < a)
            {
                return b;
            }
            else
            {
                return a + b;
            }
        }

        public void ThisMethodIsDuplicated()
        {
            Console.WriteLine("This is a duplicated method.");
        }

        public void DefinitelyDuplicatedMethod()
        {
            Console.WriteLine("This is definitely another duplicated method.");
        }

        public void ThirdDuplicatedMethod()
        {
            Console.WriteLine(1);
            Console.WriteLine(2);
            Console.WriteLine(3);
            Console.WriteLine(4);
            Console.WriteLine(5);
        }

        public void YetAnotherDuplicatedMethod(string a)
        {
            if (string.IsNullOrWhiteSpace(a))
            {
                a = "somestring";
            }

            Console.WriteLine(a);
        }
    }
}
