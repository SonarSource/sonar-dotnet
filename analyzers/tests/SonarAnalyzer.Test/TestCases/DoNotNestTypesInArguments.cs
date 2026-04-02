using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

using ComplexType = System.Collections.Generic.ICollection<System.Collections.Generic.ICollection<System.Collections.Generic.ICollection<int>>>;

public class GlobalClass<T> { }

namespace MyLibrary
{
    public class Foo
    {
        public delegate void Delegate(IList<IList<int>> arg);

        public void DoSomething(ICollection<ICollection<int>> outerCollect) { } // Noncompliant {{Refactor this method to remove the nested type argument.}}
//                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        public void Method_NoArgs() { }
        public void Method_Array(object[] arg) { }
        public void GenericMethod_Array<T>(T[] arg) { }
        public void Method_ListOfArrays(List<object[]> arg) { }
        public void Method_ArrayOfLists(List<object>[] arg) { }

        public void Method_DeepGeneric<T>(Tuple<object, Tuple<List<T>, object>> arg) { } // Noncompliant
        public void Method_DeepMethod(Tuple<object, List<List<object>>> arg) { } // Noncompliant

        public void Method_MultiArgs(List<List<object>> arg1,
//                                   ^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
                                     List<object> arg2,
                                     List<object[]>[] arg3,
                                     Tuple<List<object>, object> arg4) { }
//                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
        public void Method_ArraysAndGenerics(ICollection<ICollection<ICollection<int>[]>[]> arg) { }
//                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant

        public void Method_NestedExpression(Expression<ICollection<int>[]>[] arg) { }

        public void Method_NestedFunc(Func<ICollection<int>> arg) { }
        public void Method_NestedFunc1(Func<ICollection<int>, ICollection<int>> arg) { }
        public void Method_NestedFunc2(Func<ICollection<int>, ICollection<int>, ICollection<int>> arg) { }
        public void Method_NestedFunc3(Func<ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>> arg) { }
        public void Method_NestedFunc4(Func<ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>> arg) { }
        public void Method_NestedFunc16(Func<ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>> arg) { }
        public void Method_NestedFuncArrays(Func<ICollection<int>[], List<object>>[] arg) { }

        public void Method_NestedFunc_Level2(Func<ICollection<ICollection<int>>> arg) { } // Noncompliant

        public void Method_NestedAction(Action arg) { }
        public void Method_NestedAction1(Action<ICollection<int>> arg) { }
        public void Method_NestedAction2(Action<ICollection<int>, ICollection<int>> arg) { }
        public void Method_NestedAction3(Action<ICollection<int>, ICollection<int>, ICollection<int>> arg) { }
        public void Method_NestedAction4(Action<ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>> arg) { }
        public void Method_NestedAction16(Action<ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>, ICollection<int>> arg) { }
        public void Method_NestedActionArrays(Action<ICollection<int>[], List<object>>[] arg) { }

        public void Method_NestedAction_Level2(Action<ICollection<ICollection<int>>> arg) { } // Noncompliant

        public void Method_AnonymousMethod()
        {
            Delegate x = delegate (IList<IList<int>> arg) { };
        }

        public void Method_Tuple((List<int>, List<string>) arg) { }
        public void Method_TupleList(List<(int, string)> arg) { }
        public void Method_TupleNested((Task<List<int>>, Task<List<int>>) data) { } // Noncompliant
        public void Method_TupleListNested(List<(Task<int>, Task<string>)> args) { } //Noncompliant

        public void Method_Alias(global::GlobalClass<GlobalClass<int>> arg) { } // Noncompliant
        public void Method_QualifiedName(System.Collections.Generic.List<System.Collections.Generic.List<int>> arg) { } // Noncompliant
        public void Method_QualifiedNameWithAlias(global::System.Collections.Generic.List<IEnumerable<int>> arg) { } // Noncompliant

        public void Method_Generic<T>(List<List<List<T>>> arg) { } // Noncompliant

        public void Method_WithUsing(ComplexType arg) { } // Compliant, alias is used to hide complexity

        public void Method_Out(out ICollection<ICollection<int>> arg) { arg = null; } // Noncompliant
        public void Method_Out_Compliant(out ICollection<int> arg) { arg = null; }
    }

    public class WithLocalFunctions
    {
        public void Method()
        {
            void DoSomething(ICollection<ICollection<int>> outerCollect) { } // Noncompliant
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3277
    public class Repro_3277
    {
        public void QuestionMark(List<int?> list)
        {
        }

        public void Explicit(List<Nullable<int>> list) // Noncompliant, there's a nice way out of this
        {
        }
    }

    // https://community.sonarsource.com/t/s3342-and-s4017-complains-on-user-code-while-implementing-external-interface/120432
    namespace Repro_120432
    {
        public abstract class External
        {
            public abstract void DoWork(IDictionary<String, IList<string>> data); // Noncompliant
        }

        public class UserCode : External
        {
            public override void DoWork(IDictionary<String, IList<string>> data) { }
        }
    }

    namespace Inheritance
    {
        public interface IAbstractClassInterface
        {
            void AbstractInterfaceMethod(Task<IList<int>> data); // Noncompliant
        }

        public interface IImplicit
        {
            void ImplicitMethod(Task<IList<int>> data); // Noncompliant
        }

        public interface IExplicit
        {
            void ExplicitMethod(Task<IList<int>> data); // Noncompliant
        }

        public abstract class AbstractClass : IAbstractClassInterface
        {
            public abstract void AbstractInterfaceMethod(Task<IList<int>> data);

            public abstract void AbstractClassMethod(Task<IList<int>> data); // Noncompliant

            public virtual void VirtualMethod(Task<IList<int>> data) { } // Noncompliant

            public void ShadowedMethod(Task<IList<int>> data) { } // Noncompliant
        }

        public class DerivedClass : AbstractClass, IImplicit, IExplicit
        {
            public void ImplicitMethod(Task<IList<int>> data) { }

            void IExplicit.ExplicitMethod(Task<IList<int>> data) { }

            public override void AbstractInterfaceMethod(Task<IList<int>> data) { }

            public override void AbstractClassMethod(Task<IList<int>> data) { }

            public override void VirtualMethod(Task<IList<int>> data) { } // Noncompliant

            public new void ShadowedMethod(Task<IList<int>> data) { } // Noncompliant, 'new' hides the base method but does not force this signature.
        }

        public struct Struct : IImplicit, IExplicit
        {
            public void ImplicitMethod(Task<IList<int>> data) { }

            void IExplicit.ExplicitMethod(Task<IList<int>> data) { }
        }
    }

    namespace Operators
    {
        class Noncompliant<T>
        {
            public static implicit operator Noncompliant<T>(List<List<List<T>>> value) // Noncompliant
            {
                throw new NotImplementedException();
            }

            public static explicit operator Noncompliant<T>(Noncompliant<List<T>> obj) // Noncompliant
            {
                throw new NotImplementedException();
            }
        }

        class Compliant<T>
        {
            public static implicit operator Compliant<T>(List<T> value)
            {
                throw new NotImplementedException();
            }

            public static explicit operator Compliant<T>(HashSet<T> obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}
