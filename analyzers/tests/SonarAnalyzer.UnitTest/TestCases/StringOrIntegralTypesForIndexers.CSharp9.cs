record Foo
{
    public int this[Class index] { get { return 0; } }
//                  ^^^^^ Noncompliant {{Use string, integral, index or a range type here, or refactor this indexer into a method.}}

    public int this[Record index] { get { return 0; } } // Compliant - FN

    public int this[Range range] { get { return new int[1]; } }

    public int this[Index index] { get { return 0; } }

    public int this[int index] { get { return 0; } }

    public int this[string index] { get { return 0; } }

    public int this[nint index] { get { return 0; } }

    public int this[nuint index] { get { return 0; } }
}

class Class { }
record Record { }

