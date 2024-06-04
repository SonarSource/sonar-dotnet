using System;
using System.Runtime.InteropServices;

namespace Tests.Diagnostics
{
    struct ValueType1
    {
        public int field;
    }

    class RefType
    {
        public int field2;
    }

    struct ValueType2
    {
        public RefType field1;
    }

    public interface ISampleInterface
    {
        string Name { get; }
    }

    class MyClass
    {
        class Nested
        {
            private int field; // Noncompliant, shouldn't it be initialized? This way the value is always default(int), 0.
//                      ^^^^^
            private int field2;
            private static int field3; // Noncompliant {{Remove unassigned field 'field3', or set its value.}}
            private static int field4;
            private static int field5; //reported by unused member rule
            private static int field6 = 42;

            [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
            private int withAttribute; // Compliant

            private readonly int field7; // Noncompliant
            private int Property { get; }  // Noncompliant {{Remove unassigned auto-property 'Property', or set its value.}}
            private int Property2 { get; }
            private int Property3 { get; } = 42; // Unused, S1144 reports on it
            private virtual int VirtualPrivateProperty { get; } // Error [CS0621]: virtual or abstract members cannot be private
            private Lazy<int> Lazy { get; }

            private int Property4 { get; set; }  // Noncompliant
            private int Property5 { get; set; } = 42;
            private int Property6 { get { return 42; } set { } }

            private ValueType1 v1; // Compliant, a member is assigned
            private ValueType1 v2; // Compliant, a member is assigned
            private ValueType1 v3; // Noncompliant

            private ValueType2 v4; // Compliant, a member is assigned
            private ValueType2 v5; // Compliant, a member is assigned
            private ValueType2 v6; // Noncompliant

            public Nested()
            {
                Property2 = 42;
                v1.field++;
                ((((v2).field))) = 42;
                Console.WriteLine(v3.field);

                this.v4.field1.field2++;
                ((((this.v5).field1)).field2) = 42;
                Console.WriteLine(v6.field1?.field2);
            }

            public void Print()
            {
                Console.WriteLine((field)); //Will always print 0
                Console.WriteLine((field6));
                Console.WriteLine((field7));
                Console.WriteLine((Property));
                Console.WriteLine((Property4));
                Console.WriteLine((Property6));

                Console.WriteLine(this.field); //Will always print 0
                Console.WriteLine(MyClass.Nested.field3); //Will always print 0
                new MyClass().M(ref MyClass.Nested.field4);

                this.field2 = 10;
                field2 = 10;
            }
        }

        public void M(ref int f) { }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/242
    public class MyClass2
    {
        [StructLayout(LayoutKind.Sequential)]
        private class InteropMethodArgument
        {
            public uint number; // Compliant, we don't raise on members of classes with StructLayout attribute
        }
    }

    public class MyTupleClass
    {
        private readonly int _foo;
        private readonly int _bar;

        public MyTupleClass()
        {
            (_foo, _bar) = GetFoobar();
        }

        private static (int f, int b) GetFoobar() => (1, 2);
    }

    public static class Memory
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys { get; set; }
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        public static ulong TotalPhysicalMemory
        {
            get
            {
                MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(memStatus))
                {
                    return memStatus.ullTotalPhys;
                }
                else
                {
                    return memStatus.ullAvailPhys;
                }
            }
        }
    }

    public sealed class OuterFoo
    {
        private static class InnerFactory
        {
            public static InnerFoo<string> Instance = new InnerFoo<string> { X = "X" };
        }

        private class InnerFoo<T>
        {
            public string X;
            public string Y;  // Noncompliant

            public void Foo()
            {
                Console.Write(X);
                Console.Write(Y);
            }
        }
    }

    // Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/5232
    public class Repro5232
    {
        private int Random
        {
            get => new Random().Next();
        }
        public void Print()
        {
            Console.WriteLine(Random);
        }
    }

    [Serializable]
    public class SerializableClass
    {
        private int field; // Compliant, see: https://github.com/SonarSource/sonar-dotnet/issues/5451

        private int Prop { get; set; } // Compliant, see: https://github.com/SonarSource/sonar-dotnet/issues/5451

        public void Print()
        {
            Console.WriteLine(field);
            Console.WriteLine(Prop);
        }
    }
}

// Reproducer: https://github.com/SonarSource/sonar-dotnet/issues/9106
public class Repro_9106
{
    private int _foo; // Compliant

    public ref int Foo => ref _foo;
}
