var x = 1;

public record GlobalRecord { }

namespace MyLibrary
{
    public record Foo { }
}
namespace Helpers
{
    public static class MyExtensions
    {
        public static void Bar(this MyLibrary.Foo foo) { }
        public static void Bar(this GlobalRecord foo) { }
    }
}
namespace Same
{
    public record R { }
    public static class Extensions
    {
        public static void Bar(this R r) {  } // Noncompliant
    }
}
