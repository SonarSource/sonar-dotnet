// https://github.com/SonarSource/sonar-dotnet/issues/8024
public sealed record Line(decimal Field);

public sealed class Repro
{
    public Line[] GetLines() => CreateLines();

    private static Line[] CreateLines() => [new(0)];
}
