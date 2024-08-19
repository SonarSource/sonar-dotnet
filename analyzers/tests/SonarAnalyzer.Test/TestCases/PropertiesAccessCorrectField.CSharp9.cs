public record Record
{
    private object w;
    private object x;
    private object y;
    private object z;

    public object X
    {
        get { return x; }
        set { x ??= value; }
    }

    public object W
    {
        get { return w; }
        init { w ??= value; }
    }

    public object Y
    {
        get { return x; }  // Noncompliant {{Refactor this getter so that it actually refers to the field 'y'.}}
//                   ^
        set { x ??= value; } // Noncompliant {{Refactor this setter so that it actually refers to the field 'y'.}}
//            ^
    }

    public object Z
    {
        get { return x; }  // Noncompliant {{Refactor this getter so that it actually refers to the field 'z'.}}
//                   ^
        init { x ??= value; } // Noncompliant {{Refactor this setter so that it actually refers to the field 'z'.}}
//             ^
    }
}
