using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public partial class ParameterNamesInPartialMethod
    {
        partial void DoSomething(int x, int y);
        partial void DoSomething2(int x, int y);

        partial void DoSomething3(int x, int y);

        public void DoSomething4(int x, int y)
        {
        }
    }

    public partial class ParameterNamesInPartialMethod
    {
        partial void DoSomething(int x, int y)
        {

        }

        partial void DoSomething2(int someParam, int y) //Noncompliant {{Rename parameter 'someParam' to 'x' to match the partial class declaration.}}
//                                    ^^^^^^^^^
        {

        }

        partial void DoSomething3(int x, int y, int z);
    }

    public abstract class BaseClass
    {
        public virtual void DoSomethingVirtual(int x, int y)
        {
        }

        public abstract void DoSomethingAbstract(int x, int y);
    }

    public class ChildClass : BaseClass
    {
        public override void DoSomethingAbstract(int x, int someParam) //Noncompliant {{Rename parameter 'someParam' to 'y' to match the base class declaration.}}
        {
            throw new NotImplementedException();
        }

        public override void DoSomethingVirtual(int x, int someParam) //Noncompliant {{Rename parameter 'someParam' to 'y' to match the base class declaration.}}
        {
            base.DoSomethingVirtual(x, someParam);
        }
    }

    public abstract class ChildClassLevel2 : ChildClass
    {
        public override void DoSomethingAbstract(int x, int y) //Noncompliant {{Rename parameter 'y' to 'someParam' to match the base class declaration.}}
        {
            throw new NotImplementedException();
        }

        public override void DoSomethingVirtual(int x, int y) //Noncompliant {{Rename parameter 'y' to 'someParam' to match the base class declaration.}}
        {
            base.DoSomethingVirtual(x, y);
        }
    }

    public class InterfaceImplementation : IComparer<InterfaceImplementation>
    {
        public int Compare(InterfaceImplementation a, InterfaceImplementation y)
        {
            return 0;
        }
    }

    public partial interface IParameterNamesInPartialMethods
    {
        partial void DoSomething(int x, int y);
        partial void DoSomething2(int x, int y);
    }

    public partial interface IParameterNamesInPartialMethods
    {
        partial void DoSomething(int x, int y)
        {
        }

        partial void DoSomething2(int someParam, int y) //Noncompliant {{Rename parameter 'someParam' to 'x' to match the partial class declaration.}}
//                                    ^^^^^^^^^
        {
        }
    }

    public interface IGenericInterface<A>
    {
        void DoSomething();
        void DoSomething(A value);
        void DoSomething(A value, int intValue);
        void DoSomethingElse(A value);
        void DoSomethingElse(A value, ParameterClass parameterClassValue);
        void TryOneMoreTime(AnotherParameterClass value);
        void DoSomethingCaseSensitive(A value, int intValue);
    }
    public class ParameterClass { }
    public class AnotherParameterClass { }
    public class Implementation : IGenericInterface<ParameterClass>
    {
        public void DoSomething() { }
        public void DoSomething(ParameterClass parameter) { }
        public void DoSomething(AnotherParameterClass randomName) { }
        public void DoSomethingElse(ParameterClass completelyAnotherName) { }
        public void DoSomething(ParameterClass value, int myValue) { }                // Noncompliant
//                                                        ^^^^^^^
        public void DoSomethingElse(ParameterClass value, ParameterClass val) { }     // Noncompliant
//                                                                       ^^^
        public void TryOneMoreTime(AnotherParameterClass anotherParameter) { }        // Noncompliant
//                                                       ^^^^^^^^^^^^^^^^
        public void DoSomethingCaseSensitive(ParameterClass Value, int IntValue) { }  // Noncompliant
//                                                                     ^^^^^^^^
    }

    public struct StructImplementation : IGenericInterface<ParameterClass>
    {
        public void DoSomething() { }
        public void DoSomething(ParameterClass parameter) { }
        public void DoSomething(AnotherParameterClass randomName) { }
        public void DoSomethingElse(ParameterClass completelyAnotherName) { }
        public void DoSomething(ParameterClass value, int myValue) { }             // Noncompliant
//                                                        ^^^^^^^
        public void DoSomethingElse(ParameterClass value, ParameterClass val) { }  // Noncompliant
//                                                                       ^^^
        public void TryOneMoreTime(AnotherParameterClass anotherParameter) { }     // Noncompliant
//                                                       ^^^^^^^^^^^^^^^^
        public void DoSomethingCaseSensitive(ParameterClass Value, int IntValue) { }  // Noncompliant
//                                                                     ^^^^^^^^
    }

    public abstract class BaseClass<T>
    {
        public abstract void SomeMethod(T someParameter);

        public abstract void SomeMethod(T someParameter, int intParam);
    }

    public class ClassOne : BaseClass<int>
    {
        public override void SomeMethod(int renamedParam) { }

        public override void SomeMethod(int someParameter, int renamedParam) {  }  // Noncompliant
//                                                             ^^^^^^^^^^^^
    }

    public abstract class AbstractClassWithGenericMethod
    {
        abstract public void Foo<T>(T val);
        abstract public void Bar<T>(T val);
    }

    public class InheritedClassWithDefinition : AbstractClassWithGenericMethod
    {
        public override void Foo<T>(T myNewName) { }                               // Noncompliant
//                                    ^^^^^^^^^
        public override void Bar<T>(T val) { }
    }

    public interface IAnotherGenericInterface<A>
    {
        void DoSomething(A value);
        void DoSomething(A value, int intValue);
    }

    public interface IAnotherInterface : IAnotherGenericInterface<ParameterClass>
    {
        void DoSomethingElse(ParameterClass value);
    }

    public abstract class AnotherAbstractClass : IAnotherInterface
    {
        public abstract void DoSomething(ParameterClass abstractValue);
        public abstract void DoSomething(ParameterClass value, int IntValue);            //Noncompliant {{Rename parameter 'IntValue' to 'intValue' to match the interface declaration.}}
//                                                                 ^^^^^^^^
        public abstract void DoSomethingElse(ParameterClass Value);                      //Noncompliant {{Rename parameter 'Value' to 'value' to match the interface declaration.}}
//                                                          ^^^^^
    }

    public class AnotherImplementation : AnotherAbstractClass
    {
        public override void DoSomething(ParameterClass value) { }                       //Noncompliant {{Rename parameter 'value' to 'abstractValue' to match the base class declaration.}}
//                                                      ^^^^^
        public override void DoSomething(ParameterClass abstractValue, int IntValue) { } //Noncompliant {{Rename parameter 'abstractValue' to 'value' to match the base class declaration.}}
//                                                      ^^^^^^^^^^^^^
        public override void DoSomethingElse(ParameterClass Value) { }
    }

    // See https://github.com/SonarSource/sonar-dotnet/issues/4370
    public interface SomeInterface<A> { }

    public interface BaseInterface<A>
    {
        void Apply(SomeInterface<A> param);
    }

    public class BasicImplementation : BaseInterface<int>
    {
        public void Apply(SomeInterface<int> intValueParam) { }
    }

    public class StillGeneric<T> : BaseInterface<T>
    {
        public void Apply(SomeInterface<T> renamedParam) { }  // Noncompliant
    }

    public abstract class AbstractClass<T>
    {
        public abstract void Apply(SomeInterface<T> intValueParam);
    }

    public class OverridenCompliant : AbstractClass<int>
    {
        public override void Apply(SomeInterface<int> intValueParam) { }
    }

    public class OverridenNonCompliant<K> : AbstractClass<K>
    {
        public override void Apply(SomeInterface<K> renamedParam) { } // Noncompliant
    }

    public interface BaseInterface2<A, B>
    {
        void Apply(Tuple<A, B> param);
    }

    public class SomeClass<T> : BaseInterface2<T, int>
    {
        public void Apply(Tuple<T, int> renamed) { }
    }

    public abstract class BaseRepro
    {
        public abstract int SomeMethod(int SomeParam1, int SomeParam2);
    }

    public class Reproducer : BaseRepro
    {
        public override int SomeMethod(int _, int SomeParam2) => SomeParam2; // Noncompliant, it is not a discard
    }
}
