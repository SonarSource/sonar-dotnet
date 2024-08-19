using System;

class NullPointerDereference
{
    public void Type_IsAssignableFrom_LearnsPossibleNull(Type type, Type other)
    {
        if (other.IsAssignableFrom(type))   // Not decorated with [NotNullWhenAttribute] under .NET Fx
        {
            type.ToString();
        }
        else
        {
            type.ToString();    // Compliant, we didn't learn constraints
        }
    }
}

