class CSharp14
{
    int target = -5;
    int num = 3;

    public void NullConditionalAssignment(CSharp14 sample)
    {
        sample?.target =- num;  // Noncompliant
        sample?.target =+ num;  // Noncompliant
    }
}
