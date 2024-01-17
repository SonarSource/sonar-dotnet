using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


namespace Tests.Diagnostics
{
    public class ResourceHolder   // Noncompliant {{Implement 'IDisposable' in this class and use the 'Dispose' method to call 'Dispose' on 'fs'.}}
//               ^^^^^^^^^^^^^^
    {
        private FileStream fs;  // This member is never Disposed
        public void OpenResource(string path)
        {
            this.fs = new FileStream(path, FileMode.Open);
        }
        public void CloseResource()
        {
            this.fs.Close();
        }
    }

    public class ResourceHolder2 : IDisposable
    {
        protected FileStream fs;
        public void OpenResource(string path)
        {
            this.fs = new FileStream(path, FileMode.Open);
        }
        public void CloseResource()
        {
            this.fs.Close();
        }

        public void Dispose() // FN, it does not dispose fs
        {
        }
    }

    public abstract class ResourceHolder3 // Noncompliant
    {
        protected FileStream fs;

        protected ResourceHolder3(string path)
        {
            this.fs = new FileStream(path, FileMode.Open);
        }

        public virtual void Dispose()
        {
        }
    }

    public class ResourceHolder4 : ResourceHolder3 // Compliant; it doesn't have its own field
    {
        ResourceHolder4(string path) : base(path) { }
        public void OpenResource(string path)
        {
            this.fs = new FileStream(path, FileMode.Open);
        }
        public void CloseResource()
        {
            this.fs.Close();
        }
    }

    public class ResourceHolder5 : ResourceHolder2 //Compliant
    {
    }

    public class ObserverVsOwner // Noncompliant {{Implement 'IDisposable' in this class and use the 'Dispose' method to call 'Dispose' on 'fs2' and 'fs3'.}}
    {
        protected FileStream fs, fs2 = new FileStream("eee", FileMode.Open), fs3; // Only fs will be compliant
        public FileStream Stream
        {
            get { return fs; }
            set { fs = value; }
        }
        public ObserverVsOwner(string path)
        {
            fs3 = new FileStream(path, FileMode.Open);
        }
    }

    public class TestWithTasks // Noncompliant {{Implement 'IDisposable' in this class and use the 'Dispose' method to call 'Dispose' on 't2'.}}
    {
        // Tasks are IDisposable who usually don't really need to be disposed, but they are typicall create with a factory
        Task t1 = Task.Run(() => { Console.WriteLine("Who did that?"); });
        Task t2 = new Task(() => { Console.WriteLine("Not me!"); });
    }

    public class ExpressionBodied // Noncompliant {{Implement 'IDisposable' in this class and use the 'Dispose' method to call 'Dispose' on 'fs'.}}
    {
        protected FileStream fs;
        ExpressionBodied(int i) => fs = new FileStream("eee", FileMode.Open);
    }

    public interface IService
    {

    }

    public ref struct DisposableStruct // Compliant - FN
    {
        private FileStream fs;  // This member is never Disposed

        public void OpenResource(string path)
        {
            this.fs = new FileStream(path, FileMode.Open);
        }
    }

    //See https://github.com/SonarSource/sonar-dotnet/issues/2957
    public class Repro_2957 // Noncompliant {{Implement 'IDisposable' in this class and use the 'Dispose' method to call 'Dispose' on '_disposable'.}}
    {
        private readonly IDisposable _disposable;

        public Repro_2957()
        {
            _disposable = new DisposableStuff();
        }

        private sealed class DisposableStuff : IDisposable
        {
            public void Dispose() { }
        }
    }

    partial class PartialMethod1 : IDisposable
    {
        private readonly IDisposable _disposable = new FileStream("a", FileMode.Open);
        public void Dispose()
        {
            MyDispose();
        }
        partial void MyDispose();
    }

    partial class PartialMethod1 : IDisposable
    {
        partial void MyDispose()
        {
            _disposable.Dispose();
        }
    }

    partial class PartialMethod2 // Noncompliant
    {
        private readonly IDisposable _disposable = new FileStream("a", FileMode.Open);
        public void Dispose()
        {
            MyDispose();
        }
        partial void MyDispose();
    }

    partial class PartialMethod2 // Noncompliant
    {
        partial void MyDispose()
        {
            _disposable.Dispose();
        }
    }

    partial class PartialMethod3 : IDisposable
    {
        private readonly IDisposable _disposable = new FileStream("a", FileMode.Open);
        public void Dispose()
        {
            MyDispose();
        }
        partial void MyDispose();
    }

    partial class PartialMethod3
    {
        partial void MyDispose()
        {
            _disposable.Dispose();
        }
    }

}
