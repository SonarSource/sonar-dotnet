   partial Function() => null; // Noncompliant {{Return types named 'partial' should be escaped with '@'}}
// ^^^^^^^

global::partial Global() => null; // Noncompliant
//      ^^^^^^^

global::partial GlobalGeneric<T>() => null;

@partial Function2() => null;

global::@partial Global2() => null;

partial GenericFunction<T>() => null;

partial<int> GenericFunction2() => null;

class Noncompliant
{
    global::partial Method() => null; // Noncompliant
    //      ^^^^^^^
}

class Compliant
{
    global::@partial Method() => null;
}

class partial { }

class partial<T> { }
