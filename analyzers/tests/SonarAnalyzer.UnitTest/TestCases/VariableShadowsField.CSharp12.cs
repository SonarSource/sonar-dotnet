class MyClass(int first, int second, int third)
{
    private int first; // Compliant

    void MyMethod()
    {
        int second = 1; // Compliant

        if (first is object third) // Compliant
        {
            return;
        }
    }
}
