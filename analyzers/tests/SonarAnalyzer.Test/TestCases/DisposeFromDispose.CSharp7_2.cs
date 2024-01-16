using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;

namespace Tests.Diagnostics
{
    public readonly ref struct ReadonlyDisposableRefStruct
    {
        private readonly FileStream fs;

        public ReadonlyDisposableRefStruct(string path)
        {
            this.fs = new FileStream(path, FileMode.Open);
        }

        public void CloseResource()
        {
            this.fs.Close();
        }

        public void CleanUp()
        {
            this.fs.Dispose(); // Compliant - disposable ref structs came with C# 8
        }

        public void Dispose()
        {
        }
    }

    public ref struct DisposableRefStruct
    {
        private FileStream fs;

        public void OpenResource(string path)
        {
            this.fs = new FileStream(path, FileMode.Open);
        }

        public void CloseResource()
        {
            this.fs.Close();
        }

        public void CleanUp()
        {
            this.fs.Dispose(); // Compliant - disposable ref structs came with C# 8
        }

        public void Dispose()
        {
        }
    }

    public class OuterClass : IDisposable
    {
        private Stream stream;

        class InnerClass : IDisposable
        {
            public void M(OuterClass outter)
            {
                outter.stream.Dispose(); // Compliant
            }

            public void Dispose() { }
        }

        public void Dispose() { }
    }

    public class BaseClass : IDisposable
    {
        protected Stream stream;

        public void Dispose() { }

    }

    public class Derived : BaseClass, IDisposable
    {
        public void Cleanup() => stream.Dispose(); // Compliant
    }

    public class Conditional: IDisposable
    {
        private Stream fs;

        private void MemberBinding()
        {
            fs?.Dispose();                             // Noncompliant {{Move this 'Dispose' call into this class' own 'Dispose' method.}}
        }

        private void ThisMemberBinding()
        {
            this.fs?.Dispose();                        // Noncompliant
        }

        private void ThisAndMemberBinding()
        {
            this?.fs?.Dispose();                       // Noncompliant
        }

        private void Ternary()
        {
            (fs == null ? null : fs)?.Dispose();       // Compliant
        }

        private void InvocationWithoutName()
        {
            Func<Action> f = () => () => fs.Dispose(); // Noncompliant
            f()();                                     // Compliant
        }

        public void Dispose() { }
    }

    public class DisposedInDispose: IDisposable
    {
        Stream fs;

        public void Cleanup() => fs.Dispose(); // Compliant. fs is also disposed in Dispose
        public void Dispose() => fs.Dispose();
    }

    public class DisposedInImplicitDispose : IDisposable
    {
        Stream fs;

        public void Cleanup() => fs.Dispose();
        void IDisposable.Dispose() => fs.Dispose();
    }

    public class OtherDisposedInDispose : IDisposable
    {
        Stream fs1;
        Stream fs2;

        public void Cleanup() => fs1.Dispose(); // Noncompliant
        public void Dispose() => fs2.Dispose();
    }

    public class NotIDisposeDisposeInDispose : IDisposable
    {
        NotIDisposeDisposeInDispose d;
        public void Dispose(bool someParam) { }   // other overload
        public void Cleanup() => d.Dispose();     // Noncompliant
        public void Dispose() => d.Dispose(true);
    }
}
