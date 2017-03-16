using System;
using System.Collections.Generic;

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

        void Del()
        {
            return (3);
            return (a);
            return (false);
            return (((1)+(1))-(2));
            return (((a)));
//                 ^^ Noncompliant [0]
//                      ^^ Secondary@-1 [0]
            return (x + 1);
            return (string.Empty);
            return ("".ToString());
            return ("".ToString((CultureInfo.InvariantCulture)));
            return (string.Compare("a", "b"));

            return ((((x & 0x0000FFFF))) | y);
//                  ^^ Noncompliant [1]
//                                    ^^ Secondary@-1 [1]

            int x = (y / 2 + 1);
            int y = (4 + x) * y;
            int z = ((4 + x) * y);
            int u = ((4 + x) * (y));
            int u = ((4 + x) * ((y)));
//                             ^ Noncompliant [2]
//                                 ^ Secondary@-1 [2]
            if ((0)) { }
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
            Console.Write((foo()) ? 0 : 1);
            Console.Write((foo) ? 0 : 1);
            Invoke(((int)plop));

            int[] tab;
            tab[(1 + 2)];
            tab[(1 + 2) / 2];

            int p = ((int[])tab)[0];
            var n = new Bar((1 / 3));

            this.a = (b);
            this.a = (true ? 1 : 2);
            this.a = false ? (true ? 1 : 2) : 2;
            this.a = (1 + 2) / 2;
            this.a = ((int[])contentSpec.value)[0];

            object[] foo = new[] { (true ? 1 : 2) };
        }

        public static short value1 = (((short)(0)));
//                                   ^ Noncompliant [3]
//                                                ^ Secondary@-1 [3]
        public static short value2 = (short)(0);
        public static short value3 = ((short)0);
    }
}