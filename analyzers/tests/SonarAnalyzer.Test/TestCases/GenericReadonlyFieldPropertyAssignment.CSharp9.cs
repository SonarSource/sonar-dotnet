interface IPoint
{
    int X { get; set; }

    int Y { get; set; }
}

record PointManager<T> where T : IPoint
{
    public readonly T point;  // this could be a struct

    public PointManager(T point) => this.point = point;

    public void MovePointVertically(int newX)
    {
        point.X = newX; // Noncompliant - if point is a struct, then nothing happened
    }

    public int X
    {
        get => point.X;
        set => point.X = value; // Noncompliant
    }
}

record PointManagerCompliant<T> where T : class, IPoint
{
    public readonly T point;

    public PointManagerCompliant(T point) => this.point = point;

    public void MovePointVertically(int newX)
    {
        point.X = newX; // Compliant
    }

    public int X
    {
        get => point.X;
        set => point.X = value; // Compliant
    }
}
