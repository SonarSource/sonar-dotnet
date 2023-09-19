using System;

namespace TestCases; // Referenced in RazorTokens.razor

public class ToDo : IDisposable
{
    public string? Title { get; set; }
    public bool IsDone { get; set; }

    public void Dispose() { }
}
