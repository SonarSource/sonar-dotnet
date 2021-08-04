using System;
using System.IO;

namespace Net5.S2952
{
    public partial class ResourceHolder : IDisposable
    {
        private FileStream fs;

        public void OpenResource(string path) =>
            this.fs = new FileStream(path,
                FileMode.Open);

        public partial void CleanUp();

        public partial void Dispose();
    }

    public partial class ResourceHolder : IDisposable
    {
        public partial void CleanUp() =>
            this.fs.Dispose();

        public partial void Dispose() =>
            this.fs.Dispose();
    }

    public partial class ResourceHolder2 : IDisposable
    {
        public partial void CleanUp() =>
            this.fs.Dispose();

        public partial void Dispose() =>
            this.fs.Dispose();
    }
}
