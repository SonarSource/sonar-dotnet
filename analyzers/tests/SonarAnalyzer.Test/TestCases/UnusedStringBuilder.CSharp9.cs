using System.Text;

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

        StringBuilder builderInLine1 = new(), builderInLine2 = new();
//                    ^^^^^^^^^^^^^^^^^^^^^^                            Noncompliant
//                                            ^^^^^^^^^^^^^^^^^^^^^^    Noncompliant@-1

        static void LocalStaticMethod()
        {
            StringBuilder builder2 = new StringBuilder(); // Compliant
            var a = builder2.Length;
        }
    }
}
