int x = 5;
int y = 6;

y = y switch
{
    not 5 => 5 // Noncompliant
};

y = y switch
{
    5 => 5 // Noncompliant
};

y = y switch
{
    5 when x == 5 => 5 // Noncompliant
};

y = y switch
{
    5 => 5, // Noncompliant
    6 => 6  // Noncompliant
};

y = y switch
{
    not 5 when x == 5 => 5
};

y = y switch
{
    5 => 6
};

x = y switch
{
    5 => 6
};

y = y switch
{
    4 => 4,      // Noncompliant
    not 5 => 5,
};

y = y switch
{
    5 => 6,
    _ => y
};

y = y switch
{
    _ => y       // Noncompliant
};

y = y switch
{
    4 => 4,      // Noncompliant
    not x => 5,  // Error [CS0150]
};

int z = y switch
{
    not 5 => 5
};

if ((x,y) is (1,2))   // Compliant FN
{
    x = 1;
    y = 2;
}

record Record
{
    string x;

    string NoncompliantProperty
    {
        init
        {
            if (x != value) // Noncompliant
            {
                x = value;
            }
        }
    }

    string CompliantProperty
    {
        init { x = value; }
    }
}
