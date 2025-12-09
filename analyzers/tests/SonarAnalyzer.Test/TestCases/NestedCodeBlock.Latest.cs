public class FieldKeyword
{
    public string Name
    {
        get
        {
            { // Noncompliant
                return field;
            }
        }
        set;
    }
}
