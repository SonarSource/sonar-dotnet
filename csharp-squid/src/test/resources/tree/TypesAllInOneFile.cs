namespace Foo
{
    class Class
    {
        struct InnerStruct { }
    }

    protected struct Struct
    {
        class InnerClass { }
    }

    private enum Enum
    {
        //
    }
}

namespace Bar
{
    interface Interface
    {

    }

    public delegate void Delegate(object P);
}

namespace Foo.Bar
{
    namespace Baz
    {
        public class Class1
        {
            public class Class2
            {
                public class Class3
                {
                }
            }
        }
    }
}

namespace Foo
{
    public class Class4
    {
    }
}

public class GenericClass
{
}

public class GenericClass<T>
{
}

public class GenericClass<T1, T2>
{
}

public class GenericClass<T1, T2, T3, T4, T5>
{
}
