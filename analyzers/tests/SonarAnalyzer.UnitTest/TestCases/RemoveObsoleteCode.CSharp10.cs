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
