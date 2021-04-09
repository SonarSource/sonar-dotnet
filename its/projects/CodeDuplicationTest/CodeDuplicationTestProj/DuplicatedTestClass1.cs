using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeDuplicationTest
{
    [TestClass]
    public class DuplicatedTestClass1
    {
        public string DefinitelyNotDuplicated { get; }

        public void UniqueMethod(string parameter) =>
            Console.WriteLine(parameter);

        public string SomeDuplicatedProperty { get; }

        public string AnotherDuplicatedProperty { get; set; }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
        public void ThisMethodIsNotDuplicated()
        {
            Console.WriteLine("This is not a duplicated method.");
        }

        [TestMethod]
        public void YetAnotherNotDuplicatedMethod()
        {
            Console.WriteLine("This is yet not another duplicated method.");
        }

        [TestMethod]
        public void ThirdDuplicatedMethod()
        {
            Console.WriteLine(1);
            Console.WriteLine(2);
            Console.WriteLine(3);
            Console.WriteLine(4);
            Console.WriteLine(5);
        }

        [TestMethod]
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
