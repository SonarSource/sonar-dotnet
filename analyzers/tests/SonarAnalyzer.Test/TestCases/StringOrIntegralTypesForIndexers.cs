using System;

namespace Tests.Diagnostics
{
    class Test1
    {
        public int this[Test1 index]
//                      ^^^^^ Noncompliant {{Use string, integral, index or range type here, or refactor this indexer into a method.}}
        {
            get { return 0; }
        }
    }

    class Test2
    {
        public int this[DateTime index]
//                      ^^^^^^^^ {{Use string, integral, index or range type here, or refactor this indexer into a method.}}
        {
            get { return 0; }
        }
    }

    class Test3
    {
        public int this[Test1 index, Test1 index2]
        {
            get { return 0; }
        }
    }

    class Test4
    {
        public int this[params string[] paths]
        {
            get { return 0; }
        }
    }

    class Test5
    {
        public int this[string index]
        {
            get { return 0; }
        }
    }

    class Test6
    {
        public int this[dynamic index]
        {
            get { return 0; }
        }
    }

    class Test7
    {
        public int this[object index]
        {
            get { return 0; }
        }
    }

    class Test8
    {
        public int this[short index]
        {
            get { return 0; }
        }
    }

    class Test9
    {
        public int this[int index]
        {
            get { return 0; }
        }
    }

    class Test10
    {
        public int this[long index]
        {
            get { return 0; }
        }
    }

    class Test11
    {
        public int this[ushort index]
        {
            get { return 0; }
        }
    }

    class Test12
    {
        public int this[uint index]
        {
            get { return 0; }
        }
    }

    class Test13
    {
        public int this[ulong index]
        {
            get { return 0; }
        }
    }

    class Test14
    {
        public int this[char index]
        {
            get { return 0; }
        }
    }

    class Test15
    {
        public int this[byte index]
        {
            get { return 0; }
        }
    }

    class Test16
    {
        public int this[sbyte index]
        {
            get { return 0; }
        }
    }

    class Test17<T>
    {
        public int this[T index]
        {
            get { return 0; }
        }
    }

    class Test18
    {
        readonly int[] bar = new int[] { 1, 2, 3, 4, 5 };
        public int[] this[Range range] => bar[range];
    }

    class Test19
    {
        readonly int[] bar = new int[] { 1, 2, 3, 4, 5 };
        public int this[Index index] => bar[index];
    }

    class Test20
    {
        public int this[IntPtr index] // Noncompliant
        {
            get { return 0; }
        }
    }

    class Test21
    {
        public int this[UIntPtr index] // Noncompliant
        {
            get { return 0; }
        }
    }
}
