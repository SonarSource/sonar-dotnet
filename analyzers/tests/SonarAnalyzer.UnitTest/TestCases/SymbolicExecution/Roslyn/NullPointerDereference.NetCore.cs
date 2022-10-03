using System;

class NullPointerDereference
{
    public void Type_IsAssignableFrom_LearnsPossibleNull(Type type, Type other)
    {
        if (other.IsAssignableFrom(type))   // Decorated with [NotNullWhenAttribute]
        {
            type.ToString();
        }
        else
        {
            type.ToString();    // Noncompliant, could be unassignable because it was null
        }
    }
}

