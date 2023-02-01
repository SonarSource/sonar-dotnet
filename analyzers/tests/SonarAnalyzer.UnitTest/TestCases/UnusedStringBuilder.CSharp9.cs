using System.Text;

StringBuilder builder1 = new(); // Compliant
StringBuilder builder2 = new(); // FN (Top level statement not covered yet)
builder2.Append(builder1.ToString());

public class MyClass
{
    public void MyMethod(StringBuilder builder) // Compliant
    {
        StringBuilder builder1 = new(); // Compliant

        StringBuilder builder2 = new(); // Noncompliant
//                    ^^^^^^^^^^^^^^^^

        StringBuilder builder3 = new(); // Noncompliant
        builder3.Append(builder1.ToString());

        (StringBuilder, StringBuilder) builder4 = (new(), new()); // FN
    }
}
