using System;

namespace Tests.Diagnostics
{
    class Foo
    {
        private void Test()
        {
            byte b = 1;
            b = (byte)(b << 10);
            b = (byte)(b << 10);
            b = 1 << 0;

            sbyte sb = 1;
            sb = (sbyte)(sb << 10);

            int i = 1 << 10;
            i = i << 32;  // Noncompliant
            uint ui = 1 << 32; // Noncompliant
            Int32 i32 = 1 << 32; // Noncompliant
            Int64 i64 = 1 << 32;
            Int64 i642 = 1 << 64; // Noncompliant

            long l = 1 << 32;
            l = 1 << 64; // Noncompliant {{Remove this useless shift by 64.}}
            ulong ul = 1 << 64; // Noncompliant
            ul = 1 << 65; // Noncompliant {{Correct this shift; shift by 1 instead.}}

            ul <<= 0;
            ul <<= 1025; // Noncompliant {{Correct this shift; shift by 1 instead.}}
            ul <<= "I am not an integer"; // Error [CS0019]

            b <<= 16;
            b <<= 17;

            int value =
                      (b & 0xff) << 56 // Noncompliant {{Either promote shift target to a larger integer type or shift by 24 instead.}}
                    | (b & 0xff) << 48 // Noncompliant {{Either promote shift target to a larger integer type or shift by 16 instead.}}
                    | (b & 0xff) << 40 // Noncompliant {{Either promote shift target to a larger integer type or shift by 8 instead.}}
                    | (b & 0xff) << 32 // Noncompliant {{Either promote shift target to a larger integer type or shift by less than 32 instead.}}
                    | (b & 0xff) << 24
                    | (b & 0xff) << 16
                    | (b & 0xff) << 8
                    | (b & 0xff) << 0;
        }

        private void NonIntegerTypes()
        {
            object o = 1 << 1024; // Compliant
            IDoNotExist e = 1 << 1024; // Compliant // Error [CS0246]

            double d = 1 << 1024; // Compliant
            float f = 1 << 1024; // Compliant
            decimal m = 1 << 1024; // Compliant
            Single s = 1 << 1024; // Compliant
        }


        private int Property
        {
            get { return 1 << 0; } // Noncompliant
            set { int i = 1 << 0; } // Noncompliant
        }

        private void Lambda()
        {
            Func<int> x = () => 1 << 0; // Noncompliant
        }

        private void ParanthesesAttack()
        {
            int i;
            i = (1) << 0; // Noncompliant


            i = (((1))) << 0; // Noncompliant


            i = 1 << (0); // Noncompliant


            i = 1 << (((0))); // Noncompliant


            (((i))) = (((1))) << (((0))); // Noncompliant


            (((i))) <<= (((0))); // Noncompliant
        }

        public byte Next() => 1;

        public int GetInt()
        {
            return (short)(((Next() & 0xff) << 24)
                        | ((Next() & 0xff) << 16)
                        | ((Next() & 0xff) << 8)
                        | ((Next() & 0xff) << 0));
        }

        private int RightShift()
        {
            // Error@+1 [CS0165] - use of unassigned var
            int i = i >> 60; // Noncompliant {{Correct this shift; '60' is larger than the type size.}}
            i >>= 60; // Noncompliant {{Correct this shift; '60' is larger than the type size.}}
            int i2 = i >> 31; // Compliant
            // Error@+1 [CS0165] - use of unassigned var
            ulong ul = ul >> 64; // Noncompliant {{Correct this shift; '64' is larger than the type size.}}
            i = i >> 0; // Compliant


            i = i >> 0; // Noncompliant {{Remove this useless shift by 0.}}


            ul = ul >> 32; // Compliant
            ul = ul << 0;  // Compliant


            ul = ul << 32; // Compliant
            ul = ul >> 0;  // Compliant

            return 42;
        }

        // See https://github.com/SonarSource/sonar-dotnet/issues/2016
        private void ImproveShiftingBehavior()
        {
            short x = 12;
            short result_01 = (short)(x >> 1);
            short result_17 = (short)(x >> 17);
            short result_33 = (short)(x >> 33); // Noncompliant
        }
    }
}

// https://sonarsource.atlassian.net/browse/NET-1122
namespace Enums
{
    enum Int8Enum : byte
    {
        One = 1,
        Two = 1 << 1,
        Three = 1 << 7,
        Four = unchecked((byte)(1 << 9)), // FN
        Five = (byte)(1 << 9), // Error [CS0221]
        Six = unchecked((byte)(Three << 2)), // FN
        Seven = Three << 2, // Error [CS0031]
    }

    enum Int16Enum : short
    {
        One = 1,
        Two = 1 << 1,
        Three = 1 << 13,
        Four = unchecked((short)(1 << 17)), // FN
        Five = (short)(1 << 17), // Error [CS0221]
        Six = unchecked((short)(Three << 2)), // FN
        Seven = Three << 2, // Error [CS0031]
    }

    enum Int32Enum : int
    {
        One = 1,
        Two = 1 << 1,
        Three = 1 << 31,
        Four = 1 << 33, // FN
        Five = Three << 2, // FN
    }

    enum Int64Enum : long
    {
        One = 1,
        Two = 1 << 1,
        Three = 1 << 63,
        Four = 1 << 65, // FN
        Five = Three << 2, // FN
    }
}
