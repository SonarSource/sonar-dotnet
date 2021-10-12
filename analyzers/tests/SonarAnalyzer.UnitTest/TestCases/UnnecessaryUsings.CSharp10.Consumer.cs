// global usings declared in UnnecessaryUsings.CSharp10.Global.cs

class TestConsumingGlobalUsings
{
    static void Main() { }
    public double Size(IList list) => list.Count;
    public void Write(string s) => Console.WriteLine(s);
}
