namespace RazorClassLib.S6802;

public class LambdaInLoopInMethod
{
    public void Method()
    {
        for (int i = 0; i < 10; i++)
        {
            var a = () => { }; // Compliant - Not in blazor
        }
    }
}
