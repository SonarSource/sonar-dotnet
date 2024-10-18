using System;

[ObsoleteAttribute()] // Noncompliant
void Local()
{

}

[Obsolete] // Noncompliant
record R
{
    void M()
    {
        [Obsolete] // Noncompliant
        void Local()
        {
        }
    }
}

partial class PartialProperties
{
    [Obsolete] // Noncompliant
    partial int Value { get; set; }

    [Obsolete] // Noncompliant
    partial int this[int x] { get; set; }
}

partial class PartialProperties
{
    partial int Value { get => 42; set { } }

    partial int this[int x] { get => 42; set { } }
}
