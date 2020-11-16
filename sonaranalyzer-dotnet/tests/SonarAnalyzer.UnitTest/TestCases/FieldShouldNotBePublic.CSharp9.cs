public record Sample
{
    public readonly double Pi3 = 3.14;  // Noncompliant {{Make 'Pi3' private.}}

    public double Pi4 = 3.14;           // Noncompliant
    private double Pi5 = 3.14;
    protected double Pi6 = 3.14;
    internal double Pi7 = 3.14;

    public record InnerRecordPublic
    {
        public double Pi4 = 3.14;       // Noncompliant
        private double Pi5 = 3.14;
    }

    private record InnerRecordPrivate
    {
        public double Pi4 = 3.14;       // Compliant in private type
        private double Pi5 = 3.14;
    }
}

public class OuterClass
{
    public record InnerRecord
    {
        public double Pi4 = 3.14; // Noncompliant
        private double Pi5 = 3.14;
    }
}
