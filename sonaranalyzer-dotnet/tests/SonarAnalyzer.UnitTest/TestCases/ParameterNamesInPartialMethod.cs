using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public int Compare(InterfaceImplementation a, InterfaceImplementation y) //Noncompliant {{Rename parameter 'a' to 'x' to match the interface declaration.}}
        {
            return 0;
        }
    }
}
