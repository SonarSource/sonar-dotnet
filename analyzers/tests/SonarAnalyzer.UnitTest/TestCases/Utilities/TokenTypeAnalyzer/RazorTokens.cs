using System;

namespace TestCases;

public class ToDo : IDisposable
{
    public string? Title { get; set; }
    public bool IsDone { get; set; }

    public void Dispose() { }
}
