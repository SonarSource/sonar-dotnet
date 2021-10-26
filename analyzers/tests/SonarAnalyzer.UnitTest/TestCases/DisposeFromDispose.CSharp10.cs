using System;
using System.Collections.Generic;
using System.IO;

FileStream fs; // top level statement

void CleanUp()
{
    fs.Dispose();
}

record struct R1 : IDisposable
{
    private FileStream fs;

    public void OpenResource(string path) => this.fs = new FileStream(path, FileMode.Open);
    public void CloseResource() => this.fs.Close();
    public void CleanUp() => this.fs.Dispose(); // Noncompliant {{Move this 'Dispose' call into this class' own 'Dispose' method.}}
    public void Dispose() { }
}


public record struct PositionalRecordStruct(FileStream fs) : IDisposable
{
    public void CloseResource() => this.fs.Close();
    public void CleanUp() => this.fs.Dispose(); // FN
    public void Dispose() { }
}
