using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tests.Diagnostics
{
    [Flags]
    enum Permissions
    {
        None = 0,
        Read = 1,
        Write = 2,
        Execute = 4
    }

    [Flags]
    enum Permissions2
    {
        None = 0,
        Read = 1,
        Write = 2,
        Execute = 4
    }

    class NonFlagsEnumInBitwiseOperation
    {
        void Test()
        {
            var x = Permissions.Read | Permissions.Write;  // Fixed
            x = Permissions.Read & Permissions.Write;  // Fixed
            x = Permissions.Read ^ Permissions.Write;  // Fixed
            x &= Permissions.Read;  // Fixed

            x = ~Permissions.Read;  // Compliant

            var y = Permissions2.Read | Permissions2.Write;

            var w = 1 | 3;

            var v = System.ComponentModel.DesignerSerializationVisibility.Content
                | System.ComponentModel.DesignerSerializationVisibility.Hidden; // Fixed

            // MethodImplAttributes lacks [Flags] but is designed for bitwise operations
            // https://stackoverflow.com/questions/38689649/why-is-methodimplattributes-not-marked-with-flagsattribute
            var z = MethodImplAttributes.NoInlining | MethodImplAttributes.NoOptimization; // Compliant - BCL enum without [Flags] but designed for bitwise use
        }
    }
}
