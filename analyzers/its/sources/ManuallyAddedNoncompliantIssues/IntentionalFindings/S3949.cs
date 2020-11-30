public class IfConditionalAlwaysTrueOrFalse
{
    public void PositiveOverflow() {
        int i = 2147483600;
        i +=100; // Noncompliant (S3949) {{There is a path on which this operation always overflows}}
    }

    public void NegativeOverflow() {
        int i = -2147483600;
        i -=100; // Noncompliant (S3949) {{There is a path on which this operation always overflows}}
    }
}
