class LambdaInLoopInMethod
{
    void Method()
    {
        for (int i = 0; i < 10; i++)
        {
            var a = () => { }; // Compliant - Not in blazor
        }
    }
}
