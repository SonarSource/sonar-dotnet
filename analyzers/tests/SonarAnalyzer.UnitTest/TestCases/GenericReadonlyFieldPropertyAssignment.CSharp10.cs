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
        (point.X, int y) = p; // Compliant - FN: if point is a struct, then nothing happened
    }
}
