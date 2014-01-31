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
