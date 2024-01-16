using System;
using PointerAlias = System.IntPtr;

namespace Tests.Diagnostics
{
    public class Program
    {
        public int publicOtherType;
        private IntPtr privatePointer1;
        private UIntPtr privatePointer2;
        internal IntPtr internalPointer1;
        internal UIntPtr internalPointer2;
        public readonly IntPtr publicReadonlyPointer1;
        protected readonly IntPtr protectedReadonlyPointer1;
        protected readonly UIntPtr protectedReadonlyPointer2;
        protected internal readonly IntPtr protectedInternalReadonlyPointer1;
        protected internal readonly UIntPtr protectedInternalReadonlyPointer2;

        public IntPtr publicPointer1; // Noncompliant {{Make 'publicPointer1' 'private' or 'protected readonly'.}}
        public UIntPtr publicPointer2; // Noncompliant
        protected IntPtr protectedPointer1; // Noncompliant
        protected UIntPtr protectedPointer2; // Noncompliant
        protected internal IntPtr protectedInternalPointer1; // Noncompliant
        protected internal UIntPtr protectedInternalPointer2; // Noncompliant
        protected internal PointerAlias protectedInternalAliasPointer; // FN

        public IntPtr pointer1, // Noncompliant {{Make 'pointer1' 'private' or 'protected readonly'.}}
            pointer2, // Noncompliant {{Make 'pointer2' 'private' or 'protected readonly'.}}
            pointer3; // Noncompliant {{Make 'pointer3' 'private' or 'protected readonly'.}}

        public class PublicInnerClass
        {
            public IntPtr publicPointer1; // Noncompliant
        }

        private class PrivateInnerClass
        {
            public IntPtr publicPointer1;
        }

        internal class InternalInnerClass
        {
            public IntPtr publicPointer1;
        }
    }

    public unsafe class PointerTypes
    {
        private int* privatePointer1;
        private char* privatePointer2;
        internal byte* internalPointer1;
        internal uint* internalPointer2;
        public readonly long* publicReadonlyPointer1;
        protected readonly byte* protectedReadonlyPointer1;
        protected readonly int* protectedReadonlyPointer2;
        protected internal readonly int* protectedInternalReadonlyPointer1;
        protected internal readonly int* protectedInternalReadonlyPointer2;

        public int* publicPointer1; // Noncompliant {{Make 'publicPointer1' 'private' or 'protected readonly'.}}
        public char* publicPointer2; // Noncompliant
        protected byte* protectedPointer1; // Noncompliant
        protected long* protectedPointer2; // Noncompliant
        protected internal uint* protectedInternalPointer1; // Noncompliant
        protected internal SomeStruct* protectedInternalPointer2; // Noncompliant

        public int* pointer1, // Noncompliant {{Make 'pointer1' 'private' or 'protected readonly'.}}
            pointer2, // Noncompliant {{Make 'pointer2' 'private' or 'protected readonly'.}}
            pointer3; // Noncompliant {{Make 'pointer3' 'private' or 'protected readonly'.}}

        public class PublicInnerClass
        {
            public int* publicPointer1; // Noncompliant
        }

        private class PrivateInnerClass
        {
            public int* publicPointer1;
        }

        internal class InternalInnerClass
        {
            public int* publicPointer1;
        }

        public struct SomeStruct
        {
            int value;
        }
    }
}
