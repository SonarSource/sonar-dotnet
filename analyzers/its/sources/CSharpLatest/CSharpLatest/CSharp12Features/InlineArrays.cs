namespace CSharpLatest.CSharp12Features;

internal class InlineArrays
{
    [System.Runtime.CompilerServices.InlineArray(10)]
    public struct Buffer
    {
        private int _element0;
    }

    void Example()
    {
        var buffer = new Buffer();
        for (int i = 0; i < 10; i++)
        {
            buffer[i] = i;
        }

        foreach (var i in buffer)
        {
            Console.WriteLine(i);
        }
    }
}
