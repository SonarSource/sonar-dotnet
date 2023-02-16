using System;
using System.IO;

public partial class ResourceHolder : IDisposable
{
    private FileStream fs;

    public void OpenResource(string path) =>
        this.fs = new FileStream(path, FileMode.Open);

    public partial void CleanUp();

    public partial void Dispose();
}

public partial class ResourceHolder2 : IDisposable
{
    private FileStream fs;

    public void OpenResource(string path) =>
        this.fs = new FileStream(path, FileMode.Open);

    public partial void CleanUp();

    public partial void Dispose();
}

public partial class ResourceHolder3 : IDisposable
{
    private FileStream fs;

    public void OpenResource(string path) =>
        this.fs = new FileStream(path, FileMode.Open);

    public partial void CleanUp();

    public partial void Dispose() =>
        this.fs.Dispose();
}
