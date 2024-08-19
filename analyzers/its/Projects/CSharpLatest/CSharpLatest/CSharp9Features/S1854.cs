namespace CSharpLatest.CSharp9Features;

public class S1854
{
    public int InitProperty
    {
        init
        {
            value = 1;
            int a = 100;
            a = 2;
        }
    }
}
