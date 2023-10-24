using System;
using System.Collections.Generic;
using System.IO;

namespace Tests.Diagnostics
{
    class MyClass
    {
        public MyClass()
        {

        }
        public MyClass(int p)
        {

        }
    }

    class DefaultBaseConstructorCall : MyClass
    {
        public DefaultBaseConstructorCall() /*c*/  : /*don't keep*/ base() // Fixed


        {
        }

        public DefaultBaseConstructorCall(string s)
            : base() // Fixed
        {
        }

        public DefaultBaseConstructorCall(string[] s)


            : base() // Fixed



        {
        }

        public DefaultBaseConstructorCall(string[] s, int i) /*comment
            some comment*/

            : base() // Fixed

            /*some comment2*/

        {
        }

        public DefaultBaseConstructorCall(int parameter) : base(parameter)
        {
        }
    }

    public class MyClass1
    {
        static MyClass1() // Fixed
        {

        }
        public MyClass1() // Fixed
        {

        }
    }

    public class MyClass2
    {
        private MyClass2()
        {

        }
    }
    public class MyClass3
    {
        public MyClass3(int i)
        {
        }
    }

    public class MyClass4
    {
        public MyClass4()
        {
        }
        public MyClass4(int i)
        {
        }
    }

    public class MyClass5 : MyClass4
    {
        public MyClass5() : base() // Fixed
        {
        }
    }

    public class MyClass6 : MyClass4
    {
        public MyClass6() : base(10)
        {
        }
    }

    public class MyClass7
    {
        Stream unmanagedResource;

        ~MyClass7() // Compliant
        {
            if (unmanagedResource != null)
            {
                unmanagedResource.Dispose();
                unmanagedResource = null;
            }
        }
    }

    class LambdaCtor
    {
        private int i;
        LambdaCtor() : base() => i++; // Fixed
    }

    class LambdaCtorWithLineEnding
    {
        private int i;
        LambdaCtorWithLineEnding()
            : base() // Fixed
            => i++;
    }

    class LambdaCtorTrailing
    {
        private int i;
        LambdaCtorTrailing() : /*a*/ base() /*b*/ => /*c*/ i++ /*d*/ ;   // Fixed
    }
}
