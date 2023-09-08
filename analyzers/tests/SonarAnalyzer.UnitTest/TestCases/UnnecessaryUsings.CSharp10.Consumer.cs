// global usings declared in UnnecessaryUsings.CSharp10.Global.cs

global using System.Globalization; // FN - is not used in any file

using System.Text; // Noncompliant

class TestConsumingGlobalUsings
{
    static void Main() { }
    public double Size(IList list) => list.Count;
    public void Write(string s) => Console.WriteLine(s);
}
