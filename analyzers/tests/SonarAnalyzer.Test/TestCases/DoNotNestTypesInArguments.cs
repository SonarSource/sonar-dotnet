using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using ComplexType = System.Collections.Generic.ICollection<System.Collections.Generic.ICollection<System.Collections.Generic.ICollection<int>>>;

namespace MyLibrary
{
    public class Foo
    {
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

        public void Method_WithUsing(ComplexType arg) { } // Compliant, alias is used to hide complexity
    }

    public class WithLocalFunctions
    {
        public void Method()
        {
            void DoSomething(ICollection<ICollection<int>> outerCollect) { } // Noncompliant

            static void DoSomethingElse(ICollection<ICollection<int>> outerCollect) { } // Noncompliant
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
            public override void DoWork(IDictionary<String, IList<string>> data) // Noncompliant FP
            {
                throw new NotImplementedException();
            }
        }
    }
}
