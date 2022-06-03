int i = 1; i++;
i++;
i++;
i++;

void LocalFunction() // Noncompliant {{This local function has 4 lines, which is greater than the 2 lines authorized.}}
{
    i++;
    i++;
    i++;
    i++;
}

static void StaticLocalFunction() // Noncompliant {{This static local function has 4 lines, which is greater than the 2 lines authorized.}}
{
    int k = 1;
    k++;
    k++;
    k++;
}

void Compliant()
{
    i++;
    i++;
}

void ABitLonger() // Noncompliant {{This local function has 3 lines, which is greater than the 2 lines authorized.}}
{
    i++;
    i++;
    i++;
}

int Lambda(int a, int b, int c) => // Noncompliant {{This local function has 3 lines, which is greater than the 2 lines authorized.}}
    a
    + b
    + c;

record Sample
{
    public Sample() // Noncompliant {{This constructor 'Sample' has 4 lines, which is greater than the 2 lines authorized. Split it into smaller methods.}}
    {
        int i = 1; i++;
        i++;
        i++;
        i++;
    }

    ~Sample() // Noncompliant {{This finalizer '~Sample' has 4 lines, which is greater than the 2 lines authorized. Split it into smaller methods.}}
    {
        int i = 1; i++;
        i++;
        i++;
        i++;
    }

    public void Method_01() // Noncompliant {{This method 'Method_01' has 3 lines, which is greater than the 2 lines authorized. Split it into smaller methods.}}
    {
        int i = 1; i++;
        i++;
        i++;
    }

    public int Method_02()
    {
        int i = 1;
        return 1;
    }

    public void WithLocalFunction() // Noncompliant
    {
        LocalFunction();
        void LocalFunction()
        {
            var i = 1; i++;
            i++;
            i++;
            i++;
        }
    }
}
