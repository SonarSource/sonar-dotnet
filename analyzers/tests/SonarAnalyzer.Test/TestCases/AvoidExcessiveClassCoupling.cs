using System;
using System.Collections.Generic;
using System.IO;
using MyFileStream = System.IO.FileStream;
using FooBaseAlias = Tests.Diagnostics.FooBase;

namespace Tests.Diagnostics
{
    public interface IFoo { }
    class FooBase : IFoo { }
    class Foo1 : FooBase { }
    public struct MyStruct { }
    public interface ISelfReferencing<T> where T : ISelfReferencing<T> { }
    public class OuterForGenericArg { public class Inner { } }

    public abstract class TestCases // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other types from 8 to the maximum authorized 1 or less.}}
//                        ^^^^^^^^^
    {
        // ================================================================================
        // ==== FIELDS
        // ================================================================================
        private IFoo field1 = new FooBase();
        private FooBase field2 = Method3();
        private static IFoo field3 = Property1;
        private int field4; // Primitives don't count
        private MyStruct str;
        private System.Threading.Tasks.Task myTask;
        Action myAction;
        Func<int> myFunct;
        unsafe int* myPointer;


        // ================================================================================
        // ==== PROPERTIES
        // ================================================================================
        private static Foo1 Property1 { get; }

        public IFoo Property2 { get; set; }

        public IFoo Property3
        {
            get
            {
                return new FooBase();
            }
        }

        public IFoo Property4
        {
            set
            {
                var x = value.ToString();
            }
        }

        public IFoo Property5 => Method3();



        // ================================================================================
        // ==== EVENTS
        // ================================================================================
        public event EventHandler Event1
        {
            add
            {
                var x = Method3();
            }
            remove
            {
                IFoo xx = Method3();
            }
        }



        // ================================================================================
        // ==== CTORS
        // ================================================================================
        public TestCases()
        {
            var x = new object();
            Stream y = new System.IO.FileStream("", System.IO.FileMode.Open);
        }



        // ================================================================================
        // ==== DTORS
        // ================================================================================
        ~TestCases()
        {
            Stream y;
            y = new FileStream("", FileMode.Open);
        }



        // ================================================================================
        // ==== METHODS
        // ================================================================================
        IDisposable Method1(object o)
        {
            Stream y = new FileStream("", FileMode.Open);
            return y;
        }

        Stream Method2() => new FileStream("", FileMode.Open);
        private static FooBase Method3() => null;

        protected abstract IFoo Method4();
    }

    public class OutterClass
    {
        InnerClass whatever = new InnerClass();

        public class InnerClass // Noncompliant
        {
            public Stream stream = new FileStream("", FileMode.Open);
        }
    }

    public class WithConstraint<T> where T : IDisposable { } // Compliant
    public class UnboundGenericUsage // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other types from 2 to the maximum authorized 1 or less.}}
    {
        void M() { _ = typeof(WithConstraint<>); } // +1 WithConstraint<T>, +1 IDisposable (constraint of unbound generic)
    }

    public class ThisMemberAccessUsage // Compliant
    {
        private Stream _s;
        Stream M() => this._s; // coverage: this.X is not a simple name chain and adds no new dependencies
    }

    public class AliasInCastUsage // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other types from 2 to the maximum authorized 1 or less.}}
    {
        private Stream _s;                             // +1 Stream
        void M(object obj) { var x = (MyFileStream)obj; } // +1 FileStream (via alias to BCL type)
    }

    public class AliasForUserDefinedTypeUsage // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other types from 2 to the maximum authorized 1 or less.}}
    {
        private Stream _s;                                // +1 Stream
        void M(object obj) { var x = (FooBaseAlias)obj; } // +1 FooBase (via alias to user-defined type)
    }

    public class GlobalQualifiedCustomTypeUsage // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other types from 2 to the maximum authorized 1 or less.}}
    {
        private Stream _s;                               // +1 Stream
        private global::Tests.Diagnostics.FooBase _fb;  // +1 FooBase (global:: qualified custom type)
    }

    public class ArrayInitializerUsage // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other types from 2 to the maximum authorized 1 or less.}}
    {
        private Stream _s;                             // +1 Stream
        void M() { FooBase[] arr = null; }             // +1 FooBase (array element type via initializer ConvertedType)
    }

    public class PointerInitializerUsage // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other types from 2 to the maximum authorized 1 or less.}}
    {
        private Stream _s;                         // +1 Stream
        unsafe void M() { MyStruct* ptr = null; } // +1 MyStruct (pointer initializer ConvertedType)
    }

    public class SelfReferencingGenericUsage // Compliant
    {
        void M() { _ = typeof(ISelfReferencing<>); } // +1 ISelfReferencing<T>; constraint loops back to ISelfReferencing<T> — deduped, no infinite recursion
    }

    public class SelfReferencingGenericUsage<T> where T : ISelfReferencing<T> // Compliant
    {
        // constraint T : ISelfReferencing<T> adds +1 ISelfReferencing<T>; T is ITypeParameterSymbol — filtered by ExpandGenericTypes, no infinite recursion
    }

    public class NullableWrapperUsage // Compliant
    {
        private MyStruct? field; // +1 MyStruct (Nullable<MyStruct> not counted separately)
    }

    public class ConstraintAndNestedTypeArgUsage<T> // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other types from 3 to the maximum authorized 1 or less.}}
        where T : IFoo // +1 IFoo
    {
        void M()
        {
            _ = EqualityComparer<Dictionary<int, T>>.Default; // +1 EqualityComparer<T>, +1 Dictionary<TKey,TValue> via ExpandGenericTypes (int, T not counted)
        }
    }

    public class GenericMethodTypeArgNotCounted // Compliant: unlike EqualityComparer<T>.Default above, generic method type args are not counted (see PR #1847)
    {
        void M()
        {
            Method<IDisposable>();                           // IDisposable not counted
            this.Method<ICloneable>();                       // ICloneable not counted
            var list = new List<int>();
            list.ConvertAll<IComparable>(x => null);         // IComparable not counted
        }
        void Method<T>() { }
    }

    public class NestedTypeAsGenericArgUsage // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other types from 3 to the maximum authorized 1 or less.}}
    {
        private List<OuterForGenericArg.Inner> field; // +1 List<T>, +1 Inner, +1 OuterForGenericArg (ContainingType of Inner)
    }

    public class ChainedMemberAccessThroughPropertyUsage // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other types from 2 to the maximum authorized 1 or less.}}
    {
        private Stream _s;                                        // +1 Stream
        void M() { System.Console.Out.WriteLine("hello"); }      // +1 Console (Out is a property, not a type — must recurse deeper to find Console)
    }

    public class OuterWithNestedInterface // Compliant: nested interface types are not counted as dependencies of the outer type
    {
        InnerInterface whatever = null;

        interface InnerInterface // Noncompliant {{Split this interface into smaller and more specialized ones to reduce its dependencies on other types from 2 to the maximum authorized 1 or less.}}
        {
            Stream M(FileStream fs); // +1 Stream, +1 FileStream
        }
    }

    public class OuterWithNestedClass // Compliant: nested class types are not counted as dependencies of the outer type
    {
        InnerClass whatever = null;

        class InnerClass // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other types from 2 to the maximum authorized 1 or less.}}
        {
            Stream field = null;    // +1 Stream
            FileStream field2 = null; // +1 FileStream
        }
    }

}
