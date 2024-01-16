using System;
using System.IO;

public partial class ResourceHolder : IDisposable
{
    private FileStream fs;

    public partial void CleanUp();

    public partial void Dispose();
}

public partial class ResourceHolder2 : IDisposable
{
    private FileStream fs;

    public void Dispose() =>
        this.fs.Dispose();
}

public partial class ResourceHolder3 : IDisposable
{
    private FileStream fs;

    public partial void CleanUp();

    public partial void Dispose() =>
        this.fs.Dispose();
}

public partial class ResourceHolder4 : IDisposable
{
    private FileStream fs;

    public partial void Dispose() { }
}
