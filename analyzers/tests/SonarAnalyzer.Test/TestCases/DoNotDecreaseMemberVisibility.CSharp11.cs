using System;
using System.Collections.Generic;

namespace MyLibrary
{
    public interface IMyInterface
    {
        public static virtual void MyMethod() { }
    }

    public interface IMyOtherInterface : IMyInterface
    {
        private static void MyMethod() { }
    }

    public class MyClass<T> where T : IMyOtherInterface
    {
        public MyClass(IMyOtherInterface other)
        {
            T.MyMethod(); // Compliant, the method from IMyInterface is called
        }
    }
}
