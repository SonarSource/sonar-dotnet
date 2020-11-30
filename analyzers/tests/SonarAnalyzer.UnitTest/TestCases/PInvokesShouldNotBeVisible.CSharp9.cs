using System.Runtime.InteropServices;

public record Record
{
    public void Method()
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool RemoveDirectory(string name); // Compliant - Method is not publicly exposed
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern bool RemoveDirectory(string name);  // Noncompliant
}
