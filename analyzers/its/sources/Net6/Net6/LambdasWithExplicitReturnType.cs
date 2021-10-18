namespace Net6
{
    internal class LambdasWithExplicitReturnType
    {
        public void Example()
        {
            // Example for both explicit return type and infer a natural delegate type for lambdas and method groups
            var f = byte () => 5;
        }
    }
}
