namespace S1848.ObjectCreatedDropped
{
    class Noncompliant
    {
        void CreatedOnly()
        {
            new SomeRecord(); // Noncompliant
        }

        record SomeRecord();
    }
}
