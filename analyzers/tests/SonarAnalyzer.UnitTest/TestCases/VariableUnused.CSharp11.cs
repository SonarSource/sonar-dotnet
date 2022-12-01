public class SomeClass
{
    public void SomeMethod(object[] byteArray)
    {
        if (byteArray is [1, 2, 3] unusedVar) // Noncompliant
        {
        }

        if (byteArray is [SomeClass unusdeVar2, 42]) // Noncompliant
        {
        }
    }
}
