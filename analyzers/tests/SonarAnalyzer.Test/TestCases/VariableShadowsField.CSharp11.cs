public class SomeClass
{
    private byte[] somefield;

    public void SomeMethod(byte[] byteArray)
    {
        if (byteArray is [1, 2, 3] somefield) // Noncompliant {{Rename 'somefield' which hides the field with the same name.}}
        {
        }
    }
}
