using System;
using System.Collections.Generic;
using System.IO;

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

}
