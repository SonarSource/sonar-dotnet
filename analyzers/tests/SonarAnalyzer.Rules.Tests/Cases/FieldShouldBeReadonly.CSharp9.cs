record Person
{
    private int birthYear;     // Noncompliant {{Make 'birthYear' 'readonly'.}}
    int birthMonth = 3;        // Noncompliant

    int legSize1 = 3;
    int legSize2 = 3;
    bool usedInInit = false;    // FN, only written in property init

    Person(int birthYear)
    {
        this.birthYear = birthYear;
    }

    public int LegSize
    {
        get
        {
            legSize2++;
            return legSize1;
        }
        init
        {
            legSize1 = value;
            usedInInit = true;
        }
    }
}
