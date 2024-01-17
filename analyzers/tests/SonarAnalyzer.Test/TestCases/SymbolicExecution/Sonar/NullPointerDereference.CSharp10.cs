using System;
using System.Text;

public class Sample
{
    private string field;

    public void Examples()
    {
        StringBuilder sb;

        (sb, int a) = (null, 42);
        sb.ToString(); // FN
    }
}
