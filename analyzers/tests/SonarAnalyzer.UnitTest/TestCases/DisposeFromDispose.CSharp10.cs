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

record struct R2 : IDisposable
{
    private FileStream Fs { get; set; }

    public void OpenResource(string path) => this.Fs = new FileStream(path, FileMode.Open);
    public void CloseResource() => this.Fs.Close();
    public void CleanUp() => this.Fs.Dispose(); // FN. Properties are not considered.
    public void Dispose() { }
}

public record struct PositionalRecordStruct(FileStream fs) : IDisposable
{
    public void CloseResource() => this.fs.Close();
    public void CleanUp() => this.fs.Dispose(); // Compliant. FileStream fs is a public property
    public void Dispose() { }
}
