using System;

namespace Tests.Diagnostics
{
    interface A
    {
        void m();
    }

    interface B : A
    {
        void A.m() { Console.WriteLine("interface B"); }
    }

    interface C : A
    {
        void A.m() { Console.WriteLine("interface C"); }
    }

    class D : B, C  // Error [CS8705] Interface member 'A.m()' does not have a most specific implementation. Neither 'B.A.m()', nor 'C.A.m()' are most specific
    {
        static void Main()
        {
            C c = new D();
            c.m();
        }
    }

    public interface IPublicMethodFirst
    {
        public string Method(string value)
        {
            return value;
        }
    }

    public interface IPublicMethodSecond
    {
        public string Method(string value);
//                    ^^^^^^ Secondary
    }

    public interface ICommon : IPublicMethodFirst, IPublicMethodSecond // Noncompliant {{Rename or add member 'Method(string)' to this interface to resolve ambiguities.}}
//                   ^^^^^^^
    {
    }

    public interface IAbstractMethodFirst
    {
        public abstract string AbstractMethod(string value);
    }

    public interface IAbstractMethodSecond
    {
        public abstract string AbstractMethod(string value);
//                             ^^^^^^^^^^^^^^ Secondary
    }

    public interface IAbstractMethodCommon : IAbstractMethodFirst, IAbstractMethodSecond // Noncompliant {{Rename or add member 'AbstractMethod(string)' to this interface to resolve ambiguities.}}
//                   ^^^^^^^^^^^^^^^^^^^^^
    {
    }

    public interface IVirtualMethodFirst
    {
        public virtual string Virtual(string value)
        {
            return value;
        }
    }

    public interface IVirtualMethodSecond
    {
        public virtual string Virtual(string value)
//                            ^^^^^^^ Secondary
        {
            return value;
        }
    }

    public interface IVirtualMethodCommon : IVirtualMethodFirst, IVirtualMethodSecond // Noncompliant {{Rename or add member 'Virtual(string)' to this interface to resolve ambiguities.}}
//                   ^^^^^^^^^^^^^^^^^^^^
    {
    }

    public interface IInternalMethodFirst
    {
        internal string Internal(string value)
        {
            return value;
        }
    }

    public interface IInternalMethodSecond
    {
        internal string Internal(string value)
//                      ^^^^^^^^ Secondary
        {
            return value;
        }
    }

    public interface IInternalMethodCommon : IInternalMethodFirst, IInternalMethodSecond // Noncompliant {{Rename or add member 'Internal(string)' to this interface to resolve ambiguities.}}
//                   ^^^^^^^^^^^^^^^^^^^^^
    {
    }

    public interface IProtectedMethodFirst
    {
        protected string Protected(string value)
        {
            return value;
        }
    }

    public interface IProtectedMethodSecond
    {
        protected string Protected(string value)
//                       ^^^^^^^^^ Secondary
        {
            return value;
        }
    }

    public interface IProtectedMethodCommon : IProtectedMethodFirst, IProtectedMethodSecond // Noncompliant {{Rename or add member 'Protected(string)' to this interface to resolve ambiguities.}}
//                   ^^^^^^^^^^^^^^^^^^^^^^
    {
    }

    public interface IPrivateMethodFirst
    {
        private string Private(string value)
        {
            return value;
        }
    }

    public interface IPrivateMethodSecond
    {
        private string Private(string value)
//                     ^^^^^^^ Secondary
        {
            return value;
        }
    }

    public interface IPrivateMethodCommon : IPrivateMethodFirst, IPrivateMethodSecond // Noncompliant - FP: Issue is raised but since the methods are not accessible on the derived classes or interfaces it doesn't make sense.
//                   ^^^^^^^^^^^^^^^^^^^^
    {
    }

    public interface ISealedMethodFirst
    {
        public sealed string Sealed(string value)
        {
            return value;
        }
    }

    public interface ISealedMethodSecond
    {
        public sealed string Sealed(string value)
//                           ^^^^^^ Secondary
        {
            return value;
        }
    }

    public interface ISealedMethodCommon : ISealedMethodFirst, ISealedMethodSecond // Noncompliant
//                   ^^^^^^^^^^^^^^^^^^^
    {
    }

    public interface IStaticMethodFirst
    {
        public static string Static(string value)
        {
            return value;
        }
    }

    public interface IStaticMethodSecond
    {
        public static string Static(string value)
//                           ^^^^^^ Secondary
        {
            return value;
        }
    }

    public interface IStaticMethodDerived : IStaticMethodFirst, IStaticMethodSecond // Noncompliant - FP: static methods are not inherited
    {
    }

    public interface IBase1
    {
        public static string PublicStaticField;
    }

    public interface IBase2
    {
        public static string PublicStaticField;
    }

    public interface IDerived : IBase1, IBase2
    {
    }

    public interface IIndexer1
    {
        public string this[int i]
        {
            get { return ""; }
            set { }
        }
    }

    public interface IIndexer2
    {
        public string this[int i] // Rule is raised but the message is not clear
        {
            get { return ""; } // Noncomliant
//          ^^^ Secondary
            set { }
//          ^^^ Secondary
        }
    }

    public interface IIndexerDerived : IIndexer1, IIndexer2 // Noncompliant {{Rename or add members '' and '' to this interface to resolve ambiguities.}}
//                   ^^^^^^^^^^^^^^^
    {
    }

    public interface IProperty1
    {
        public string Property
        {
            get => string.Empty;
            set => Console.WriteLine(value);
        }
    }

    public interface IProperty2
    {
        public string Property
        {
            get => string.Empty;
//          ^^^ Secondary
            set => Console.WriteLine(value);
//          ^^^ Secondary
        }
    }

    public interface IPropertyDerived : IProperty1, IProperty2 // Noncompliant {{Rename or add members 'Property.get' and 'Property.set' to this interface to resolve ambiguities.}}
//                   ^^^^^^^^^^^^^^^^
    {
    }

    public interface IEvents1
    {
        public event EventHandler Click;

        public event EventHandler OnClick
        {
            add
            {
                Click += value;
            }
            remove
            {
                Click -= value;
            }
        }
    }

    public interface IEvents2
    {
        public event EventHandler Click;
//                                ^^^^^ Secondary

        public event EventHandler OnClick
        {
            add
//          ^^^ Secondary
            {
                Click += value;
            }
            remove
            {
                Click -= value;
            }
        }
    }

    public interface IEventsDerived : IEvents1, IEvents2 // Noncompliant {{Rename or add members 'Click' and 'OnClick' to this interface to resolve ambiguities.}}
    {
    }
}
