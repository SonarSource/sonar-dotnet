using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class Mod
    {
        public static void DoSomething(ref int x)
        {
        }
        public static void DoSomething2(out int x)
        {
            x = 6;
        }
    }

    class Person
    {
        private readonly int _birthYear;  // Fixed
        readonly int _birthMonth = 3;  // Fixed
        int _birthDay = 31;  // Compliant, the setter action references it
        int _birthDay2 = 31;  // Compliant, it is used in a delegate
        int _birthDay3 = 31;  // Compliant, it is passed as ref outside the ctor
        int _birthDay4 = 31;  // Compliant, it is passed as out outside the ctor
        int _legSize = 3;
        int _legSize2 = 3;
        int _neverUsed;

        private readonly Action<int> setter;

        Person(int birthYear)
        {
            setter = i => { _birthDay = i; };

            System.Threading.Thread t1 = new System.Threading.Thread
                (delegate()
                {
                    _birthDay2 = 42;
                });
            t1.Start();

            _birthYear = birthYear;
        }

        private void M()
        {
            Mod.DoSomething(ref this._birthDay3);
            Mod.DoSomething2(out _birthDay4);
        }

        public int LegSize
        {
            get
            {
                _legSize2++;
                return _legSize;
            }
            set { _legSize = value; }
        }
    }

    partial class Partial
    {
        private int i; // Non-compliant, but not reported now because of the partial
    }
    partial class Partial
    {
        public Partial()
        {
            i = 42;
        }
    }

    class X
    {
        private int x; // Compliant
        private int y; // Compliant
        private readonly int z = 10; // Fixed
        private readonly int w = 10; // Fixed
        public X()
        {
            new X().x = 12;
            var xx = new X();
            Modif(ref xx.y);

            Modif(ref (z));
            this.w = 42;
        }

        private void Modif(ref int i) { }
    }

    struct X1Struct
    {
        public Y1 y;
    }
    class X1Class
    {
        public Y1 y;
    }
    struct Y1
    {
        public string z;
    }

    class MyClass
    {
        private X1Struct x; // Compliant
        // See https://github.com/SonarSource/sonar-dotnet/issues/2291
        private X1Struct y; // Compliant - could be set as readonly but this changes the behavior of the field
        private readonly IntPtr myPtr; // Fixed

        private readonly X1Class z; // Fixed

        private bool field = false;

        public MyClass()
        {
            x = new X1Struct();
            y = new X1Struct();
            z = new X1Class();
            myPtr = new IntPtr(12);
            (this.y.y).z = "a";
            (this.z.y).z = "a";
            if (this.field)
            { }
        }

        public void M()
        {
            (this.x.y).z = "a";
            (this.z.y).z = "a";
        }

        private class Nested
        {
            private readonly MyClass inst;
            public Nested()
            {
                inst = new MyClass();
            }
            private void Method()
            {
                this.inst.field = false;
            }
        }
    }

    class Attributed
    {
        [My]
        private int myField1; // Compliant because of the attribute

        public Attributed()
        {
            myField1 = 42;
        }
    }

    public class MyAttribute : Attribute { }

    // See https://github.com/SonarSource/sonar-csharp/issues/1009
    // Issue with leading trivia not moved to the readonly modifier
    public class MyClassWithField
    {
        #region Test
        readonly string teststring = null; // Fixed
        #endregion

        public void Do()
        {
            var x = teststring;
        }
    }

    [Serializable]
    public class SerializableClass
    {
        private int field; // Compliant, containing class is marked with [Serializable]

        public SerializableClass()
        {
            field = 5;
        }
    }

    public class DerivedFromSerializable : SerializableClass
    {
        private readonly int otherField; // Fixed

        public DerivedFromSerializable()
        {
            otherField = 5;
        }
    }
}
