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

            this.fs.Dispose();
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
}
