interface IPoint
{
    int X { get; set; }

    int Y { get; set; }
}

partial record PointManager<T> where T : IPoint
{
    public partial PointManager(T point);   // Partial constructor
}

partial record PointManager<T> where T : IPoint
{
    public readonly T point;  // this could be a struct

    public partial PointManager(T point) => this.point = point;

    public void MovePointVertically(int newX)
    {
        point.X = newX; // Noncompliant - if point is a struct, then nothing happened
    }

    public int X
    {
        get => point.X;
        set => point.X = value; // Noncompliant
    }

    public void FixByRemove()
    {
        point.X = 1; // Noncompliant. Needed for the fixer to remove at least one non-compliant diagnostic.
    }

    public void TupleAssignment((int, int) p)
    {
        (point.X, int y) = p; // Noncompliant {{Restrict 'point' to be a reference type or remove this assignment of 'X'; it is useless if 'point' is a value type.}}
//       ^^^^^^^
    }

    public void NestedTuples((int, int) p)
    {
        var tuple1 = ((p, 1), 2);
        (((point.X, _), _), _) = tuple1; // Noncompliant
        var tuple2 = ((1, p), 2);
        ((_, (_, point.Y)), _) = tuple2; // Noncompliant
    }

    public void CompoundBitshift(int newX)
    {
        point.X >>>= 4; // Noncompliant
    }

    public void NullConditionalAssignment()
    {
        point?.Y = 42; // FN
    }

    public T FieldKeyword
    {
        get => field;
        set => field.X = value.X; // FN
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
