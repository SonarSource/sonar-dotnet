// https://github.com/SonarSource/sonar-dotnet/issues/8024
public sealed record Line(decimal Field);

public sealed class Repro
{
    public Line[] GetLines() => CreateLines();

    private static Line[] CreateLines() => [new(0)];
}

// https://github.com/SonarSource/sonar-dotnet/issues/9652
class Repro_9652
{
    class Inner
    {
        public object? this[int test] => 0;
        public int this[int x, int y] => 0;
        public int this[int x, int y, int z] => 0;  // Noncompliant
        public int this[string a, string b] => 0;
        private int this[string test] => 0;
        private int this[double test] => 0;         // Noncompliant

        public int Method() => _ = this[""];
    }

    class AnotherInner
    {
        private static Inner inner = new();
        public int Method() => inner["a", "b"];
    }

    private int this[int test] => 0;

    public Repro_9652()
    {
        var inner = new Inner();
        _ = inner[0];
        _ = inner?[0];
        _ = inner![0];
        _ = inner!?[0];
        _ = inner[0].ToString();
        _ = inner?[0].ToString();
        _ = inner[0]?.ToString();
        _ = inner?[0]?.ToString();
        _ = inner![0].ToString();
        _ = inner[0]!.ToString();
        _ = inner![0]!.ToString();
        _ = inner!?[0]!?.ToString();
        _ = inner[0, 0];
        _ = this[0];
        _ = inner.Method();

        var anotherInner = new AnotherInner();
        _ = anotherInner.Method();
    }
}
