using System;
using System.Collections.Generic;
using System.IO;

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

        public void Dispose()
        {
            this.fs.Dispose();
        }
    }

    public abstract class ResourceHolder3 // Noncompliant
    {
        protected FileStream fs;

        protected ResourceHolder3()
        {
            this.fs = new FileStream(path, FileMode.Open);
        }

        public virtual void Dispose()
        {
        }
    }

    public class ResourceHolder4 : ResourceHolder3 // Compliant; it doesn't have its own field
    {
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

    public interface IService
    {

    }
}
