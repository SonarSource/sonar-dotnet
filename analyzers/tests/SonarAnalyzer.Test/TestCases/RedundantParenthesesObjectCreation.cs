using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class MyAttribute : Attribute {
        public int MyProperty { get; set; }
    }

    [MyAttribute()] // Noncompliant {{Remove these redundant parentheses.}}
//              ^^
    [MyAttribute] // Compliant // Error [CS0579] - duplicate attribute
    [MyAttribute(MyProperty =5)] // Compliant // Error [CS0579] - duplicate attribute
    class MyClass
    {
        public MyClass()
        {

        }
        public MyClass(int i)
        {

        }
        public int MyProperty { get; set; }
        public static MyClass CreateNew(int propertyValue)
        {
            return new MyClass() //Noncompliant
//                            ^^
            {
                MyProperty = propertyValue
            };
        }

        public static MyClass CreateNew2(int propertyValue)
        {
            return new MyClass
            {
                MyProperty = propertyValue
            };
        }

        public static MyClass CreateNew3(int propertyValue)
        {
            return new MyClass(5)
            {
                MyProperty = propertyValue
            };
        }
    }
}
