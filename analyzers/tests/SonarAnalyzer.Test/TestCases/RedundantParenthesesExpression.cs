using System;
using System.Collections.Generic;
using System.Globalization;

namespace Tests.Diagnostics
{
    class Bar
    {
        public Bar(object param) { }
    }

    class Foo
    {
        private object a;

        void Invoke(object param) { }

        object Del()
        {
            return (3);
            return (a);
            return (false);
            return (((1)+(1))-(2));
            return (((a)));
//                 ^^ Noncompliant [0] {{Remove these redundant parentheses.}}
//                      ^^ Secondary@-1 [0] {{Remove the redundant closing parentheses.}}
            var x = 1;
            return (x + 1);
            return (string.Empty);
            return ("".ToString());
            return ("".ToString((CultureInfo.InvariantCulture)));
            return (string.Compare("a", "b"));

            var y = 12;
            return ((((x & 0x0000FFFF))) | y);
//                  ^^ Noncompliant [1]
//                                    ^^ Secondary@-1 [1]

            int x2 = (y / 2 + 1);
            int y2 = (4 + x2) * y2;
            int z = ((4 + x2) * y2);
            int u = ((4 + x2) * (y2));
            int u2 = ((4 + x2) * ((y2)));
//                               ^ Noncompliant [2]
//                                    ^ Secondary@-1 [2]
            if ((0)) { } // Error [CS0029] - cannot convert
            while ((true)) { }
            do { } while ((true));

            Console.Write((x));
            Console.Write((x + y));
            Console.Write("", (x), y);
            Console.Write("", (x + y), y);
            Console.Write(false ? (true ? 1 : 2) : 2);
            Console.Write(false ? 0 : (true ? 1 : 2));
            Console.Write(false ? 0 : (1 + 1));
            Console.Write(false ? 0 : (1));
            Console.Write((false) ? 0 : 1);
            Console.Write((x > y) ? 0 : 1);
            Console.Write((false ? false : true) ? 0 : 1);
            Console.Write((foo()) ? 0 : 1); // Error [CS0841] - undeclared var
            Console.Write((foo) ? 0 : 1); // Error [CS0841] - undeclared var
            Invoke(((int)plop)); // Error [CS0103] - undeclared var

            int[] tab;
            tab[(1 + 2)]; // Error [CS0201] - not a statement
            tab[(1 + 2) / 2]; // Error [CS0201] - not a statement

            int p = ((int[])tab)[0];
            var n = new Bar((1 / 3));

            this.a = (b); // Error [CS0103] - unknown b
            this.a = (true ? 1 : 2);
            this.a = false ? (true ? 1 : 2) : 2;
            this.a = (1 + 2) / 2;
            this.a = ((int[])contentSpec.value)[0]; // Error [CS0103] - unknown contentSpec

            object[] foo = new[] { (true ? 1 : 2) }; // Error [CS0029] - cannot convert
        }

        public static short value1 = (((short)(0)));
//                                   ^ Noncompliant [3]
//                                                ^ Secondary@-1 [3]
        public static short value2 = (short)(0);
        public static short value3 = ((short)0);
    }
}
