#error Error message
#warning Warning message
#pragma warning disable 414, 3021
#pragma warning restore 3021
#line 6
#line 2 "test.cs"
#line default
#line hidden
#define foo
#if foo
#else
#endif
#undef foo
 
extern alias Foo;
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using M = System.Math;
 
#if DEBUG || TRACE
using System.Diagnostics;
#elif SILVERLIGHT
using System.Diagnostics;
#else
using System.Diagnostics;
#endif
 
#region Region
 
#region more
using ConsoleApplication2.Test;
#endregion
using X = int1;
using Y = ABC.X<int>;
 
#endregion
 
[assembly: System.Copyright(@"(C)"" 
 
2009")]
[module: System.Copyright("\n\t\u0123(C) \"2009" + "\u0123")]
 
class TopLevelType : IDisposable
{
    void IDisposable.Dispose() { }
}
 
namespace My
{
    using A.B;
 
    interface CoContra<out T, in K> { }
    delegate void CoContra2<[System.Obsolete()] out T, in K> () where T : struct;
 
    public unsafe partial class A : C, I
    {
        [DllImport("kernel32", SetLastError = true)]
        static extern bool CreateDirectory(string name, SecurityAttribute sa);
 
        private const int global = int.MinValue - 1;
 
        [method: Obsolete]
        public A([param: Obsolete] int foo) :
            base(1)
        {
        L:
            {
                int i = sizeof(int);
                ++i;
            }
 
#if DEBUG
      Console.WriteLine(export.iefSupplied.command);
#endif
            const int? local = int.MaxValue;
            const Guid? local0 = new Guid(r.ToString());
 
            var привет = local;
            var мир = local;
            var local3 = 0, local4 = 1;
            local3 = local4 = 1;
            var local5 = null as Action ?? null;
            var local6 = local5 is Action;
 
            var u = 1u;
            var U = 1U;
            long hex = 0xBADC0DE, Hex = 0XDEADBEEF, l = -1L, L = 1L, l2 = 2l;
            ulong ul = 1ul, Ul = 1Ul, uL = 1uL, UL = 1UL,
                lu = 1lu, Lu = 1Lu, lU = 1lU, LU = 1LU;
            int minInt32Value = -2147483648;
            int minInt64Value = -9223372036854775808L;
 
            bool @bool;
            byte @byte;
            //TODO char @char = 'c', \u0066 = '\u0066', hexchar = '\x0130', hexchar2 = (char)0xBAD; // WORK to do on the lexer
            //TODO string \U00000065 = "\U00000065"; // WORK to do on the lexer
            decimal @decimal = 1.44M;
            @decimal = 1.2m;
            dynamic @dynamic;
            double @double = M.PI;
            @double = 1d;
            @double = 1D;
            @double = -1.2e3;
            float @float = 1.2f;
            @float = 1.44F;
            int @int = local ?? -1;
            long @long;
            object @object;
            sbyte @sbyte;
            short @short;
            string @string = @"""/*";
            uint @uint;
            ulong @ulong;
            ushort @ushort;
            
            dynamic dynamic = local5;
            var add = 0;
            var ascending = 0;
            var descending = 0;
            var from = 0;
            var get = 0;
            var global = 0;
            var group = 0;
            var into = 0;
            var join = 0;
            var let = 0;
            var orderby = 0;
            var partial = 0;
            var remove = 0;
            var select = 0;
            var set = 0;
            var value = 0;
            var var = 0;
            var where = 0;
            var yield = 0;
            where = yield = 0;
 
            if (i > 0)
            {
                return;
            }
            else if (i == 0)
            {
                throw new Exception();
            }
            var o1 = new MyObject();
            var o2 = new MyObject(var);
            var o3 = new MyObject { A = i };
            var o4 = new MyObject(@dynamic)
            {
                A = 0,
                B = 0,
                C = 0
            };
            var o5 = new { A = 0 };
            var dictionaryInitializer = new Dictionary<int, string> 
            { 
                {1, ""}, 
                {2, "a"} 
            };
            float[] a = new float[] 
            {
                0f,
                1.1f
            };
            int[, ,] cube = { { { 111, 112, }, { 121, 122 } }, { { 211, 212 }, { 221, 222 } } };
            int[][] jagged = { { 111 }, { 121, 122 } };
            int[][,] arr = new int[5][,]; // as opposed to new int[][5,5]
            arr[0] = new int[5,5];  // as opposed to arr[0,0] = new int[5];
            arr[0][0,0] = 47;
            int[] arrayTypeInference = new[] { 0, 1, };
            switch (3) { }
            switch (i)
            {
                case 0:
                case 1:
                    {
                        goto case 2;
                    }
                case 2 + 3:
                    {
                        goto default;
                        break;
                    }
                default:
                    {
                        return;
                    }
            }
            while (i < 10)
            {
                ++i;
                if (true) continue;
                break;
            }
            do
            {
                ++i;
                if (true) continue;
                break;
            }
            while (i < 10);
            for (int j = 0; j < 100; ++j)
            {
                for(;;) 
                {
                    for (int i = 0, j = 0; i < length; i++, j++) { }
                    if (true) continue;
                    break;
                }
            }
            label:
            goto label;
            label2: ;
            foreach (var i in Items())
            {
                if (i == 7)
                    return;
                else
                    continue;
            }
            checked
            {
                //TODO checked(++i); // don't know why this does not work...
            }
            unchecked
            {
                //TODO unchecked(++i); // don't know why this does not work...
            }
            lock (sync)
                process();
            using (var v = BeginScope())
            using (A a = new A())
            using (A a = new A(), b = new A())
            using (BeginScope())
                return;
            yield return this.items[3];
            yield break;
            fixed (int* p = stackalloc int[100], q = &y)
            {
                *intref = 1;
            }
            fixed (int* p = stackalloc int[100])
            {
                *intref = 1;
            }
            unsafe
            {
                int* p = null;
            }
            try
            {
                throw null;
            }
            catch (System.AccessViolationException av)
            {
                throw av;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                try { } catch { }
            }
            var anonymous = 
            {
                A = 1,
                B = 2,
                C = 3,
            };
            var query = from c in customers
                        let d = c
                        where d != null
                        join c1 in customers on c1.GetHashCode() equals c.GetHashCode()
                        join c1 in customers on c1.GetHashCode() equals c.GetHashCode() into e
                        group c by c.Country
                            into g
                            orderby g.Count() ascending
                            orderby g.Key descending
                            select new { Country = g.Key, CustCount = g.Count() };
            query = from c in customers
                    select c into d
                    select d;
        }
        ~A()
        {
        }
        private readonly int f1;
        [Obsolete]
        [NonExisting]
        [Foo::NonExisting(var, 5)]
        [CLSCompliant(false)]
        [Obsolete, System.NonSerialized, NonSerialized, CLSCompliant(true || false & true)]
        private volatile int f2;
        [return: Obsolete]
        [method: Obsolete]
        public void Handler(object value)
        {
        }
        public int m<T>(T t)
          where T : class, new()
        {
            base.m(t);
            return 1;
        }
        public string P
        {
            get
            {
                return "A";
            }
            set;
        }
        public abstract string P
        {
            get;
        }
        public abstract int this[int index]
        {
            protected internal get;
            internal protected set;
        }
        [method: Obsolete]
        [field: Obsolete]
        [event: Obsolete]
        //TODO public readonly event Event E; // Seems not possible according to the specification...
        [event: Test]
        public event Action E1
        {
            [Obsolete]
            add { value = value; }
            [Obsolete]
            [return: Obsolete]
            remove { E += Handler; E -= Handler; }
        }
        public static A operator +(A first, A second)
        {
            Delegate handler = new Delegate(Handler);
            return first.Add(second);
        }
        [method: Obsolete]
        [return: Obsolete]
        public static bool operator true(A a)
        {
            return true;
        }
        public static bool operator false(A a)
        {
            return false;
        }
        class C
        {
        }
    }
    public struct S : I
    {
        public S()
        {
        }
        private int f1;
        [Obsolete]
        private volatile int f2;
        public abstract int m<T>(T t)
          where T : struct
        {
            return 1;
        }
        public string P
        {
            get
            {
                int value = 0;
                return "A";
            }
            set;
        }
        public abstract string P
        {
            get;
        }
        public abstract int this[int index]
        {
            get;
            internal protected set;
        }
        public event Event E;
        public static A operator +(A first, A second)
        {
            return first.Add(second);
        }
        fixed int field[10];
        class C
        {
        }
    }
    public interface I
    {
        void A(int value);
        string Value
        {
            get;
            set;
        }
    }
    [type: Flags]
    public enum E
    {
        A,
        B = A,
        C = 2 + A,
 
#if DEBUG
    D,
#endif
 
    }
    public delegate void Delegate(object P);
    namespace Test
    {
        using System;
        using System.Collections;
        public class Список
        {
            public static IEnumerable Power(int number, int exponent)
            {
                Список Список = new Список();
                Список.Main();
                int counter = 0;
                int אתר = 0;
                while (++counter++ < --exponent--)
                {
                    result = result * number + +number+++++number;
                    yield return result;
                }
            }
            static void Main()
            {
                foreach (int i in Power(2, 8))
                {
                    Console.Write("{0} ", i);
                }
            }
        }
    }
}
 
namespace ConsoleApplication1
{
    namespace RecursiveGenericBaseType
    {
        class A<T> : B<A<T>, A<T>> where T : A<T>
        {
            protected virtual A<T> M() { }
            protected abstract B<A<T>, A<T>> N() { }
            static B<A<T>, A<T>> O() { }
        }
 
        sealed class B<T1, T2> : A<B<T1, T2>>
        {
            protected override A<T> M() { }
            protected sealed override B<A<T>, A<T>> N() { }
            new static A<T> O() { }
        }
    }
 
    namespace Boo
    {
        public class Bar<T> where T : IComparable
        {
            public T f;
            public class Foo<U> : IEnumerable<T>
            {
                public void Method<K, V>(K k, T t, U u)
                    where K : IList<V>, IList<T>, IList<U>
                    where V : IList<K>
                {
                    A<int> a;
                    M(A<B, C>(5));
                }
            };
        };
    };
 
    class Test
    {
        void Bar3()
        {
            var x = new Boo.Bar<int>.Foo<object>();
            x.Method<string, string>(" ", 5, new object());
 
            var q = from i in new int[] { 1, 2, 3, 4 }
                    where i > 5
                    select i;
        }
 
        public static implicit operator Test(string s)
        {
            return new ConsoleApplication1.Test();
        }
        public static explicit operator Test(string s)
        {
            return new Test();
        }
 
        public int foo = 5;
        void Bar2()
        {
            foo = 6;
            this.Foo = 5.GetType(); Test t = "sss";
        }
 
        public event EventHandler MyEvent = delegate { };
 
        void Blah()
        {
            int i = 5;
            int? j = 6;
 
            Expression<Func<int>> e = () => i;
            Expression<Func<bool, Action>> e2 = b => () => { return; };
            Func<bool, bool> f = delegate (bool a)
            {
                return !a;
            };
            Func<int, int, int> f2 = (a, b) => 0;
            f2 = (int a, int b) => 1;
            Action a = Blah;
            f2 = () => {};
            f2 = () => {;};
        }
 
        delegate Recursive Recursive(Recursive r);
        delegate Recursive Recursive<A,R>(Recursive<A,R> r);
 
        public Type Foo
        {
            [Obsolete("Name", error = false)]
            get
            {
                var result = typeof(IEnumerable<int>);
                var t = typeof(int?) == typeof(Nullable<int>);
                t = typeof(IEnumerable<int?[][][]>);
                return typeof(IEnumerable<>);
            }
            set
            {
                var t = typeof(System.Int32);
                t.ToString();
                t = value;
            }
        }
 
        public void Constants()
        {
            int i = 1 + 2 + 3 + 5;
            global::System.String s = "a" + (System.String)"a" + "a" + "a" + "a" + "A";
        }
 
        public void ConstructedType()
        {
            List<int> i = null;
            int c = i.Count;
        }
    }
}
 
namespace Comments.XmlComments.UndocumentedKeywords
{
    /// <summary>
    /// Whatever 
    /// </summary>
    /// <!-- c -->
    /// <![CDATA[c]]> //
    /// <c></c> /* */
    /// <code></code>
    /// <example></example>
    /// <exception cref="bla"></exception>
    /// <include file='' path='[@name=""]'/>
    /// <permission cref=" "></permission>
    /// <remarks></remarks>
    /// <see cref=""/>
    /// <seealso cref=" "/>
    /// <value></value>
    /// <typeparam name="T"></typeparam>
    class /*///*/C<T>
    {
        void M<U>(T t, U u)
        {
            // comment
            /* *** / */
            /* //
             */
            /*s*///comment
            // /***/
            /*s*/int /*s*/intValue = 0;
            intValue = intValue /*s*/+ 1;
            string strValue = /*s*/"hello";
            /*s*/MyClass c = new MyClass();
            string verbatimStr = /*s*/@"\\\\";
        }
    }
 
    //General Test F. Type a very long class name, verify colorization happens correctly only upto the correct size (118324)
    class TestClassXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX/*Scen8*/{ }
 
    class TestClassXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX22/*Scen9*/{ }
 
    class yield
    {
        //TODO void Foo<U>(__arglist) // Don't know why it's not working...
        void Foo()
        {
            C<U> c = null;
            c.M<int>(5, default(U));
            TypedReference tr = __makeref(c);
            Type t = __reftype(tr);
            //TODO int j = __refvalue(tr, int); // Don't know why it's not working...
            Params(a: t, b: t);
            Params(ref c, out c);
        }
        void Params(ref dynamic a, out dynamic b, params dynamic[] c) {}
        void Params(out dynamic a = 2, ref dynamic c = default(dynamic), params dynamic[][] c) {}
 
        public override string ToString() { return base.ToString(); } 
 
        public partial void OnError();
 
        public partial void method()
        {
            int?[] a = new int?[5];/*[] bug*/ // YES []
            int[] var = { 1, 2, 3, 4, 5 };/*,;*/
            int i = a[i];/*[]*/
            Foo<T> f = new Foo<int>();/*<> ()*/
            f.method();/*().*/
            i = i + i - i * i / i % i & i | i ^ i;/*+ - * / % & | ^*/
            bool b = true & false | true ^ false;/*& | ^*/
            b = !b;/*!*/
            i = ~i;/*~i*/
            b = i < i && i > i;/*< && >*/
            int? ii = 5;/*? bug*/ // NO ?
            int f = true ? 1 : 0;/*? :*/   // YES :
            i++;/*++*/
            i--;/*--*/
            b = true && false || true;/*&& ||*/
            //TODO i << 5;/*<<*/ // Not sure it is possible in the spec...
            //TODO i >> 5;/*>>*/  // Not sure it is possible in the spec...
            b = i == i && i != i && i <= i && i >= i;/*= == && != <= >=*/
            i += 5.0;/*+=*/
            i -= i;/*-=*/
            i *= i;/**=*/
            i /= i;/*/=*/
            i %= i;/*%=*/
            i &= i;/*&=*/
            i |= i;/*|=*/
            i ^= i;/*^=*/
            i <<= i;/*<<=*/
            i >>= i;/*>>=*/
            object s = x => x + 1;/*=>*/
            Point point;
            unsafe
            {
                Point* p = &point;// &
                p->x = 10;// ->
            }
            IO::BinaryReader br = null;
        }
 
        struct Point { public int X; public int Y; public void ThisAccess() { this = this; } }
    }
}