using System;
using System.Collections.Generic;
using System.IO;

FileStream fs; // top level statement

void CleanUp()
{
    fs.Dispose();
}

record R1 : IDisposable
{
    private FileStream fs;

    public void OpenResource(string path) => this.fs = new FileStream(path, FileMode.Open);
    public void CloseResource() => this.fs.Close();
    public void CleanUp() => this.fs.Dispose(); // Noncompliant {{Move this 'Dispose' call into this class' own 'Dispose' method.}}

    public void Dispose() { }
}

record R2 : IDisposable
{
    private FileStream fs;

    public void OpenResource(string path) => this.fs = new FileStream(path, FileMode.Open);
    public void Dispose() => this.fs.Dispose(); // Compliant
}

public ref partial struct DisposableRefStruct1
{
    private FileStream fs;
    public void OpenResource(string path) => this.fs = new FileStream(path, FileMode.Open);
    public void CleanUp() => this.fs.Dispose(); // Noncompliant

    public partial void Dispose();
}

public ref partial struct DisposableRefStruct1
{
    public partial void Dispose() { }
}

public ref partial struct DisposableRefStruct2
{
    private FileStream fs;
    public void OpenResource(string path) => this.fs = new FileStream(path, FileMode.Open);
    public void CleanUp() => this.fs.Dispose(); // Noncompliant
    public partial void Dispose();
}

public ref partial struct DisposableRefStruct2
{
    public partial void Dispose() => this.fs.Dispose(); // Compliant
}

public partial class ResourceHolder : IDisposable
{
    public partial void CleanUp() =>
        this.fs.Dispose(); // Compliant. See also test case DisposedInDispose in CSharp7_2

    public partial void Dispose() =>
        this.fs.Dispose();
}

public partial class ResourceHolder2 : IDisposable
{
    public void CleanUp() =>
        this.fs.Dispose(); // Compliant. See also test case DisposedInDispose in CSharp7_2
}

public partial class ResourceHolder3 : IDisposable
{
    public partial void CleanUp() =>
        this.fs.Dispose(); // Compliant. See also test case DisposedInDispose in CSharp7_2

    public partial void Dispose();
}

public partial class ResourceHolder4 : IDisposable
{
    public void CleanUp() =>
        this.fs.Dispose(); // Noncompliant. "Dispose" implementation does not dispose fs.

    public partial void Dispose();
}
