public class SomeClass
{
    public void SomeMethod(object[] objects)
    {
        if (objects[0] is not null && objects is [not null]) // FN
        {
        }
    }
}
