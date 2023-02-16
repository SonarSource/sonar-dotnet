using System;
using System.Collections.Generic;
using System.IO;

namespace Tests.Diagnostics
{
    public class ResourceHolder : IDisposable
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
            this.fs.Dispose(); // Noncompliant {{Move this 'Dispose' call into this class' own 'Dispose' method.}}
//                  ^^^^^^^
        }

        public void Dispose()
        {
            // method added to satisfy demands of interface
        }
    }
    public class NonDisposable
    {
        private FileStream fs;
        public void OpenResource(string path)
        {
            this.fs = new FileStream(path, FileMode.Open);
        }

        public void CleanUp()
        {
            this.fs.Dispose(); // Compliant; class is not IDisposable
        }
    }

    public class ResourceHolder2 : IDisposable
    {
        private FileStream fs;
        public void OpenResource(string path)
        {
            this.fs = new FileStream(path, FileMode.Open);
        }
        public void CloseResource()
        {
            var stream = new MemoryStream();
            stream.Dispose();

            this.fs.Close();
        }
        public void CloseResource2()
        {
            var stream = new MemoryStream();
            stream.Dispose();

            this.fs.Close();
        }

        public void Dispose()
        {
            var a = new Action(() =>
            {
                this.fs.Dispose(); //Noncompliant
            });
        }
    }
    public class Class : IDisposable
    {
        FileStream fs;

        public Class()
        {
            fs = new FileStream("", FileMode.Append);
        }

        void IDisposable.Dispose()
        {
            fs.Dispose(); // Compliant, do not report here
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
            this.fs.Dispose(); // Noncompliant
        }

        public void Dispose()
        {
        }
    }

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
            this.fs.Dispose(); // Noncompliant
        }

        public void Dispose()
        {
        }
    }

    public ref struct FakeDisposableRefStruct
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
            this.fs.Dispose(); // Compliant - the Dispose method is not accessible
        }

        private void Dispose()
        {
        }
    }

    public struct DisposableStruct : IDisposable
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
            this.fs.Dispose(); // Noncompliant
        }

        public void Dispose()
        {
            // method added to satisfy demands of interface
        }
    }

    public struct DisposableStructCorrect : IDisposable
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

        public void Dispose()
        {
            this.fs.Dispose(); // compliant
        }
    }

    public ref struct MyTestDisposableRefStruct
    {
        public void Dispose()
        {
        }
    }

    public ref struct RefStructResourceHolder
    {
        // ref structs can only be fields in other ref structs

        private MyTestDisposableRefStruct foo;

        public void OpenResource(string path)
        {
            this.foo = new MyTestDisposableRefStruct();
        }

        public void CleanUp()
        {
            this.foo.Dispose(); // Noncompliant
        }

        public void Dispose()
        {
            // this method makes this struct a disposable ref struct in C# 8
        }
    }

    public ref struct NotDisposableRefStruct
    {
        public void Dispose(bool shouldDispose)
        {
        }
        public string Dispose()
        {
            return "";
        }
    }

    public ref struct NotDisposableRefStructHolder1
    {
        private NotDisposableRefStruct foo;

        public void OpenResource(string path)
        {
            this.foo = new NotDisposableRefStruct();
        }

        public void CleanUp()
        {
            this.foo.Dispose(true); // Ok
        }
        public void Dispose()
        {
        }
    }

    public ref struct NotDisposableRefStructHolder2
    {
        private NotDisposableRefStruct foo;

        public void OpenResource(string path)
        {
            this.foo = new NotDisposableRefStruct();
        }

        public void CleanUp()
        {
            var x = foo.Dispose(); // Ok
        }
        public void Dispose()
        {
        }
    }



    public ref struct AnotherDisposableRefStruct
    {
        public void Dispose() { }
        public void Dispose(bool x) { }
        public void Dispose<T>() { }
        public void AnotherMethod() { }
    }

    public ref struct AnotherDisposableRefStructHolder1
    {
        private AnotherDisposableRefStruct foo;

        public void Cleanup()
        {
            this.foo.Dispose(); // Noncompliant
        }

        public void Dispose()
        {
            foo.Dispose(true);
        }
    }

    public ref struct AnotherDisposableRefStructHolder2
    {
        private AnotherDisposableRefStruct foo;

        public void Cleanup()
        {
            this.foo.AnotherMethod(); // ok
        }

        public void Dispose()
        {
        }
    }

    public ref struct AnotherDisposableRefStructHolder3
    {
        private AnotherDisposableRefStruct foo;

        public void Cleanup()
        {
            this.foo.Dispose(true); // ok
        }

        public void Dispose()
        {
        }
    }

    public ref struct AnotherDisposableRefStructHolder4
    {
        private AnotherDisposableRefStruct foo;

        public void Cleanup()
        {
            this.foo.Dispose<string>(); // ok
        }

        public void Dispose()
        {
        }
    }

    public class NullSupression: IDisposable
    {
        private Stream fs;

        private void NullSupressionOperator()
        {
            fs!.Dispose();  // Noncompliant {{Move this 'Dispose' call into this class' own 'Dispose' method.}}
        }

        private void NullSupressionAndNullCoalescing()
        {
            fs!?.Dispose(); // Noncompliant
        }

        private void ThisNullSupressionAndNullCoalescing()
        {
            this!.fs?.Dispose(); // Noncompliant
        }

        public void Dispose() { }

    }
}
