using System;
using System.Collections.Generic;

namespace Records
{
    public abstract record Empty { }

    public abstract record Animal // Noncompliant {{Convert this 'abstract' record to an interface.}}
    {
        public abstract void move();
        public abstract void feed();
    }

    public record SomeBaseRecord { }

    public abstract record Animal2 : SomeBaseRecord // Compliant
    {
        public abstract void move();
        public abstract void feed();
    }

    public abstract record RecordWithProtectedAbstractMethod // Noncompliant
    {
        protected abstract void ProtectedMethod();
    }

    public abstract record Color
    {
        private int red = 0;
        public int getRed() => red;
    }

    public interface AnimalCompliant
    {
        void move();
        void feed();
    }

    public class ColorCompliant
    {
        private int red = 0;

        private ColorCompliant()
        { }

        public int getRed() => red;
    }

    public abstract record LampCompliant
    {

        private bool switchLamp = false;

        public abstract void glow();

        public void flipSwitch()
        {
            switchLamp = !switchLamp;
            if (switchLamp)
            {
                glow();
            }
        }
    }

    public abstract record View // Noncompliant {{Convert this 'abstract' record to an interface.}}
    //                     ^^^^
    {
        public abstract string Content { get; }
    }

    public abstract record View2() // Compliant, has abstract and non abstract members
    {
        public abstract string Content { get; }
        public abstract string Content1 { get; }
        public string Content2 { get; }
    }

    public abstract record Record(string X);

    public abstract record Record2(string X) // Compliant, this record has a propery X which is concrete
    {
        public abstract string Content { get; }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/9494
    public abstract class AbstractClassWithStaticField      // TN for .NET Framework, FN for .NET Core / .NET (where interfaces can have static members)
    {
        protected static int _data;
        public abstract void SomeMethod();
    }
}

namespace FileAccessibility
{
    file abstract class Empty
    {
    }

    file abstract class OnlyAbstract    // Noncompliant {{Convert this 'abstract' class to an interface.}}
    //                  ^^^^^^^^^^^^
    {
        public abstract void Move();
    }

    file abstract class Animal2 //Compliant
    {
        public abstract void Move();
        string Foo() => "FOO";
    }
}

namespace PartialProperties
{
    public abstract partial class PartialPropertyAbstractOnly //Noncompliant {{Convert this 'abstract' class to an interface.}}
    //                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^
    {
        public abstract string Name { get; }
    }

    public abstract partial class PartialPropertyPartial
    {
        public partial string Name { get; }
    }
}

namespace Events
{
    public abstract class EventTest
    {
        public event System.EventHandler<System.EventArgs> AbstractEvent { add { } remove { } }
    }

    public abstract class AbstractEventTest
    {
        public abstract event System.EventHandler<System.EventArgs> AbstractEvent;
    }
}

namespace CSharp14
{
    public abstract partial class PartialConstructor
    {
        public partial PartialConstructor();
    }

    public abstract partial class PartialEventTest
    {
        public partial event System.EventHandler<System.EventArgs> PartialEvent;
    }
}

namespace CSharp15
{
    public closed class ClosedAllAbstract // Compliant NET-4873 - interfaces cannot be closed
    {
        public abstract void Move();
    }

    public class ClosedAllAbstractDerived : ClosedAllAbstract
    {
        public override void Move() { }
    }

    public abstract class NotClosedAllAbstract // Noncompliant
    {
        public abstract void Move();
    }

    public closed record ClosedAllAbstractRecord // Compliant
    {
        public abstract void Move();
    }

    public abstract record NotClosedAllAbstractRecord // Noncompliant
    {
        public abstract void Move();
    }

    public closed partial class ClosedPartial // Compliant
    {
        public abstract void Move();
    }

    public partial class ClosedPartial // Compliant
    {
        public abstract void Stop();
    }

    public abstract partial class NotClosedPartial // Noncompliant
    {
        public abstract void Move();
    }

    public partial class NotClosedPartial // Noncompliant - raised on every partial declaration
    {
        public abstract void Stop();
    }

    public partial class ClosedOnSecondPartial // Compliant
    {
        public abstract void Move();
    }

    public closed partial class ClosedOnSecondPartial // Compliant
    {
        public abstract void Stop();
    }

    public closed class ClosedWithBase : NotClosedAllAbstract // Compliant
    {
        public abstract void Stop();
    }

    public abstract class NotClosedWithBase : NotClosedAllAbstract // Compliant - base type is not object
    {
        public abstract void Stop();
    }
}
