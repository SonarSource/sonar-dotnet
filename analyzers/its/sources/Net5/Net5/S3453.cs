namespace Net5
{
    public class S3453
    {
        public class Foo
        {
            public static readonly Foo Instance = new();

            public bool IsActive => true;

            private Foo() { }
        }

        public class Bar
        {
            public static readonly BarInner Instance = new ();

            public bool IsActive => true;

            private Bar() { }

            public class BarInner { }
        }
    }
}
