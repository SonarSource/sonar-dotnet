record Foo
{
    public int this[Class index] { get { return 0; } }
//                  ^^^^^ Noncompliant {{Use string or an integral type here, or refactor this indexer into a method.}}

    public int this[Record index] { get { return 0; } }

    public int this[int index] { get { return 0; } }

    public int this[string index] { get { return 0; } }

    public int this[nint index] { get { return 0; } }

    public int this[nuint index] { get { return 0; } }
}

class Class { }
record Record { }
