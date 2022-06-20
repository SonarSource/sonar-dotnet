interface IPoint
{
    int X { get; set; }
    int Y { get; set; }
}

class PointManager<T> where T : IPoint
{
    readonly T point;

    public PointManager(T point)
    {
        this.point = point;
    }

    public void MovePointVertically((int, int) p)
    {
        (point.X, int y) = p; // Noncompliant {{Restrict 'point' to be a reference type or remove this assignment of 'X'; it is useless if 'point' is a value type.}}
//       ^^^^^^^
    }
}
