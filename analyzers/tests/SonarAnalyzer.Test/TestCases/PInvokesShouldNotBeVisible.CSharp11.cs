using System.Runtime.InteropServices;

public interface IInterface
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    static extern virtual bool RemoveDirectory1(string name); // Noncompliant

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern virtual bool RemoveDirectory2(string name);  // Noncompliant
}
