using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

FileStream fs = new FileStream("", FileMode.Open); // Compliant, not a class member

public class ResourceHolder   // Noncompliant {{Implement 'IDisposable' in this class and use the 'Dispose' method to call 'Dispose' on 'fs'.}}
//           ^^^^^^^^^^^^^^
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
}

public record RecordInit
{
    protected FileStream fs;
    public FileStream FsProp
    {
        init
        {
            FsProp = new FileStream("", FileMode.Open);
        }
    }
}

public record RecordConstruct // Noncompliant {{Implement 'IDisposable' in this class and use the 'Dispose' method to call 'Dispose' on 'fs'.}}
{
    protected FileStream fs;
    public RecordConstruct()
    {
        fs = new FileStream("", FileMode.Open);
    }
}

public record RecordDispose : IDisposable
{
    protected FileStream fs;
    public RecordDispose()
    {
        fs = new FileStream("", FileMode.Open);
    }
    public void Dispose()
    {
        this.fs.Dispose();
    }
}
