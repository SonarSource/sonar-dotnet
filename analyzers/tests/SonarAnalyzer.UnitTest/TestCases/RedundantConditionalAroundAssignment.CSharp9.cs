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
    4 => 4,
    not 5 => 5,
};

y = y switch
{
    4 => 4,
    not x => 5,  // Error [CS0150]
};

int z = y switch
{
    not 5 => 5
};

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
