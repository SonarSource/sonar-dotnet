public class Sample
{
    public void EscapedParameter
        (int @default) // Needs to be on a separate line to have a simple test scaffolding
    {
        int x = @default;
        @default = 42;
    }
}
