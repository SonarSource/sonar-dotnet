using System;

namespace Tests.Diagnostics
{
    public delegate void EventHandler1(object s);
    public delegate int EventHandler2(object s, EventArgs e);
    public delegate void EventHandler3(int sender, EventArgs e);
    public delegate void EventHandler4(object sender, int e);
    public delegate void EventHandler5(object sender, EventArgs args);
    public delegate void EventHandler6(object wrongName, EventArgs e);

    public delegate void potentiallyCorrectEventHandler<T>(object sender, T e);

    public delegate void CorrectEventHandler1(object sender, EventArgs e);
    public delegate void CorrectEventHandler2<T>(object sender, T e) where T : EventArgs;

    public class Foo<TEventArgs> where TEventArgs : EventArgs
    {
        public event EventHandler1 Event1; // Noncompliant {{Change the signature of that event handler to match the specified signature.}}
//                   ^^^^^^^^^^^^^
        public event EventHandler2 Event2; // Noncompliant
        public event EventHandler3 Event3; // Noncompliant
        public event EventHandler4 Event4; // Noncompliant
        public event EventHandler5 Event5; // Noncompliant
        public event EventHandler6 Event6; // Noncompliant
        public event potentiallyCorrectEventHandler<Object> Event7; // Noncompliant

        public event NonExistentType Event8; // Error[CS0246]
        public event NamedType Event9; // Error[CS0066]

        public event EventHandler1 Event1AsProperty // Noncompliant {{Change the signature of that event handler to match the specified signature.}}
//                   ^^^^^^^^^^^^^
        {
            add { }
            remove { }
        }

        public event CorrectEventHandler1 CorrectEvent;
        public event CorrectEventHandler2<EventArgs> CorrectEvent2;
        public event CorrectEventHandler2<TEventArgs> CorrectEvent3;
        public event potentiallyCorrectEventHandler<EventArgs> CorrectEvent4;
        public event potentiallyCorrectEventHandler<TEventArgs> CorrectEvent5;

        public class NamedType { }
    }

    public class Bar<TEventArgs1, TEventArgs2, TParamWithoutConstraint>
        where TEventArgs1 : EventArgs
        where TEventArgs2 : TEventArgs1
    {
        public event EventHandler<TEventArgs1> CorrectEvent1;
        public event EventHandler<TEventArgs2> CorrectEvent2;

        public event EventHandler<TParamWithoutConstraint> IncorrectEvent; // Noncompliant
    }
}

// Reproducer of https://github.com/SonarSource/sonar-dotnet/issues/3453
namespace Repro3453
{
    public class ProviderChangedEventArgs : EventArgs { }

    public delegate void ProviderChangedEventHandler(object sender, ProviderChangedEventArgs args);

    public interface ILocalizationProvider
    {
        event ProviderChangedEventHandler ProviderChanged1; // Noncompliant
        event ProviderChangedEventHandler ProviderChanged2; // Noncompliant
    }

    public abstract class BaseClass
    {
        public abstract event ProviderChangedEventHandler ProviderChanged3; // Noncompliant
        public abstract event ProviderChangedEventHandler ProviderChanged4; // Noncompliant
    }

    public class Repro3453 : BaseClass, ILocalizationProvider
    {
        public event ProviderChangedEventHandler ProviderChanged1;
        public event ProviderChangedEventHandler ProviderChanged2
        {
            add { }
            remove { }
        }

        public override event ProviderChangedEventHandler ProviderChanged3;
        public override event ProviderChangedEventHandler ProviderChanged4
        {
            add { }
            remove { }
        }

        public event ProviderChangedEventHandler NonOverridenNotFromInterfaceEvent1; // Noncompliant
        public event ProviderChangedEventHandler NonOverridenNotFromInterfaceEvent2 // Noncompliant
        {
            add { }
            remove { }
        }
    }

    public class DisposableClass : IDisposable
    {
        public event ProviderChangedEventHandler ProviderChanged1; // Noncompliant
        public event ProviderChangedEventHandler ProviderChanged2 // Noncompliant
        {
            add { }
            remove { }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
