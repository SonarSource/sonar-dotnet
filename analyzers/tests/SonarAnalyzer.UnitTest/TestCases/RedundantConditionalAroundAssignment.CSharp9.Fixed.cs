int x = 5;
int y = 6;

y = 5;

y = 5;

y = 5;

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
            x = value;
        }
    }

    string CompliantProperty
    {
        init { x = value; }
    }
}
