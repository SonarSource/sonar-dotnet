using System.Text;

StringBuilder builder1 = new(); // Noncompliant
StringBuilder builder2 = new(); // Compliant
builder1.Append(builder2.ToString());

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
