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

        partial void DoSomething2(int someParam, int y) //Noncompliant {{Rename parameter 'someParam' to 'x'.}}
//                                    ^^^^^^^^^
        {

        }
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
        public override void DoSomethingAbstract(int x, int someParam) //Noncompliant {{Rename parameter 'someParam' to 'y'.}}
        {
            throw new NotImplementedException();
        }

        public override void DoSomethingVirtual(int x, int someParam) //Noncompliant {{Rename parameter 'someParam' to 'y'.}}
        {
            base.DoSomethingVirtual(x, y);
        }
    }

    public abstract class ChildClassLevel2 : ChildClass
    {
        public override void DoSomethingAbstract(int x, int y) //Noncompliant {{Rename parameter 'y' to 'someParam'.}}
        {
            throw new NotImplementedException();
        }

        public override void DoSomethingVirtual(int x, int y) //Noncompliant {{Rename parameter 'y' to 'someParam'.}}
        {
            base.DoSomethingVirtual(x, y);
        }
    }
}
