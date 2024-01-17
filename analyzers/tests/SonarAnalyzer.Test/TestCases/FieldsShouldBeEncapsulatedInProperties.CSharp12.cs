public class PublicClass(int myValue, int notMyValue)
{
    public int myValue = 42; // Noncompliant {{Make this field 'private' and encapsulate it in a 'public' property.}}
}
