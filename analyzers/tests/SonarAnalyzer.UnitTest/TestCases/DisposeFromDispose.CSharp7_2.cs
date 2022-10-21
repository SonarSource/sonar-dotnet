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

    public class OutterClass : IDisposable
    {
        private Stream stream;

        class InnerClass : IDisposable
        {
            public void M(OutterClass outter)
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
            fs?.Dispose();                       // Noncompliant
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

    public class DisposedInDisposed: IDisposable
    {
        Stream fs;

        public void Cleanup() => fs.Dispose();
        public void Dispose() => fs.Dispose();
    }
}
